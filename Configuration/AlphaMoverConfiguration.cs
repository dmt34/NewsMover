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
    using Sitecore.Sharedsource.NewsMover;

    public class AlphaMoverConfiguration : BaseMoverConfiguration
    {
        public TemplateItem FolderTemplate { get; private set; }

        public string[] SortFields { get; private set; }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateConfiguration"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="template">The template.</param>
        /// <param name="dateField">The date field.</param>
        /// <param name="yearTemplate">The year template.</param>
        /// <param name="monthTemplate">The month template.</param>
        /// <param name="dayTemplate">The day template.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="yearFormat">The year format.</param>
        /// <param name="monthFormat">The month format.</param>
        /// <param name="dayFormat">The day format.</param>
        internal AlphaMoverConfiguration(Database database, string template, SortOrder sort, string folderTemplate, string[] sortFields)
            : base (database, template, sort)
        {
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderTemplate, "FolderTemplate");

            FolderTemplate = Database.Templates[folderTemplate];
            SortFields = sortFields;
        }
    }
}
