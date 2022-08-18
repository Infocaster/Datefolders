using System.Collections.Generic;

namespace Infocaster.Umbraco.DateFolders.Models
{
    public class DateFoldersConfigBase
    {
        public List<string> ItemDocTypes { get; set; } = new List<string>();
        public List<int> AllowedParentIds { get; set; } = new List<int>();
        public List<string> AllowedParentDocTypes { get; set; } = new List<string>();
        public string FolderDocType { get; set; }
        public bool OrderByDescending { get; set; } = true;
        public bool CreateDayFolders { get; set; } = false;
        public string ItemDateProperty { get; set; }
    }
}
