<h3 align="center">
<img height="100" src="https://raw.githubusercontent.com/Infocaster/.github/main/assets/infocaster_nuget_yellow.svg">
</h3>

<h1 align="center">
DateFolders

[![Downloads](https://img.shields.io/nuget/dt/Infocaster.Umbraco.DateFolders?color=ff0069)](https://www.nuget.org/packages/Infocaster.Umbraco.DateFolders/)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Infocaster.Umbraco.DateFolders?color=ffc800)](https://www.nuget.org/packages/Infocaster.Umbraco.DateFolders/)
![GitHub](https://img.shields.io/github/license/Infocaster/DateFolders?color=ff0069)

</h1>
This package makes it easy to separate your content base on the date on which it is added.
With websites becoming bigger and bigger it is important that the content editors can easily find their content. This package makes that possible!
A great use case for this is a website that shows articles or blog posts. With this package they will always be ordered correctly by date, no matter how many items the content editor has written.

## Requirements
This package creates DateFolders (year/month/day) for the Umbraco backoffice to help the Content Editor easily find pages.
**for Umbraco 10 use version 10.0.x and above!**

For Umbraco 8 please use 3.0.x. For umbraco 7 use v2 and older versions please use v1.4, these can be retrieved from [Our.Umbraco](https://our.umbraco.com/packages/developer-tools/datefolders/) <br> <br>


## Getting Started
The DateFolders package is also available via NuGet. Visit [The DateFolders package on NuGet](https://www.nuget.org/packages/Infocaster.Umbraco.DateFolders/).
After installing the package, just complete the configuration steps below and you'll be good to go!

## Behavior
- When you create a document with doctype "itemDocType", this package will automatically create year/month/day folders for it
- When you edit the "itemDateProperty", the document is automatically moved to the correct year/month/day folder
- Automatically cleans up empty year, month and day folders
- Orders the items in the year, month and dayfolders by "itemDateProperty" with every action

## Configuration
```json
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
```

- **ItemDocTypes** | The doctype alias to create datefolders for. (e.g. "blogpost") - comma separated values are allowed for multiple doctype aliases
- **ItemDateProperty** | The property of the itemDocType to read the date from. (e.g. "startDate") - don't add this key if you just want to use the document's create date
- **FolderDocType** | The doctype alias to use for creating the year/month/day folders. (e.g "dateFolder")
- **MonthFolderDocType** | (Optional) The doctype alias to use for the month folder, if not specified will default to FolderDocType.
- **DayFolderDocType** | (Optional) The doctype alias to use for the day folder, if not specified will default to FolderDocType.
- **CreateDayFolders** | Boolean indicating whether or not day folders should be created, if false only years and months are created.
- **OrderByDecending** | Boolean indicating sort order for date folders (default: false)
- **AllowedParentIds** | (Optional) The node id for the parent(s) to limit the creation of datefolders to. (e.g. 1234) - comma separated values are allowed for multiple note ids
- **AllowedParentDocTypes** | (Optional) The doctype alias for the parent(s) to limit the creation of datefolders to. (e.g. "blog") - comma separated values are allowed for multiple doctype aliases


## Changelog
Version 11.0.0
- Updated to use umbraco v11

Version 10.0.0
- Updated to use umbraco v10.

Version 9.0.0
- Updated to use umbraco v9.

Version 3.0.0 
- Updated to use umbraco v8.

Version 2.1.2
- Fixed nested date folders.
- Fix to sort

Version 2.1.1
- Fixed cast error when using Date picker with DB type date.

Version 2.1
- Removed legacy configuration settings
- Added datefolders:OrderByDecending
- Implemented fix for 'Publish At' given by - Wayne Godfrey
- Refactored to reduce code complexity

Version 2.0.1
- Fix to order by child name

Version 2.0
- Updated to use umbraco v6 api.
- Fixed ordering to handle non date folders.

Version 1.4
- Removed Threading (Threading can cause the back-end to be out-of-sync, therefore removed)
- Changed configuration keys, added prefix (legacy still works)
- Added day folders feature (configurable, off by default)
- Fixed silly order by hard-coded propertyAlias bug

Version 1.3
- Better exception handling (speechbubble)
- Exception get's handled when the datefoler document type doesn't exist
- Month folders are now named with a leading zero if the month number is a single number (01, 02 etc.)
- Exception get's handled when a date item is created under the 'Content' root node

Version 1.2
- Support for multiple docTypes (comma separated)

Version 1.1
- Tree get's synced automatically

<a href="https://infocaster.net">
<img align="right" height="200" src="https://raw.githubusercontent.com/Infocaster/.github/main/assets/Infocaster_Corner.png">
</a>