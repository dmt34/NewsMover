﻿//------------------------------------------------------------------------------------------------- 
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
    using Sitecore.Data;
    using Sitecore.Data.Items;

    public abstract class BaseMoverConfiguration : IMoverConfiguration
    {
        public BaseMoverConfiguration(Database database, string itemKey, string folderKey, SortOrder sort)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(database, "Database");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(itemKey, "Item Key");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderKey, "Folder Key");

            Database = database;
            ItemKey = itemKey;
            FolderKey = folderKey;
            SortOrder = sort;
        }

        public Database Database { get; set; }
        public string ItemKey { get; set; }
        public string FolderKey { get; set; }
        public SortOrder SortOrder { get; set; }

    }
}
