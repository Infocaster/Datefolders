using Infocaster.Umbraco.DateFolders.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Infocaster.Umbraco.DateFolders.Composers.NotificatonHandlers
{
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
}
