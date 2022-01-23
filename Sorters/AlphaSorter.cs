using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Sharedsource.NewsMover.Configuration;
using Sitecore.Shell.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Sharedsource.NewsMover.Sorters
{
    public class AlphaSorter : BaseSorter
    {
        public AlphaSorter(Dictionary<string, IMoverConfiguration> templates) : base(templates) { }


        protected override Item CreateFolders(Item item, IMoverConfiguration config)
        {
            var c = (AlphaMoverConfiguration)config;
            var itemName = item.Name;
            var root = GetRootItem(item, c);

            root = GetOrCreateChild(root, c.FolderTemplate, GetName(item, c), c.SortOrder);

            return root;
        }

        /// <summary>
        /// Use either the item name or the first non-empty SortField
        /// </summary>
        private static string GetName(Item item, AlphaMoverConfiguration config)
        {
            var name = item.Name[0].ToString();

            if (config.SortFields.Length > 0)
            {
                var f = config.SortFields.FirstOrDefault(x => !string.IsNullOrEmpty(item[x]));
                if (f != null)
                    name = item[f][0].ToString();
            }

            return name.ToUpper();
        }

        protected Item GetRootItem(Item item, AlphaMoverConfiguration config)
        {
            Item parent = item.Parent;

            while (Templates.ContainsKey(parent.ItemKey()) || IsMoverFolderItem(parent, config))
            {
                parent = parent.Parent;
            }

            var sortValue = Constants.SortOrderValueIDs[config.SortOrder].ToString();

            // enforce that sub-item sorting is set
            if (config.SortOrder != SortOrder.None && parent[FieldIDs.SubitemsSorting] != sortValue)
            {
                using (new Sitecore.Data.Items.EditContext(parent))
                {
                    parent.Fields[FieldIDs.SubitemsSorting].Value = sortValue;
                }
                Sorting.SortReset(parent);
            }

            return parent;
        }

        protected override bool IsMoverFolderItem(Item item, IMoverConfiguration config)
        {
            return item.TemplateID == ((AlphaMoverConfiguration)config).FolderTemplate.ID;
        }

        /// <summary>
        /// Gets the or create child.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        protected Item GetOrCreateChild(Item parent, TemplateItem template, string childName, SortOrder subItemSorting)
        {
            Item child = parent.Children[childName];
            if (child == null)
            {
                child = parent.Add(childName, template);
            }

            var sortValue = Constants.SortOrderValueIDs[subItemSorting].ToString();

            // enforce that sub-item sorting is set
            if (subItemSorting != SortOrder.None && child[FieldIDs.SubitemsSorting] != sortValue)
            {
                using (new Sitecore.Data.Items.EditContext(child))
                {
                    child.Fields[FieldIDs.SubitemsSorting].Value = sortValue;
                }
                Sorting.SortReset(parent);
            }

            return child;
        }
    }
}
