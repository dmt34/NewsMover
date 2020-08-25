//------------------------------------------------------------------------------------------------- 
// <copyright file="NewsEventHandler.cs" company="Sitecore Shared Source">
// Copyright (c) Sitecore.  All rights reserved.
// </copyright>
// <summary>Defines the NewsEventHandler type.</summary>
// <license>
// http://sdn.sitecore.net/Resources/Shared%20Source/Shared%20Source%20License.aspx
// </license>
// <url>http://trac.sitecore.net/NewsMover/</url>
//-------------------------------------------------------------------------------------------------

namespace Sitecore.Sharedsource.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;
    using Sitecore.Sharedsource.NewsMover;
    using Sitecore.Sharedsource.NewsMover.Pipelines;

    public class NewsMover
    {
        private static readonly SynchronizedCollection<ID> _inProcess = new SynchronizedCollection<ID>();
        private bool _legacyConfigLoaded = false;
        private string _databaseName = "master";
        private Database _database = null;

        public string Database
        {
            get
            {
                return _databaseName;
            }
            set
            {
                _databaseName = value;
                Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(_databaseName, "Database");
            }
        }

        /// <summary>
        /// Gets the sitecore database.
        /// </summary>
        protected Database SitecoreDatabase
        {
            get
            {
                if (_database == null)
                {
                    _database = Sitecore.Configuration.Factory.GetDatabase(_databaseName);
                    Sitecore.Diagnostics.Assert.IsNotNull(_database, this.Database);
                }
                return _database;
            }
        }

        #region Legacy Config Properties
        public string YearTemplate { get; set; }

        public string MonthTemplate { get; set; }

        public string DayTemplate { get; set; }

        public string DateField { get; set; }

        public string ArticleTemplate { get; set; }
        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance has legacy configuration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has legacy configuration; otherwise, <c>false</c>.
        /// </value>
        protected bool HasLegacyConfiguration
        {
            get
            {
                var isLegacy = !string.IsNullOrEmpty(DateField) || !string.IsNullOrEmpty(ArticleTemplate) || !string.IsNullOrEmpty(YearTemplate);
                if (isLegacy)
                {
                    Log.Info(string.Format("Legacy configurations used: [DateField:{0}][ArticleTemplate:{1}][YearTemplate:{2}]", DateField, ArticleTemplate, YearTemplate), this);
                }
                return isLegacy;
            }
        }

        /// <summary>
        /// Gets the templates to be organized.
        /// </summary>
        protected Dictionary<ID, TemplateConfiguration> Templates { get; private set; }

        public NewsMover()
        {
            Templates = new Dictionary<ID, TemplateConfiguration>();
        }

        /// <summary>
        /// Adds a configured template to be organized
        /// </summary>
        /// <param name="configNode">The XML configuration node.</param>
        public virtual void AddTemplateConfiguration(XmlNode configNode)
        {
            var templateConfig = TemplateConfigurationBuilder.Create(SitecoreDatabase, configNode);
            if (templateConfig != null && !Templates.ContainsKey(templateConfig.Template.ID))
            {
                Templates.Add(templateConfig.Template.ID, templateConfig);
            }
        }

        /// <summary>
        /// Called when [item saved].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void OnItemSaved(object sender, EventArgs args)
        {
            LoadLegacySettings();

            Item item = GetItem(args);

            if (!string.Equals(item.Database.Name, Database, StringComparison.OrdinalIgnoreCase) || // if we are NOT in the supported database
                !Templates.ContainsKey(item.TemplateID) ||  // if the template is NOT supported
                item.IsStandardValues() || // if we are the standard value
                _inProcess.Contains(item.ID))
            {
                return;
            }

            _inProcess.Add(item.ID);
            TemplateConfiguration config = Templates[item.TemplateID];
            DateTime articleDate = EnsureAndGetDate(item, config.DateField);
            OrganizeItem(item, config, articleDate);
            _inProcess.Remove(item.ID);
        }

        /// <summary>
        /// Loads the legacy settings.
        /// </summary>
        protected void LoadLegacySettings()
        {
            // The original version of this module supported a single template.
            // we don't want to break this so lets convert the legacy settings into 
            // our new TemplateConfiguration.
            // we only want to do this once since this class is instantiated once by Sitecore 
            // and kept around for all usages (i.e. some sort of singleton)

            if (!_legacyConfigLoaded)
            {
                if (HasLegacyConfiguration)
                {
                    Log.Info("Loading legacy configuration for NewsMover", this);

                    // create a new wrapper around the old config
                    var config = TemplateConfigurationBuilder.Create(SitecoreDatabase, ArticleTemplate, DateField, YearTemplate, MonthTemplate, DayTemplate, DefaultSettings.SortOrder);
                    if (config != null && !Templates.ContainsKey(config.Template.ID))
                        Templates.Add(config.Template.ID, config);
                }
                _legacyConfigLoaded = true;
            }
        }

        /// <summary>
        /// Gets the item from the event args.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected Item GetItem(EventArgs args)
        {
            Item item = Sitecore.Events.Event.ExtractParameter(args, 0) as Item;
            Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
            return item;
        }

        /// <summary>
        /// Initializes the date field to the current date if there is no value.
        /// </summary>
        /// <param name="item">The article.</param>
        /// <param name="dateFieldName">Name of the date field.</param>
        /// <returns></returns>
        protected DateTime EnsureAndGetDate(Item item, string dateFieldName)
        {
            Sitecore.Data.Fields.DateField dateField = item.Fields[dateFieldName];
            Sitecore.Diagnostics.Assert.IsNotNull(dateField, dateFieldName);
            DateTime result = dateField.DateTime;

            // if there is no value in the date field, then set it to now.
            if (string.IsNullOrEmpty(dateField.InnerField.Value))
            {
                using (new Sitecore.Data.Items.EditContext(item))
                {
                    dateField.Value = Sitecore.DateUtil.IsoNow;
                    result = Sitecore.DateUtil.IsoDateToDateTime(dateField.InnerField.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Organizes the item in the configurd year, [month], [day] structure.
        /// It will also remove any empty folders
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        /// <param name="articleDate">The article date.</param>
        protected void OrganizeItem(Item item, TemplateConfiguration config, DateTime articleDate)
        {
            // Assign each news item an explicit sortorder
            var sign = config.SortOrder == SortOrder.Descending ? "-" : string.Empty;
            string text = sign + DateUtil.ToIsoDate(articleDate).Substring(4).Replace("T", string.Empty);
            if (item[FieldIDs.Sortorder] != text)
            {
                using (new EditContext(item))
                {
                    item[FieldIDs.Sortorder] = text;
                }
            }

            Item root = GetRoot(item, config);

            // get/create the year folder
            root = GetOrCreateChild(root, config.YearFolder.Template, config.YearFolder.GetName(articleDate), config.SortOrder, articleDate.Year);

            // get/create any month -> day structure we need
            if (config.MonthFolder != null)
            {
                root = GetOrCreateChild(root, config.MonthFolder.Template, config.MonthFolder.GetName(articleDate), config.SortOrder, articleDate.Month);

                if (config.DayFolder != null)
                {
                    root = GetOrCreateChild(root, config.DayFolder.Template, config.DayFolder.GetName(articleDate), config.SortOrder, articleDate.Day);
                }
            }

            // if the item is already where it should be, then bail out
            if (!string.Equals(item.Parent.Paths.FullPath, root.Paths.FullPath, StringComparison.OrdinalIgnoreCase))
            {

                // save the original location so we can clean up 
                Item originalParent = item.Parent;

                // move the item to the proper location
                item.MoveTo(root);

                // delete the original parent if there are no children.
                // keep walking up while we are a year/month/day
                while ((!originalParent.HasChildren) && IsItemYearMonthOrDay(originalParent, config))
                {
                    Item parent = originalParent.Parent;
                    originalParent.Delete();
                    originalParent = parent;
                }

            }

            if ((!Sitecore.Context.IsBackgroundThread) && Sitecore.Context.ClientPage.IsEvent)
            {
                var args = new MoveCompletedArgs() { Article = item, Root = item.Database.GetRootItem() };
                CorePipeline.Run("NewsMover.MoveCompleted", args);
            }
        }

        /// <summary>
        /// Gets the root of where we start organization.
        /// i.e. the parent of the 'year' node
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected Item GetRoot(Item item, TemplateConfiguration config)
        {
            Item parent = item.Parent;

            while (Templates.ContainsKey(parent.TemplateID) || IsItemYearMonthOrDay(parent, config))
            {
                parent = parent.Parent;
            }

            return parent;
        }

        /// <summary>
        /// Gets the or create child.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        protected Item GetOrCreateChild(Item parent, TemplateItem template, string childName, SortOrder sortOrder, int sortIndex)
        {
            Item child = parent.Children[childName];
            if (child == null)
            {
                child = parent.Add(childName, template);
            }

            if (sortOrder == SortOrder.Descending)
                sortIndex = sortIndex * -1;

            var sortValue = sortIndex.ToString();

            // enforce that sub-item sorting is set
            if (child[FieldIDs.Sortorder] != sortValue)
            {
                using (new Sitecore.Data.Items.EditContext(child))
                {
                    child.Fields[FieldIDs.Sortorder].Value = sortValue;
                }
            }

            return child;
        }

        /// <summary>
        /// Determines whether the item is a year, month or day item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        /// <returns>
        ///   <c>true</c> if [is item year month or day] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsItemYearMonthOrDay(Item item, TemplateConfiguration config)
        {
            return item.TemplateID == config.YearFolder.Template.ID
                    || (config.MonthFolder != null && item.TemplateID == config.MonthFolder.Template.ID)
                    || (config.DayFolder != null && item.TemplateID == config.DayFolder.Template.ID);
        }
    }
}
