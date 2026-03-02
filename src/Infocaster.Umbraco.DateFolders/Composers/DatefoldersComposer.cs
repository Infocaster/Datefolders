using Infocaster.Umbraco.DateFolders.Composers.NotificatonHandlers;
using Infocaster.Umbraco.DateFolders.Models;

using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

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
}
