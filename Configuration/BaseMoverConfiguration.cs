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

namespace Sitecore.Sharedsource.NewsMover.Configuration
{
    using System;
    using System.Xml;
    using Sitecore.Data;
    using Sitecore.Data.Items;

    public abstract class BaseMoverConfiguration : IMoverConfiguration
    {
        public BaseMoverConfiguration(Database database, string template, SortOrder sort)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(database, "Database");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(template, "Template");

            Database = database;
            Template = Database.Templates[template];
            SortOrder = sort;
        }

        protected Database Database { get; set; }

        public TemplateItem Template { get; set; }

        public SortOrder SortOrder { get; set; }
    }
}
