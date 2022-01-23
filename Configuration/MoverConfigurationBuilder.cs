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
    using System.Linq;
    using System.Xml;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Sharedsource.NewsMover;

    internal class MoverConfigurationBuilder
    {
        /// <summary>
        /// Creates a template configuration from the XmlNode
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="configNode">The config node.</param>
        /// <returns></returns>
        public static IMoverConfiguration Create(Database database, XmlNode configNode)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(database, "Database");
            Sitecore.Diagnostics.Assert.IsNotNull(configNode, "XmlNode");

            if (configNode["DateField"] != null)
            {
                return CreateDateBased(database, configNode);
            }
            else
            {
                return CreateAlphaBased(database, configNode);
            }
        }

        private static IMoverConfiguration CreateAlphaBased(Database database, XmlNode configNode)
        {
            string template = configNode.Attributes["id"].Value;
            string branch = configNode.Attributes["branch"]?.Value ?? string.Empty;
            string folderTemplate = configNode["FolderTemplate"].InnerText;

            SortOrder s = GetSortOrder(configNode);

            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(template, "Template");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderTemplate, "FolderTemplate");

            var sortFields = new string[] { };
            if (configNode["SortField"] != null && !string.IsNullOrEmpty(configNode["SortField"].InnerText))
            {
                sortFields = configNode["SortField"].InnerText.Split(new char[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            }


            return new AlphaMoverConfiguration(database, template, branch, s, folderTemplate, sortFields);
        }

        private static DateMoverConfiguration CreateDateBased(Database database, XmlNode configNode)
        {
            string template = configNode.Attributes["id"].Value;
            string branch = configNode.Attributes["branch"]?.Value ?? string.Empty;
            string yearTemplate = configNode["YearTemplate"].InnerText;
            string yearFormat = configNode["YearTemplate"].GetAttributeWithDefault("formatString", null);
            string monthTemplate = null, monthFormat = null;
            string dayTemplate = null, dayFormat = null;
            string dateField = configNode["DateField"].InnerText;

            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(template, "Template");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(yearTemplate, "YearTemplate");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(dateField, "DateField");

            // make sure we have the template of the items we want to move
            TemplateItem templateItem = database.Templates[template];

            if (templateItem == null)
            {
                Sitecore.Diagnostics.Log.Warn(string.Format("Template '{0}' not found.", template), configNode);
                return null;
            }

            if (configNode["MonthTemplate"] != null)
            {
                monthTemplate = configNode["MonthTemplate"].InnerText;
                monthFormat = configNode["MonthTemplate"].GetAttributeWithDefault("formatString", null);
            }

            if (configNode["DayTemplate"] != null)
            {
                dayTemplate = configNode["DayTemplate"].InnerText;
                dayFormat = configNode["DayTemplate"].GetAttributeWithDefault("formatString", null);
            }

            SortOrder s = GetSortOrder(configNode);

            return CreateDateBased(database, template, branch, dateField, yearTemplate, monthTemplate, dayTemplate, s, yearFormat, monthFormat, dayFormat);
        }

        private static SortOrder GetSortOrder(XmlNode configNode)
        {
            string sort = configNode.GetAttributeWithDefault("sort", null);
            SortOrder s = DefaultSettings.SortOrder;
            if (!string.IsNullOrEmpty(sort))
            {
                Enum.TryParse(sort, true, out s);
            }

            return s;
        }

        public static DateMoverConfiguration CreateDateBased(Database database, string template, string branch, string dateField, string yearTemplate, string monthTemplate, string dayTemplate, SortOrder sort, string yearFormat = null, string monthFormat = null, string dayFormat = null)
        {
            var msg = string.Empty;

            if (database.Templates[template] == null)
                msg += "Template does not exist: " + template + "\n";

            if (!string.IsNullOrEmpty(yearTemplate) && database.Templates[yearTemplate] == null)
                msg += "Year template does not exist: " + yearTemplate + "\n";

            if (!string.IsNullOrEmpty(monthTemplate) && database.Templates[monthTemplate] == null)
                msg += "Month template does not exist: " + monthTemplate + "\n";

            if (!string.IsNullOrEmpty(dayTemplate) && database.Templates[dayTemplate] == null)
                msg += "Day template does not exist: " + dayTemplate + "\n";

            if (!string.IsNullOrEmpty(msg))
            {
                Log.Warn(string.Format("Invalid NewsMover configuration. \n{0}", msg), typeof(MoverConfigurationBuilder));
                return null;
            }

            return new DateMoverConfiguration(database, template, branch, sort, dateField, yearTemplate, monthTemplate, dayTemplate, yearFormat, monthFormat, dayFormat);
        }
    }
}
