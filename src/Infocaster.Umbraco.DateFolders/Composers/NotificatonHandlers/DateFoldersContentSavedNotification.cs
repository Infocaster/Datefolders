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
                IContent content = _contentService.GetById(savedContent.Id);

                if (content is null) continue;
                if (!_options.ItemDocTypes.Contains(content.ContentType.Alias)) continue;

                if (!ParentValid(content)) continue;

                IContentType folderDocType = _contentTypeService.Get(_options.FolderDocType);
                if (folderDocType is null)
                {
                    _logger.LogError("The date folder document type '{folderDocType}' does not exist", _options.FolderDocType);
                    continue;
                }

                if (content.ParentId == default)
                {
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

                bool dayCreated = false;

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
                    (IContent newYearFolder, bool yearCreated) = yearChanged || yearFolder is null ? GetOrCreateAndPublishDateFolder(_contentService, parent, date.Year.ToString(), content.CreatorId) : (yearFolder, false);
                    (IContent newMonthFolder, bool monthCreated) = GetOrCreateAndPublishDateFolder(_contentService, newYearFolder, date.Month.ToString("00"), content.CreatorId);

                    if (_options.CreateDayFolders)
                    {
                        (IContent newDayFolder, dayCreated) = GetOrCreateAndPublishDateFolder(_contentService, newMonthFolder, date.Day.ToString("00"), content.CreatorId);
                        _contentService.Move(content, newDayFolder.Id);
                    }
                    else
                    {
                        _contentService.Move(content, newMonthFolder.Id);
                    }

                    if (content.Published)
                    {
                        _contentService.Save(content);
                        _contentService.Publish(content, ["*"]);
                    }

                    DeleteFolderIfEmpty(dayFolder);
                    DeleteFolderIfEmpty(monthFolder);
                    DeleteFolderIfEmpty(yearFolder);

                    if (yearCreated)
                    {
                        SortChildrenByName(parent, _options.OrderByDescending);
                    }

                    if (monthCreated)
                    {
                        SortChildrenByName(newYearFolder, _options.OrderByDescending);
                    }

                    if (_options.CreateDayFolders && dayCreated)
                    {
                        SortChildrenByName(newMonthFolder, _options.OrderByDescending);
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

            return content.CreateDate;
        }

        /// <summary>
        /// Gets the existing date folder with the given name under <paramref name="parent"/>, or creates it if none exists.
        /// Ensures the folder is published before returning.
        /// </summary>
        /// <remarks>
        /// Matches an existing child by both content type alias (<c>_options.FolderDocType</c>) and name; if no
        /// match is found, a new folder is created. If the resulting folder is not yet published, its key is
        /// recorded in <c>_selfInitiatedSaves</c> immediately before saving, so the notification handler can
        /// recognize the resulting save/publish as self-initiated and skip re-processing it.
        /// </remarks>
        /// <param name="contentService">The content service used to save and publish the date folder.</param>
        /// <param name="parent">The parent content node under which the date folder should exist.</param>
        /// <param name="nodeName">The name of the date folder to find or create (e.g. a year, month, or day segment).</param>
        /// <param name="currentUserId">The id of the user to attribute the create/save/publish actions to.</param>
        /// <returns>A tuple containing the date folder's <see cref="IContent"/> and a <see langword="bool"/> indicating whether it was newly created.</returns>
        private (IContent, bool) GetOrCreateAndPublishDateFolder(IContentService contentService, IContent parent, string nodeName, int currentUserId)
        {
            IContent content = null;
            var created = false;

            var parentChildren = parent.GetAllChildren(_contentService);

            if (parentChildren.Any())
            {
                content = parentChildren.FirstOrDefault(x => x.ContentType.Alias.Equals(_options.FolderDocType) && x.Name.Equals(nodeName));
            }

            if (content is null)
            {
                created = true;
                content = _contentService.Create(nodeName, parent.Key, _options.FolderDocType, currentUserId);
            }

            if (!content.Published)
            {
                //Keep track of content not to retrigger this event for
                contentService.Save(content, userId: currentUserId);
                contentService.Publish(content, ["*"], userId: currentUserId);
            }

            return (content, created);
        }

        /// <summary>
        /// Sorts the children of a content node by name, either ascending or descending.
        /// Optimized to only touch children whose <see cref="IContent.SortOrder"/> would actually change,
        /// rather than resaving every sibling on each pass.
        /// </summary>
        /// <remarks>
        /// When available, <see href="https://apidocs.umbraco.com/v18/csharp/api/Umbraco.Cms.Core.Services.IContentService.html">IContentService.SortChildren</see>
        /// should be used instead, since it can reorder without triggering <see cref="ContentSavedNotification"/> for each item.
        /// </remarks>
        /// <param name="parent">The parent content node whose direct children should be reordered. If <see langword="null"/>, the method returns without doing anything.</param>
        /// <param name="descending">If <see langword="true"/>, children are sorted by parsed name in descending order; otherwise ascending.</param>
        private void SortChildrenByName(IContent parent, bool descending)
        {
            if (parent is null)
                return;

            try
            {
                var children = parent.GetAllChildren(_contentService).ToList();

                var ordered = descending
                    ? children.OrderByDescending(x => ParseName(x.Name)).ToList()
                    : children.OrderBy(x => ParseName(x.Name)).ToList();

                var changed = ordered
                    .Select((child, index) => (child, index))
                    .Where(x => x.child.SortOrder != x.index)
                    .ToList();

                if (changed.Count == 0)
                    return;

                foreach (var (child, index) in changed)
                {
                    child.SortOrder = index;
                }

                _contentService.Save(changed.Select(x => x.child).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DateFolders custom sorting failed");
            }
        }

        private static int ParseName(string name)
        {
            return int.TryParse(name, out var value)
                ? value
                : int.MaxValue;
        }

        // Inlined replacement for the package's internal ContentHelper.DeleteFolderIfEmpty,
        // which is not accessible outside the package's own assembly.
        private void DeleteFolderIfEmpty(IContent folder)
        {
            if (folder is null) return;
            if (folder.ContentType.Alias == _options.FolderDocType && !_contentService.HasChildren(folder.Id))
            {
                _contentService.MoveToRecycleBin(folder);
            }
        }

        private bool ParentValid(IContent content)
        {
            if (!_options.AllowedParentIds.Any() && !_options.AllowedParentDocTypes.Any()) return true;

            IContent parentContentItem = _contentService.GetAncestors(content).Reverse().FirstOrDefault(x => !x.ContentType.Alias.Equals(_options.FolderDocType));
            if (parentContentItem is null) return false;
            if (_options.AllowedParentIds.Any() && _options.AllowedParentIds.Contains(parentContentItem.Key.ToString())) return true;
            if (_options.AllowedParentDocTypes.Any() && _options.AllowedParentDocTypes.Contains(parentContentItem.ContentType.Alias)) return true;

            return false;
        }
    }
}
