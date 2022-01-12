using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Infocaster.Umbraco.DateFolders.Helpers
{
    static class ContentHelper
    {
        /// <summary>
        /// Deletes the alphabetic folder if it does not have any children
        /// </summary>
        public static void DeleteFolderIfEmpty(string folderDocType, IContent folder, IContentService contentService)
        {
            if (folder.ContentType.Alias == folderDocType && !contentService.HasChildren(folder.Id))
            {
                contentService.MoveToRecycleBin(folder);
            }
        }
    }
}
