using Infocaster.Umbraco.DateFolders.Helpers;
using Infocaster.Umbraco.DateFolders.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Infocaster.Umbraco.DateFolders.Composers.NotificatonHandlers
{
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
}
