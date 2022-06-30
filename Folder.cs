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
    using Sitecore.Data.Items;

    public class Folder
    {
        /// <summary>
        /// Gets the template to use for the item.
        /// </summary>

        //public TemplateItem Template { get; private set; }
        public string TemplatePath { get; private set; }

        /// <summary>
        /// Gets the format string to apply on the date to determine the name of the item.
        /// </summary>
        public string FormatString { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Folder"/> class.
        /// </summary>
        /// <param name="templatePath">The template path string.</param>
        /// <param name="format">The format.</param>
        public Folder(string templatePath, string format)
        {
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(templatePath, "Template Path");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(format, "format");

            TemplatePath = templatePath;
            FormatString = format;
        }

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        public string GetName(DateTime date)
        {
            return date.ToString(FormatString);
        }
    }
}
