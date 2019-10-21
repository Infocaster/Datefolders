# Datefolders
This package creates Datefolders (year/month(/day)) for the specified doctype for Umbraco 8.2.x+. For older versions please use v2

# Behavior
- When you create a document with doctype "itemDocType", this package will automatically create year/month/day folders for it
- When you edit the "itemDateProperty", the document is automatically moved to the correct year/month/day folder
- Automatically cleans up empty year, month and day folders
- Orders the items in the year, month and dayfolders by "itemDateProperty" with every action

## Configuration
Add these keys/values to your appSettings section in the web.config:

Key: "datefolders:ItemDocType" - the doctype alias to create datefolders for (e.g. "newsItem") - comma separated values are allowed for multiple doctype aliases 

Key: "datefolders:ItemDateProperty" - the property of the itemDocType to read the date from (e.g. "startDate") (don't add this key if you just want to use the document's create date)

Key: "datefolders:DateFolderDocType" - the doctype to use for creating the year/month/day folders (e.g "DateFolder")

Key: "datefolders:CreateDayFolders" - boolean indicating whether or not day folders should be created

Key: "datefolders:OrderByDecending"  - boolean indicating sort order for date folders (default: false)