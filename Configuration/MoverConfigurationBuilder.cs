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
    using System.Text.RegularExpressions;
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
            string itemKey = configNode.Attributes["itemKey"].Value;
            string folderKey = configNode.Attributes["folderKey"].Value;
            string folderTemplate = configNode["FolderTemplate"].InnerText;

            SortOrder s = GetSortOrder(configNode);

            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(itemKey, "Item Key");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderKey, "Folder Key");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderTemplate, "FolderTemplate");

            var sortFields = new string[] { };
            if (configNode["SortField"] != null && !string.IsNullOrEmpty(configNode["SortField"].InnerText))
            {
                sortFields = configNode["SortField"].InnerText.Split(new char[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            }


            return new AlphaMoverConfiguration(database, itemKey, folderKey, s, folderTemplate, sortFields);
        }

        private static DateMoverConfiguration CreateDateBased(Database database, XmlNode configNode)
        {
            string itemKey = configNode.Attributes["itemKey"].Value;
            string folderKey = configNode.Attributes["folderKey"].Value;
            string yearTemplate = configNode["YearTemplate"].InnerText;
            string yearFormat = configNode["YearTemplate"].GetAttributeWithDefault("formatString", null);
            string monthTemplate = null, monthFormat = null;
            string dayTemplate = null, dayFormat = null;
            string dateField = configNode["DateField"].InnerText;

            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(itemKey, "Item Key");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(folderKey, "Folder Key");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(yearTemplate, "YearTemplate");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(dateField, "DateField");

            // make sure we have the template of the items we want to move
            //TemplateItem templateItem = database.Templates[itemKey];

            //if (templateItem == null)
            //{
            //    Sitecore.Diagnostics.Log.Warn($"Template for '{itemKey}' not found.", configNode);
            //    return null;
            //}

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

            return CreateDateBased(database, itemKey, folderKey, dateField, yearTemplate, monthTemplate, dayTemplate, s, yearFormat, monthFormat, dayFormat);
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

        public static DateMoverConfiguration CreateDateBased(Database database, string itemKey, string folderKey, string dateField, string yearTemplate, string monthTemplate, string dayTemplate, SortOrder sort, string yearFormat = null, string monthFormat = null, string dayFormat = null)
        {
            var msg = string.Empty;

            if (!string.IsNullOrEmpty(yearTemplate))
            {
                if (Regex.IsMatch(yearTemplate, @"Branches/", RegexOptions.IgnoreCase))
                {
                    if (database.Branches[yearTemplate] == null)
                        msg += "Year BRANCH template does not exist: " + yearTemplate + "\n";
                }
                else
                {
                    if (database.Templates[yearTemplate] == null)
                        msg += "Year REGULAR template does not exist: " + yearTemplate + "\n";
                }
            }

            if (!string.IsNullOrEmpty(monthTemplate))
            {
                if (Regex.IsMatch(monthTemplate, @"Branches/", RegexOptions.IgnoreCase))
                {
                    if (database.Branches[monthTemplate] == null)
                        msg += "Year BRANCH template does not exist: " + monthTemplate + "\n";
                }
                else
                {
                    if (database.Templates[monthTemplate] == null)
                        msg += "Year REGULAR template does not exist: " + monthTemplate + "\n";
                }
            }

            if (!string.IsNullOrEmpty(dayTemplate))
            {
                if (Regex.IsMatch(dayTemplate, @"Branches/", RegexOptions.IgnoreCase))
                {
                    if (database.Branches[dayTemplate] == null)
                        msg += "Year BRANCH template does not exist: " + dayTemplate + "\n";
                }
                else
                {
                    if (database.Templates[dayTemplate] == null)
                        msg += "Year REGULAR template does not exist: " + dayTemplate + "\n";
                }
            }

            if (!string.IsNullOrEmpty(msg))
            {
                Log.Warn(string.Format("Invalid NewsMover configuration. \n{0}", msg), typeof(MoverConfigurationBuilder));
                return null;
            }

            return new DateMoverConfiguration(database, itemKey, folderKey, sort, dateField, yearTemplate, monthTemplate, dayTemplate, yearFormat, monthFormat, dayFormat);
        }
    }
}
