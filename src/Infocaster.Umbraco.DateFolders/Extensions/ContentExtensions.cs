using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Infocaster.Umbraco.DateFolders.Extensions
{
    public static class ContentExtensions
    {
        public static IEnumerable<IContent> GetAllChildren(this IContent item, IContentService contentService)
        {
            int childCount = contentService.CountChildren(item.Id);
            return contentService.GetPagedChildren(item.Id, 0, childCount, out long totalChildren);
        }
    }
}
