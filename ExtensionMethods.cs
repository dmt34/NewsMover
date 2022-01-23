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
using System.Xml;
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
        ///   Produces a dictionary key for checking managed items.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   templateid + [optional] branchid
        /// </returns>
        /// <remarks>
        ///   compare with output of BaseMoverConfiguration.GetItemKey()
        /// </remarks>
        public static string ItemKey(this Item item)
        {
            if (item == null)
                return "";

            string key = item.TemplateID.ToShortID().ToString().ToLower();
            
            if (!Sitecore.Data.ID.IsNullOrEmpty(item.BranchId))
                key += item.BranchId.ToShortID().ToString().ToLower();

            return key;
        }
    }

    internal static class EnumExtensions
    {
        /// <summary>
        /// Gets the value of the description attribute from the enum
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }

    }
}
