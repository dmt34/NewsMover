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
    using Sitecore.Data.Items;
    using Sitecore.Pipelines;
    using Sitecore.Publishing;
    using Sitecore.Sharedsource.NewsMover.Configuration;
    using Sitecore.Sharedsource.NewsMover.Pipelines;
    using System;
    using System.Collections.Generic;

    public abstract class BaseSorter : ISorter
    {
        protected Dictionary<string, IMoverConfiguration> Templates { get; private set; }

        public BaseSorter(Dictionary<string, IMoverConfiguration> templates)
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
                var args = new MoveCompletedArgs() {
                    Article = item,
                    Root = item.Database.GetRootItem()
                };
                CorePipeline.Run("NewsMover.MoveCompleted", args);
            }
        } 

        public void PublishNewItem(Item item)
        {
            // if there is a workflow, set the workflow state to the final state
            var itemWorkflow = item.Fields[FieldIDs.Workflow]?.Value;
            if (itemWorkflow == Constants.FolderWorkflowId)
            {
                using (new Sitecore.Data.Items.EditContext(item))
                {
                    item.Fields[FieldIDs.WorkflowState].Value = Constants.FolderFinalWorkflowState;
                }
            }

            // send to web db
            var web = Sitecore.Configuration.Factory.GetDatabase("web");
            var master = Sitecore.Configuration.Factory.GetDatabase("master");

            Sitecore.Data.Database[] targets = { web };
            Sitecore.Globalization.Language[] languages = master.Languages;
            bool deep = false;
            bool smart = true;
            PublishManager.PublishItem(item, targets, languages, deep, smart);
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