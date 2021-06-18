//------------------------------------------------------------------------------------------------- 
// <copyright file="NewsEventHandler.cs" company="Sitecore Shared Source">
// Copyright (c) Sitecore.  All rights reserved.
// </copyright>
// <summary>Defines the NewsEventHandler type.</summary>
// <license>
// http://sdn.sitecore.net/Resources/Shared%20Source/Shared%20Source%20License.aspx
// </license>
// <url>https://github.com/JimmieOverby/NewsMover</url>
//-------------------------------------------------------------------------------------------------

namespace Sitecore.Sharedsource.NewsMover
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Sharedsource.NewsMover.Configuration;
    using Sitecore.Sharedsource.NewsMover.Sorters;

    public class EventHandler
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
        protected Dictionary<ID, IMoverConfiguration> Templates { get; private set; }

        public EventHandler()
        {
            Templates = new Dictionary<ID, IMoverConfiguration>();
        }

        /// <summary>
        /// Adds a configured template to be organized
        /// </summary>
        /// <param name="configNode">The XML configuration node.</param>
        public virtual void AddTemplateConfiguration(XmlNode configNode)
        {
            var templateConfig = MoverConfigurationBuilder.Create(SitecoreDatabase, configNode);
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

            var config = Templates[item.TemplateID];
            var s = GetSorter(item, config);
            s.OrganizeItem(item, config);

            _inProcess.Remove(item.ID);
        }

        private ISorter GetSorter(Item item, IMoverConfiguration config)
        {
            if (config is DateMoverConfiguration)
            {
                return new DateSorter(Templates);
            }
            else
            {
                return new AlphaSorter(Templates);
            }
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
                    var config = MoverConfigurationBuilder.CreateDateBased(SitecoreDatabase, ArticleTemplate, DateField, YearTemplate, MonthTemplate, DayTemplate, DefaultSettings.SortOrder);
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

    }
}
