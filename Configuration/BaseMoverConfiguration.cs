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
        public BaseMoverConfiguration(Database database, string template, string branch, SortOrder sort)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(database, "Database");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(template, "Template");

            Database = database;
            ItemKey = GetItemKey(database, template, branch);
            SortOrder = sort;
        }

        protected Database Database { get; set; }
        public string ItemKey { get; set; }
        public SortOrder SortOrder { get; set; }


        private string GetItemKey(Database db, string templatePath, string branchPath)
        {
            string key = string.Empty;

            TemplateItem ti = db.Templates[templatePath];
            if (ti == null)
                Sitecore.Diagnostics.Log.Warn($"bad template path={templatePath}...", this);
            else
                key = ti.ID.ToShortID().ToString().ToLower();


            if (!string.IsNullOrEmpty(branchPath))
            {
                BranchItem bi = db.Branches[branchPath];
                if (bi == null)
                    Sitecore.Diagnostics.Log.Warn($"bad branch path={branchPath}...", this);
                else
                    key += bi.ID.ToShortID().ToString().ToLower();
            }

            return key;
        }
    }


}
