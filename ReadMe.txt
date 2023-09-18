  _____        _          __      _     _               
 |  __ \      | |        / _|    | |   | |              
 | |  | | __ _| |_ ___  | |_ ___ | | __| | ___ _ __ ___ 
 | |  | |/ _` | __/ _ \ |  _/ _ \| |/ _` |/ _ \ '__/ __|
 | |__| | (_| | ||  __/ | || (_) | | (_| |  __/ |  \__ \
 |_____/ \__,_|\__\___| |_| \___/|_|\__,_|\___|_|  |___/
                                                        
                                                        
========================================================

Add the following configuration to your appsettings.json file:

"DateFolders": {
    "ItemDateProperty":  "",            
    "CreateDayFolders": false,          
    "OrderByDescending": true,          
    "FolderDocType": "dateFolder",      
    "MonthFolderDocType": "",      
    "DayFolderDocType": "",      
    "ItemDocTypes": [ "blogpost" ],
    "AllowedParentIds": [ 2222 ],
    "AllowedParentDocTypes": ["blog"]
}

ItemDocType | The doctype alias to create datefolders for. (e.g. "blogpost") - comma separated values are allowed for multiple doctype aliases
ItemDateProperty | The property of the itemDocType to read the date from. (e.g. "startDate") - don't add this key if you just want to use the document's create date
FolderDocType | The doctype alias to use for creating the year/month/day folders. (e.g "dateFolder")
MonthFolderDocType | (Optional) The doctype alias to use for the month folder, if not specified will default to FolderDocType.
DayFolderDocType | (Optional) The doctype alias to use for the day folder, if not specified will default to FolderDocType.
CreateDayFolders | Boolean indicating whether or not day folders should be created, if false only years and months are created.
OrderByDecending | Boolean indicating sort order for date folders (default: false)
AllowedParentIds | (Optional) The node id for the parent(s) to limit the creation of datefolders to. (e.g. 1234) - comma separated values are allowed for multiple note ids
AllowedParentDocTypes | (Optional) The doctype alias for the parent(s) to limit the creation of datefolders to. (e.g. "blog") - comma separated values are allowed for multiple doctype aliases


For more information, check out the links below:
Github - https://github.com/Infocaster/Datefolders
Our Umbraco - https://our.umbraco.com/packages/developer-tools/datefolders