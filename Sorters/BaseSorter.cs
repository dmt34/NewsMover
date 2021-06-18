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

namespace Sitecore.Sharedsource.NewsMover.Sorters
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;
    using Sitecore.Sharedsource.NewsMover.Configuration;
    using Sitecore.Sharedsource.NewsMover.Pipelines;

    public abstract class BaseSorter : ISorter
    {
        protected Dictionary<ID, IMoverConfiguration> Templates { get; private set; }

        public BaseSorter(Dictionary<ID, IMoverConfiguration> templates)
        {
            Templates = templates;
        }

        public void OrganizeItem(Item item, IMoverConfiguration config)
        {
            var root = CreateFolders(item, config);

            // if the item is already where it should be, then bail out
            if (!string.Equals(item.Parent.Paths.FullPath, root.Paths.FullPath, StringComparison.OrdinalIgnoreCase))
            {

                // save the original location so we can clean up 
                Item originalParent = item.Parent;

                // move the item to the proper location
                item.MoveTo(root);

                // delete the original parent if there are no children.
                // keep walking up while we are a year/month/day
                while ((!originalParent.HasChildren) && IsMoverFolderItem(originalParent, config))
                {
                    Item parent = originalParent.Parent;
                    originalParent.Delete();
                    originalParent = parent;
                }

            }

            if ((!Sitecore.Context.IsBackgroundThread) && Sitecore.Context.ClientPage.IsEvent)
            {
                var args = new MoveCompletedArgs() { Article = item, Root = item.Database.GetRootItem() };
                CorePipeline.Run("NewsMover.MoveCompleted", args);
            }
        }

        
        /// <summary>
        /// Creates necessary folders
        /// </summary>
        /// <param name="item"></param>
        /// <param name="config"></param>
        /// <returns>Return leaf folder</returns>
        protected abstract Item CreateFolders(Item item, IMoverConfiguration config);

        protected abstract bool IsMoverFolderItem(Item item, IMoverConfiguration config);

    }
}