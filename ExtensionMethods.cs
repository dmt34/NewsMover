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

using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.Sharedsource.NewsMover
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Determines whether [is null or empty] [the specified s].
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>
        ///   <c>true</c> if [is null or empty] [the specified s]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }

    internal static class XmlExtensions
    {
        /// <summary>
        /// Gets the attribute from the XmlNode. If no attribute exists, return the default value. If the attribute is empty string then use default.
        /// </summary>
        /// <param name="configNode">The config node.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="default">The default value.</param>
        /// <returns></returns>
        public static string GetAttributeWithDefault(this XmlNode configNode, string attributeName, string @default)
        {
            return configNode.Attributes[attributeName] == null ? @default : configNode.Attributes[attributeName].Value.IsNullOrEmpty() ? @default : configNode.Attributes[attributeName].Value;
        }
    }

    internal static class ItemExtensions
    {
        /// <summary>
        /// Determines whether the item is the __standard values item of the template.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the item is the __standard values item of the template; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStandardValues(this Item item)
        {
            if (item == null)
                return false;

            bool isStandardValue = false;

            if (item.Template.StandardValues != null)
                isStandardValue = (item.Template.StandardValues.ID == item.ID);

            return isStandardValue;
        }

        /// <summary>
        ///   Returns the dictionary key for checking managed items.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   string value of target item's "News Mover Id" field
        /// </returns>
        /// <remarks>
        ///   - compare with "itemKey" attribute of managed items in NewsMover.config
        ///   - if item is null or item key is empty, return a non-null, non-empty string (guaranteed to not match)
        /// </remarks>
        public static string ItemKey(this Item item)
        {
            if (item == null)
                return Constants.NotManagedKey;

            string key = item[Constants.NewsMoverTargetIdField];
            
            if (string.IsNullOrEmpty(key))
                return Constants.NotManagedKey;

            return key;
        }

        /// <summary>
        ///   Returns a string for identifying years, months, days, alpha folders
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   string value of item's "Folder Id" field
        /// </returns>
        /// <remarks>
        ///   - compare with "folderKey" attribute of managed items in NewsMover.config
        ///   - if item is null or item key is empty, return empty string
        /// </remarks>

        public static string FolderKey(this Item item)
        {
            if (item == null)
                return "";

            string folderkey = item[Constants.NewsMoverFolderIdField];

            if (string.IsNullOrEmpty(folderkey))
                return "";

            return folderkey;
        }

        public static Item MakeChild(this Item parent, Database db, string name, string tpath)
        {
            if (Regex.IsMatch(tpath, @"Branches/", RegexOptions.IgnoreCase))
            {
                var branchItem = db.Branches[tpath];
                string bname = branchItem.Name;
                Sitecore.Diagnostics.Log.Warn($"MakeChild: BRANCH Path={tpath}...name={bname}...", typeof(ItemExtensions));
                Item child = parent.Add(name, branchItem);
                return child;
            }
            else
            {
                var templateItem = db.Templates[tpath];
                string tname = templateItem.Name;
                Sitecore.Diagnostics.Log.Warn($"MakeChild: TEMPLATE Path={tpath}...name={tname}...", typeof(ItemExtensions));
                Item child = parent.Add(name, templateItem);
                return child;
            }
        }
    }
}
