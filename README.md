# Datefolders
This package creates Datefolders (year/month(/day)) for the specified doctype for Umbraco 9. For umbraco 8 use v3, for older versions please use v2

# Behavior
- When you create a document with doctype "itemDocType", this package will automatically create year/month/day folders for it
- When you edit the "itemDateProperty", the document is automatically moved to the correct year/month/day folder
- Automatically cleans up empty year, month and day folders
- Orders the items in the year, month and dayfolders by "itemDateProperty" with every action

## Configuration
Add these keys/values to your appsettings.json in a new section:
 
<pre>
"DateFolders": {
    "ItemDateProperty":  "",            // the property of the itemDocType to read the date from (e.g. "startDate") (don't add this key if you just want to use the document's create date)
    "CreateDayFolders": false,          // boolean indicating whether or not day folders should be created
    "OrderByDescending": true,          // boolean indicating sort order for date folders (default: false)
    "FolderDocType": "dateFolder",      // the doctype to use for creating the year/month/day folders (e.g "DateFolder")
    "ItemDocTypes": [ "contentPage" ]   // the doctype alias to create datefolders for (e.g. "newsItem") - multiple doctype aliases are allowed
}
</pre>