using Infocaster.Umbraco.DateFolders.Extensions;
using Infocaster.Umbraco.DateFolders.Helpers;
using Infocaster.Umbraco.DateFolders.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Infocaster.Umbraco.DateFolders.Composers
{
    public class DatefoldersComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.Configure<DateFoldersConfigBase>(builder.Config.GetSection("DateFolders"));
            builder.AddNotificationHandler<ContentMovingToRecycleBinNotification, DateFoldersContentMovingToRecycleBinNotification>();
            builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, DateFoldersContentMovedToRecycleBinNotification>();
            builder.AddNotificationHandler<ContentSavedNotification, DateFoldersContentSavedNotification>();
        }
    }

    /// <summary>
    /// Item moving to recyclebin. We need to set the parent id's for the content item here since the context of the IContent is diffent when placed in recycle bin
    /// </summary>
    public class DateFoldersContentMovingToRecycleBinNotification : INotificationHandler<ContentMovingToRecycleBinNotification>
    {
        private readonly ILogger<ContentMovingToRecycleBinNotification> _logger;
        private readonly IContentService _contentService;
        private DateFoldersConfigBase _options;

        public DateFoldersContentMovingToRecycleBinNotification(IOptions<DateFoldersConfigBase> options, ILogger<ContentMovingToRecycleBinNotification> logger, IContentService contentService)
        {
            _options = options.Value;
            _logger = logger;
            _contentService = contentService;
        }

        public void Handle(ContentMovingToRecycleBinNotification notification)
        {
            foreach (var item in notification.MoveInfoCollection)
            {
                if (!_options.ItemDocTypes.Contains(item.Entity.ContentType.Alias)) continue;
                try
                {
                    var parent = _contentService.GetById(item.Entity.ParentId);
                    if (parent is null || !parent.ContentType.Alias.Equals(_options.FolderDocType)) continue;

                    if (_options.CreateDayFolders)
                    {
                        notification.State.Add($"dayId-{item.Entity.Key}", parent.Id);
                        notification.State.Add($"monthId-{item.Entity.Key}", parent.ParentId);

                        IContent yearItem = _contentService.GetById(parent.ParentId);
                        if (yearItem != null)
                        {
                            notification.State.Add($"yearId-{item.Entity.Key}", yearItem.ParentId);
                        }
                    }
                    else
                    {
                        notification.State.Add($"monthId-{item.Entity.Key}", parent.Id);
                        notification.State.Add($"yearId-{item.Entity.Key}", parent.ParentId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DateFolders ContentService_Trashing exception");
                }
            }
        }
    }

    /// <summary>
    /// Item moved to recyclebin. Clean up empty folders.
    /// </summary>
    public class DateFoldersContentMovedToRecycleBinNotification : INotificationHandler<ContentMovedToRecycleBinNotification>
    {
        private readonly ILogger<ContentMovedToRecycleBinNotification> _logger;
        private readonly IContentService _contentService;
        private DateFoldersConfigBase _options;

        public DateFoldersContentMovedToRecycleBinNotification(IOptions<DateFoldersConfigBase> options, ILogger<ContentMovedToRecycleBinNotification> logger, IContentService contentService)
        {
            _options = options.Value;
            _logger = logger;
            _contentService = contentService;
        }

        public void Handle(ContentMovedToRecycleBinNotification notification)
        {
            foreach (var item in notification.MoveInfoCollection)
            {
                string itemKey = item.Entity.Key.ToString();

                if (!_options.ItemDocTypes.Contains(item.Entity.ContentType.Alias)) continue;
                if (!notification.State.ContainsKey($"yearId-{itemKey}")) continue;

                try
                {
                    string dayKey = $"dayId-{itemKey}";
                    string monthKey = $"monthId-{itemKey}";
                    string yearKey = $"yearId-{itemKey}";

                    // Remove empty day folder if enabled
                    if (_options.CreateDayFolders)
                    {
                        if (int.TryParse(notification.State[dayKey].ToString(), out int dayId) && dayId > 0)
                        {
                            var day = _contentService.GetById(dayId);
                            ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, day, _contentService);
                        };
                        notification.State.Remove(dayKey);
                    }

                    // Remove empty month folder
                    if (int.TryParse(notification.State[monthKey].ToString(), out int monthId) && monthId > 0)
                    {
                        var month = _contentService.GetById(monthId);
                        ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, month, _contentService);
                        notification.State.Remove(monthKey);
                    }

                    // Remove empty year folder
                    if (int.TryParse(notification.State[yearKey].ToString(), out int yearId) && yearId > 0)
                    {
                        var year = _contentService.GetById(yearId);
                        ContentHelper.DeleteFolderIfEmpty(_options.FolderDocType, year, _contentService);
                        notification.State.Remove(yearKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DateFolders ContentService_Trashed exception");
                }
            }
        }
    }

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
            foreach (var content in notification.SavedEntities)
            {
                // Allowed doctypes
                if (!_options.ItemDocTypes.Contains(content.ContentType.Alias)) continue;

                IContentType folderDocType = _contentTypeService.Get(_options.FolderDocType);
                if (folderDocType is not null)
                {
                    if (content.ParentId > 0)
                    {
                        var date = GetItemDate(content, _options.ItemDateProperty);
                        IContent parent = _contentService.GetById(content.ParentId);

                        IContent? monthFolder = null;
                        IContent? yearFolder = null;
                        IContent? dayFolder = null;

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
                            IContent? newDayFolder = null;

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
                                _contentService.SaveAndPublish(content);
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
                    else
                    {
                        // Item is created under 'Content' root, which is unsupported
                        _logger.LogError("Creating a date folder item under 'Content' root is unsupported");
                    }
                }
                else
                {
                    // Date folder doctype is null
                    _logger.LogError("The date folder document type '{folderDocType}' does not exist", _options.FolderDocType);
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
                return content.GetValue<DateTime>(propertyAlias);
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
            IContent? content = null;
            var parentChildren = parent.GetAllChildren(_contentService);

            // Get first child of FolderDocType if it exists
            if (parentChildren.Any())
            {
                content = parentChildren.FirstOrDefault(x => x.ContentType.Alias.Equals(_options.FolderDocType) && x.Name.Equals(nodeName));
            }

            // Create folder if if non exists
            if (content is null)
            {
                content = _contentService.CreateContent(nodeName, parent.GetUdi(), _options.FolderDocType, currentUserId);
            }

            if (!content.Published) contentService.SaveAndPublish(content, userId: currentUserId);
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
        private void OrderChildrenByDateProperty(IContent parent, bool orderByDesc, string? propertyAlias = null)
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
    }
}
