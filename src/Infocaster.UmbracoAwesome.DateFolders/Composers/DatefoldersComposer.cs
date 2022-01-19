using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Infocaster.UmbracoAwesome.DateFolders.Composers
{
    public class DatefoldersComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<DatefoldersComponent>();
        }
    }

    public class DatefoldersComponent : IComponent
    {
        private readonly string[] _itemDocType;
        private readonly string _itemDateProperty;
        private readonly string _dateFolderDocType;
        private readonly bool _createDayFolders;
        private readonly bool _orderByDescending;
        static readonly object Syncer = new object();

        private readonly ILogger _logger;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;
        
        public DatefoldersComponent(ILogger logger, IContentService contentService, IContentTypeService contentTypeService)
        {
            _logger = logger;
            _contentService = contentService;
            _contentTypeService = contentTypeService;

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["datefolders:ItemDocType"]) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["datefolders:DateFolderDocType"]))
            {
                _itemDocType = ConfigurationManager.AppSettings["datefolders:ItemDocType"].Split(',');
                _itemDateProperty = ConfigurationManager.AppSettings["datefolders:ItemDateProperty"];
                _dateFolderDocType = ConfigurationManager.AppSettings["datefolders:DateFolderDocType"];
                var createDayFoldersString = ConfigurationManager.AppSettings["datefolders:CreateDayFolders"];
                var orderByDescendingString = ConfigurationManager.AppSettings["datefolders:OrderByDescending"];

                if (!string.IsNullOrEmpty(createDayFoldersString))
                {
                    if (!bool.TryParse(createDayFoldersString, out _createDayFolders))
                    {
                        LogError(new ConfigurationErrorsException("datefolders:CreateDayFolders in not a valid boolean (true/false)"), "datefolders:CreateDayFolders configuration is not a valid boolean value");
                    }
                }

                if (!string.IsNullOrEmpty(orderByDescendingString))
                {
                    if (!bool.TryParse(orderByDescendingString, out _orderByDescending))
                    {
                        LogError(new ConfigurationErrorsException("datefolders:OrderByDecending in not a valid boolean (true/false)"), "datefolders:OrderByDecending configuration is not a valid boolean value");
                    }
                }
            }
        }

        public void Initialize()
        {
            if (_itemDocType == null || _itemDocType.Length <= 0 || string.IsNullOrEmpty(_dateFolderDocType)) return;

            CreateDateFolder();

            ContentService.Saved += ContentService_Saved;            
            ContentService.Trashing += ContentService_Trashing;
            ContentService.Trashed += ContentService_Trashed;
        }

        private void ContentService_Trashing(IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            foreach (var item in e.MoveInfoCollection)
            {
                if (!_itemDocType.Contains(item.Entity.ContentType.Alias)) continue;
                try
                {
                    var parent = _contentService.GetById(item.Entity.ParentId);
                    if (parent == null || !parent.ContentType.Alias.Equals(_dateFolderDocType)) continue;
                    if (_createDayFolders)
                    {
                        HttpContext.Current.Items.Add("dayId", parent.Id);
                        HttpContext.Current.Items.Add("monthId", parent.ParentId);

                        IContent yearItem = _contentService.GetById(parent.ParentId);
                        if (yearItem != null)
                        {
                            HttpContext.Current.Items.Add("yearId", _contentService.GetById(parent.ParentId).ParentId); 
                        }
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("monthId", parent.Id);
                        HttpContext.Current.Items.Add("yearId", parent.ParentId);
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "DateFolders ContentService_Trashing exception");
                }
            }
        }

        private void ContentService_Trashed(IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            foreach(var item in e.MoveInfoCollection)
            {
                if (!_itemDocType.Contains(item.Entity.ContentType.Alias)) continue;
                if (!HttpContext.Current.Items.Contains("yearId")) continue;

                try
                {
                    var dayId = -100;
                    int monthId;
                    int yearId;

                    if (_createDayFolders)
                    {
                        dayId = int.Parse(HttpContext.Current.Items["dayId"].ToString());
                        HttpContext.Current.Items.Remove("dayId");
                        monthId = int.Parse(HttpContext.Current.Items["monthId"].ToString());
                        yearId = int.Parse(HttpContext.Current.Items["yearId"].ToString());
                    }
                    else
                    {
                        monthId = int.Parse(HttpContext.Current.Items["monthId"].ToString());
                        yearId = int.Parse(HttpContext.Current.Items["yearId"].ToString());
                    }

                    HttpContext.Current.Items.Remove("monthId");
                    HttpContext.Current.Items.Remove("yearId");

                    if (_createDayFolders)
                    {
                        var day = sender.GetById(dayId);
                        if (!_contentService.HasChildren(dayId))
                        {
                            sender.Delete(day);
                        }
                    }

                    var month = sender.GetById(monthId);
                    if (!_contentService.HasChildren(monthId))
                    {
                        sender.Delete(month);
                    }

                    var year = sender.GetById(yearId);
                    if (!_contentService.HasChildren(yearId))
                    {
                        sender.Delete(year);
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "DateFolders ContentService_Trashed exception");
                }
            }
        }

        private void ContentService_Saved(IContentService sender, global::Umbraco.Core.Events.ContentSavedEventArgs e)
        {
            foreach (var content in e.SavedEntities)
            {
                if (!_itemDocType.Contains(content.ContentType.Alias)) continue;

                IContentType folderDocType = _contentTypeService.Get(_dateFolderDocType);
                if (folderDocType != null)
                {
                    if (content.ParentId > 0)
                    {
                        var date = GetItemDate(content, _itemDateProperty);

                        IContent monthFolder = null;
                        IContent yearFolder = null;
                        IContent dayFolder = null;

                        IContent parent = _contentService.GetById(content.ParentId);

                        bool dayChanged = false;
                        bool monthChanged;
                        bool yearChanged;

                        if (parent.ContentType.Alias.Equals(_dateFolderDocType))
                        {
                            if (_createDayFolders)
                            {
                                dayFolder = parent;
                                monthFolder = _contentService.GetById(dayFolder.ParentId);
                                yearFolder = _contentService.GetById(monthFolder.ParentId);
                                dayChanged = date.Day.ToString("00") != dayFolder.Name;
                            }
                            else
                            {
                                monthFolder = parent;
                                yearFolder = _contentService.GetById(monthFolder.ParentId);
                            }
                            parent = _contentService.GetById(yearFolder.ParentId);

                            yearChanged = date.Year.ToString() != yearFolder.Name;
                            monthChanged = date.Month.ToString("00") != monthFolder.Name;
                        }
                        else
                        {
                            dayChanged = true;
                            monthChanged = true;
                            yearChanged = true;
                        }

                        if (yearChanged || monthChanged || dayChanged)
                        {
                            IContent newDayFolder = null;

                            IContent newYearFolder = yearChanged ? GetOrCreateAndPublishDateFolder(sender, parent, date.Year.ToString(), content.CreatorId) : yearFolder;
                            IContent newMonthFolder = GetOrCreateAndPublishDateFolder(sender, newYearFolder, date.Month.ToString("00"), content.CreatorId);

                            if (_createDayFolders)
                            {
                                newDayFolder = GetOrCreateAndPublishDateFolder(sender, newMonthFolder, date.Day.ToString("00"), content.CreatorId);
                                sender.Move(content, newDayFolder.Id);
                            }
                            else
                            {
                                sender.Move(content, newMonthFolder.Id);
                            }

                            if (content.Published)
                            {
                                sender.SaveAndPublish(content);
                            }

                            
                            IContent orderParent = newMonthFolder;
                            if (_createDayFolders)
                            {
                                orderParent = newDayFolder;
                            }

                            if (!string.IsNullOrEmpty(_itemDateProperty))
                            {
                                OrderChildrenByDateProperty(orderParent, _itemDateProperty, _orderByDescending);
                            }
                            else
                            {
                                OrderChildrenByCreateDate(orderParent, _orderByDescending);
                            }

                            // Delete if empty
                            if (_createDayFolders)
                            {
                                if (dayFolder != null && !_contentService.HasChildren(dayFolder.Id))
                                {
                                    sender.Delete(dayFolder);
                                }
                            }

                            if (monthFolder != null && !_contentService.HasChildren(monthFolder.Id))
                            {
                                sender.Delete(monthFolder);

                                if (yearFolder != null && !_contentService.HasChildren(yearFolder.Id))
                                {
                                    sender.Delete(yearFolder);
                                }
                            }

                            // Order
                            OrderChildrenByName(parent, _orderByDescending);
                            OrderChildrenByName(newYearFolder, _orderByDescending);
                            if (_createDayFolders)
                            {
                                OrderChildrenByName(newMonthFolder, _orderByDescending);
                            }
                        }
                    }
                    else
                    {
                        // Item is created under 'Content' root, which is unsupported
                        LogError(new Exception("DateFolders Unsupported Exception"), $"Creating a date folder item under 'Content' root is unsupported");
                    }
                }
                else
                {
                    // Date folder doctype is null
                    LogError(new Exception("DateFolders configuration Error"), $"The date folder document type('{_dateFolderDocType}') does not exist");
                }
            }
        }

        
        public void Terminate()
        {

        }

        /// <summary>
        /// Returns the node under passed parent that matches the name passed or creates a node with that name and the dateFolderDocType.
        /// Saves and publishs created/found node.
        /// </summary>
        /// <returns></returns>
        IContent GetOrCreateAndPublishDateFolder(IContentService contentService, IContent parent, string nodeName, int currentUserId)
        {
            long totalChildren;
            int childCount = _contentService.CountChildren(parent.Id);
            IContent content;

            if (childCount > 0)
            {
                var childItems = _contentService.GetPagedChildren(parent.Id, 0, childCount, out totalChildren);
                content = childItems.FirstOrDefault(x => x.ContentType.Alias == _dateFolderDocType && x.Name.Equals(nodeName)) ??
                           contentService.CreateContent(nodeName, parent.GetUdi(), _dateFolderDocType, currentUserId);
            }
            else
            {
                content = contentService.CreateContent(nodeName, parent.GetUdi(), _dateFolderDocType, currentUserId);
            }

            if (!content.Published) contentService.SaveAndPublish(content, userId: currentUserId);
            return content;
        }

        /// <summary>
        /// Returns itemDataProperty or Creation Date of passed content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        private static DateTime GetItemDate(IContentBase content, string propertyAlias)
        {
            if (string.IsNullOrEmpty(propertyAlias)) return content.CreateDate;
            var dateValue = content.GetValue(propertyAlias) as DateTime?;
            return dateValue ?? content.CreateDate;
        }

        /// <summary>
        /// Order the children by propertyAlias, appends any node without alias.
        /// </summary>
        /// <param name="parent">The parent document</param>
        /// <param name="propertyAlias">The property alias</param>
        /// <param name="orderByDesc">order by descending or not</param>
        void OrderChildrenByDateProperty(IContent parent, string propertyAlias, bool orderByDesc)
        {
            lock (Syncer)
            {
                try
                {
                    long totalChildren;
                    int childCount = _contentService.CountChildren(parent.Id);
                    var childItems = _contentService.GetPagedChildren(parent.Id, 0, childCount, out totalChildren);

                    var orderedChildren = (orderByDesc ? childItems.Where(c => c.HasProperty(propertyAlias)).OrderByDescending(c => GetItemDate(c, propertyAlias))
                        : childItems.Where(c => c.HasProperty(propertyAlias)).OrderBy(c => GetItemDate(c, propertyAlias))).ToList();
                    orderedChildren.AddRange(childItems.Where(c => !c.HasProperty(propertyAlias)));
                    DoOrder(orderedChildren);
                }
                catch (Exception ex)
                {
                    LogError(ex, "DateFolders OrderChildrenByDateProperty exception");
                }
            }
        }

        /// <summary>
        /// Order the children by createDate
        /// </summary>
        /// <param name="parent">The parent cotent</param>
        /// <param name="orderByDesc">order by descending or not</param>
        void OrderChildrenByCreateDate(IContent parent, bool orderByDesc)
        {
            lock (Syncer)
            {
                try
                {
                    long totalChildren;
                    int childCount = _contentService.CountChildren(parent.Id);
                    var childItems = _contentService.GetPagedChildren(parent.Id, 0, childCount, out totalChildren);

                    DoOrder(orderByDesc ? childItems.OrderByDescending(x => x.CreateDate) : childItems.OrderBy(x => x.CreateDate));
                }
                catch (Exception ex)
                {
                    LogError(ex, "DateFolders OrderChildrenByCreateDate exception");
                }
            }
        }

        /// <summary>
        /// Order the children by name, converts Name to int any non int Names are pushed to the bottom of the ordering.
        /// </summary>
        /// <param name="parent">The parent content</param>
        /// <param name="orderByDesc">order by descending or not</param>
        void OrderChildrenByName(IContent parent, bool orderByDesc)
        {
            lock (Syncer)
            {
                try
                {
                    // Todo: find alternative for getPagedChildren, IContent.Children() doesn't exist at the moment
                    long totalChildren;                    
                    int childCount = _contentService.CountChildren(parent.Id);
                    var childItems = _contentService.GetPagedChildren(parent.Id, 0, childCount, out totalChildren);

                    int i;
                    var orderedChildren = (orderByDesc ? childItems.Where(c => int.TryParse(c.Name, out i)).OrderByDescending(x => int.Parse(x.Name))
                                               : childItems.Where(c => int.TryParse(c.Name, out i)).OrderBy(x => int.Parse(x.Name))).ToList();

                    orderedChildren.AddRange(childItems.Where(c => !int.TryParse(c.Name, out i)));
                    DoOrder(orderedChildren);
                }
                catch (Exception ex)
                {
                    LogError(ex, "DateFolders OrderChildrenByName exception");
                }
            }
        }

        /// <summary>
        /// Order documents
        /// </summary>
        /// <param name="contents">content to order</param>
        void DoOrder(IEnumerable<IContent> contents)
        {
            Current.Services.ContentService.Sort(contents);
        }

        /// <summary>
        /// Logs passed error to umbraco Log.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="header"></param>
        void LogError(Exception ex, string header)
        {
            _logger.Error<DatefoldersComponent>(ex, header);
        }

        private void CreateDateFolder()
        {
            var folderType =_contentTypeService.Get("folders");
            if (folderType == null)
            {
                folderType = new ContentType(-1);
                folderType.Alias = "folders";
                folderType.Name = "Folders";
                folderType.Description = "";
                folderType.AllowedAsRoot = true;
                folderType.SortOrder = 1;
                folderType.CreatorId = 0;
                folderType.Icon = "icon-folders";
                _contentTypeService.Save(folderType);
            }
        
            var docType = _contentTypeService.Get("dateFolder");
            if (docType == null)
            {
                docType = new ContentType(folderType.Id);
                docType.Alias = "dateFolder";
                docType.Name = "Date Folder";
                docType.Description = "";
                docType.AllowedAsRoot = true;
                docType.SortOrder = 1;
                docType.CreatorId = 0;
                docType.Icon = "icon-calendar-alt";
                _contentTypeService.Save(docType);
            }
        }
    }
}