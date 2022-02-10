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
    "ItemDocTypes": [ "contentPage" ]   
}

ItemDocType | The doctype alias to create datefolders for. (e.g. "contentPage") - comma separated values are allowed for multiple doctype aliases
ItemDateProperty | The property of the itemDocType to read the date from. (e.g. "startDate") (don't add this key if you just want to use the document's create date)
DateFolderDocType | The doctype to use for creating the year/month/day folders. (e.g "DateFolder")
CreateDayFolders | Boolean indicating whether or not day folders should be created, if false only years and months are created.
OrderByDecending | Boolean indicating sort order for date folders (default: false)

For more information, check out the links below:
Github - https://github.com/Infocaster/Datefolders
Our Umbraco - https://our.umbraco.com/packages/developer-tools/datefolders