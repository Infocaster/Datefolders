using Infocaster.Umbraco.DateFolders.Extensions;
using Infocaster.Umbraco.DateFolders.Helpers;
using Infocaster.Umbraco.DateFolders.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Infocaster.Umbraco.DateFolders.Composers.NotificatonHandlers
{
    /// <summary>
    /// Item saved in Umbraco. Create parent folders for content and clean up empty ones.
    /// </summary>
    public class DateFoldersContentSavedNotification : INotificationHandler<ContentSavedNotification>
    {
        private readonly ILogger<ContentSavedNotification> _logger;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        private DateFoldersConfigBase _options;

        public DateFoldersContentSavedNotification(IOptions<DateFoldersConfigBase> options, ILogger<ContentSavedNotification> logger, IContentService contentService, IContentTypeService contentTypeService)
        {
            _options = options.Value;
            _logger = logger;
            _contentService = contentService;
            _contentTypeService = contentTypeService;
        }

        public void Handle(ContentSavedNotification notification)
        {
            foreach (IContent savedContent in notification.SavedEntities)
            {
                // Fix for 'cannot save non-current version' error: https://our.umbraco.com/forum/using-umbraco-and-getting-started/99320-cannot-save-a-non-current-version
                // Error occurs when no datefolders available yet and multiple items are moved into datefolders
                IContent content = _contentService.GetById(savedContent.Id);

                if (!_options.ItemDocTypes.Contains(content.ContentType.Alias)) continue;

                if (!ParentValid(content)) continue;

                IContentType folderDocType = _contentTypeService.Get(_options.FolderDocType);
                if (folderDocType is null)
                {
                    // Date folder doctype is null
                    _logger.LogError("The date folder document type '{folderDocType}' does not exist", _options.FolderDocType);
                    continue;
                }

                if (content.ParentId == default)
                {
                    // Item is created under 'Content' root, which is unsupported
                    _logger.LogError("Creating a date folder item under 'Content' root is unsupported");
                    continue;
                }

                var date = GetItemDate(content, _options.ItemDateProperty);
                IContent parent = _contentService.GetById(content.ParentId);

                IContent monthFolder = null;
                IContent yearFolder = null;
                IContent dayFolder = null;

                bool dayChanged = false;
                bool monthChanged;
                bool yearChanged;

                // Item already has datefolder as parent
                if (parent.ContentType.Alias.Equals(_options.FolderDocType))
                {
                    monthFolder = parent;

                    if (_options.CreateDayFolders)
                    {
                        dayFolder = parent;
                        monthFolder = _contentService.GetById(dayFolder.ParentId);

                        dayChanged = date.Day.ToString("00") != dayFolder.Name;
                    }

                    yearFolder = _contentService.GetById(monthFolder.ParentId);

                    // Set item parent to source folder which contains the datefolders for sorting
                    parent = _contentService.GetById(yearFolder.ParentId);

                    yearChanged = date.Year.ToString() != yearFolder.Name;
                    monthChanged = date.Month.ToString("00") != monthFolder.Name;
                }
                else
                {
                    dayChanged = true;
                    monthChanged = true;
                    yearChanged = true;
                }

                if (yearChanged || monthChanged || dayChanged)
                {
                    IContent newDayFolder = null;

                    IContent newYearFolder = yearChanged || yearFolder is null ? GetOrCreateAndPublishDateFolder(_contentService, parent, date.Year.ToString(), content.CreatorId) : yearFolder;
                    IContent newMonthFolder = GetOrCreateAndPublishDateFolder(_contentService, newYearFolder, date.Month.ToString("00"), content.CreatorId);
                    IContent orderParent = newMonthFolder;

                    // Move content to correct folder
                    if (_options.CreateDayFolders)
                    {
                        newDayFolder = GetOrCreateAndPublishDateFolder(_contentService, newMonthFolder, date.Day.ToString("00"), content.CreatorId);
                        _contentService.Move(content, newDayFolder.Id);
                        orderParent = newDayFolder;
                    }
                    else
                    {
                        _contentService.Move(content, newMonthFolder.Id);
                    }

                    // Todo: Check if needed
                    if (content.Published)
                    {
                        _contentService.Save(content);
                        _contentService.Publish(content, ["*"]);
                    }

                    // Clean up old folders if empty
                    if (dayFolder is not null)
                    {
                        ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, dayFolder, _contentService);
                    }

                    if (monthFolder is not null)
                    {
                        ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, monthFolder, _contentService);
                    }

                    if (yearFolder is not null)
                    {
                        ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, yearFolder, _contentService);
                    }

                    // Sort all content in folders by date
                    OrderChildrenByDateProperty(orderParent, _options.OrderByDescending, !string.IsNullOrEmpty(_options.ItemDateProperty) ? _options.ItemDateProperty : null);

                    // Sort all folders by name
                    OrderChildrenByName(parent, _options.OrderByDescending);
                    OrderChildrenByName(newYearFolder, _options.OrderByDescending);
                    if (_options.CreateDayFolders)
                    {
                        OrderChildrenByName(newMonthFolder, _options.OrderByDescending);
                    }
                }                
            }
        }

        /// <summary>
        /// Returns itemDataProperty or Umbraco CreateDate of passed content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        private static DateTime GetItemDate(IContentBase content, string propertyAlias)
        {
            if (string.IsNullOrEmpty(propertyAlias)) return content.CreateDate;

            if (content.HasProperty(propertyAlias))
            {
                DateTime propertyDate = content.GetValue<DateTime>(propertyAlias);
                return propertyDate == DateTime.MinValue ? content.CreateDate : propertyDate;
            }
            else
            {
                return content.CreateDate;
            }
        }

        /// <summary>
        /// Gets DateFolder by name if exists, if it not exists than create folder with specified name.
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="parent"></param>
        /// <param name="nodeName"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        private IContent GetOrCreateAndPublishDateFolder(IContentService contentService, IContent parent, string nodeName, int currentUserId)
        {
            IContent content = null;
            var parentChildren = parent.GetAllChildren(_contentService);

            // Get first child of FolderDocType if it exists
            if (parentChildren.Any())
            {
                content = parentChildren.FirstOrDefault(x => x.ContentType.Alias.Equals(_options.FolderDocType) && x.Name.Equals(nodeName));
            }

            // Create folder if if non exists
            if (content is null)
            {
                content = _contentService.Create(nodeName, parent.Key, _options.FolderDocType, currentUserId);
            }

            if (!content.Published)
            {
                contentService.Save(content, userId: currentUserId);
                contentService.Publish(content, ["*"], userId: currentUserId); // TODO: 'Only wildcard culture is supported when publishing invariant content types. (Parameter 'cultures')'
                //contentService.Publish(content, [Thread.CurrentThread.CurrentCulture.IetfLanguageTag], userId: currentUserId);
            }
            return content;
        }

        /// <summary>
        /// Orders children of IContent by name (if name is parsable to number) and adds remaining children after sorted folders
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="orderByDesc"></param>
        private void OrderChildrenByName(IContent parent, bool orderByDesc)
        {
            try
            {
                var allChildren = parent.GetAllChildren(_contentService);

                var orderedChildren = (orderByDesc
                    ? allChildren.Where(c => int.TryParse(c.Name, out int i)).OrderByDescending(x => int.Parse(x.Name))
                    : allChildren.Where(c => int.TryParse(c.Name, out int i)).OrderBy(x => int.Parse(x.Name))).ToList();

                orderedChildren.AddRange(allChildren.Where(c => !int.TryParse(c.Name, out int i)));

                _contentService.Sort(orderedChildren);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DateFolders OrderChildrenByName exception");
            }
        }

        /// <summary>
        /// Order children of Icontent by provided date field alias. If not provided, order by CreateDate.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="orderByDesc"></param>
        /// <param name="propertyAlias"></param>
        private void OrderChildrenByDateProperty(IContent parent, bool orderByDesc, string propertyAlias = "")
        {
            try
            {
                var allChildren = parent.GetAllChildren(_contentService);

                if (!string.IsNullOrEmpty(propertyAlias))
                {
                    var orderedChildren = (orderByDesc
                        ? allChildren.Where(c => c.HasProperty(propertyAlias)).OrderByDescending(c => GetItemDate(c, propertyAlias))
                        : allChildren.Where(c => c.HasProperty(propertyAlias)).OrderBy(c => GetItemDate(c, propertyAlias))).ToList();

                    orderedChildren.AddRange(allChildren.Where(c => !c.HasProperty(propertyAlias)));
                    _contentService.Sort(orderedChildren);
                }
                else
                {
                    var orderedChildren = orderByDesc ? allChildren.OrderByDescending(x => x.CreateDate) : allChildren.OrderBy(x => x.CreateDate);
                    _contentService.Sort(orderedChildren);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DateFolders OrderChildrenByDateProperty exception");
            }
        }

        private bool ParentValid(IContent content)
        {
            if (!_options.AllowedParentIds.Any() && !_options.AllowedParentDocTypes.Any()) return true;

            IContent parentContentItem = _contentService.GetAncestors(content).Reverse().FirstOrDefault(x => !x.ContentType.Alias.Equals(_options.FolderDocType));
            if (_options.AllowedParentIds.Any() && _options.AllowedParentIds.Contains(parentContentItem.Key.ToString())) return true;
            if (_options.AllowedParentDocTypes.Any() && _options.AllowedParentDocTypes.Contains(parentContentItem.ContentType.Alias)) return true;

            return false;
        }
    }
}
