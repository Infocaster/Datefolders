namespace Infocaster.Umbraco.DateFolders.Models
{
    public class DateFoldersConfigBase
    {
        public string[] ItemDocTypes { get; set; }
        //public int[] AllowedParentIds { get; set; }
        public string FolderDocType { get; set; }
        public bool OrderByDescending { get; set; } = true;
        public bool CreateDayFolders { get; set; } = false;
        public string ItemDateProperty { get; set; }
    }
}
