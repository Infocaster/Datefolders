
  _____        _          __      _     _               
 |  __ \      | |        / _|    | |   | |              
 | |  | | __ _| |_ ___  | |_ ___ | | __| | ___ _ __ ___ 
 | |  | |/ _` | __/ _ \ |  _/ _ \| |/ _` |/ _ \ '__/ __|
 | |__| | (_| | ||  __/ | || (_) | | (_| |  __/ |  \__ \
 |_____/ \__,_|\__\___| |_| \___/|_|\__,_|\___|_|  |___/
                                                        
                                                        
-----------------------------------------------------------------

Add the following configuration to your appsettings.json file:

"DateFolders": {
    "ItemDateProperty":  "",            // the property of the itemDocType to read the date from (e.g. "startDate") (don't add this key if you just want to use the document's create date)
    "CreateDayFolders": false,          // boolean indicating whether or not day folders should be created
    "OrderByDescending": true,          // boolean indicating sort order for date folders (default: false)
    "FolderDocType": "dateFolder",      // the doctype to use for creating the year/month/day folders (e.g "DateFolder")
    "ItemDocTypes": [ "contentPage" ]   // the doctype alias to create datefolders for (e.g. "newsItem") - multiple doctype aliases are allowed
}

-----------------------------------------------------------------

More information: https://our.umbraco.com/packages/developer-tools/datefolders/