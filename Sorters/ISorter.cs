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

using Sitecore.Data.Items;
using Sitecore.Sharedsource.NewsMover.Configuration;

namespace Sitecore.Sharedsource.NewsMover.Sorters
{
    public interface ISorter
    {
        void OrganizeItem(Item item, IMoverConfiguration config);
    }
}