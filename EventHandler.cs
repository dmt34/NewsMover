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
        //private bool _legacyConfigLoaded = false;
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

        /// <summary>
        /// Gets the templates to be organized.
        /// </summary>
        protected Dictionary<string, IMoverConfiguration> Templates { get; private set; }

        public EventHandler()
        {
            Templates = new Dictionary<string, IMoverConfiguration>();
        }

        /// <summary>
        /// Adds a configured template to be organized
        /// </summary>
        /// <param name="configNode">The XML configuration node.</param>
        public virtual void AddTemplateConfiguration(XmlNode configNode)
        {
            var templateConfig = MoverConfigurationBuilder.Create(SitecoreDatabase, configNode);
            if (templateConfig != null && !Templates.ContainsKey(templateConfig.ItemKey))
            {
                Templates.Add(templateConfig.ItemKey, templateConfig);
            }
        }

        /// <summary>
        /// Called when [item saved].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void OnItemSaved(object sender, EventArgs args)
        {
            //LoadLegacySettings();

            Item item = GetItem(args);

            if (!string.Equals(item.Database.Name, Database, StringComparison.OrdinalIgnoreCase) || // if we are NOT in the supported database
                !Templates.ContainsKey(item.ItemKey()) ||  // if the target item template is NOT supported
                item.IsStandardValues() || // if we are the standard value
                _inProcess.Contains(item.ID))
            {
                return;
            }

            _inProcess.Add(item.ID);

            var config = Templates[item.ItemKey()];
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
