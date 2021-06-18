namespace Sitecore.Sharedsource.NewsMover.Sorters
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Sharedsource.NewsMover.Configuration;
    using Sitecore.Sharedsource.NewsMover;

    public class DateSorter : BaseSorter
    {
        public DateSorter(Dictionary<ID, IMoverConfiguration> templates) : base(templates) { }

        /// <summary>
        /// Initializes the date field to the current date if there is no value.
        /// </summary>
        /// <param name="item">The article.</param>
        /// <param name="dateFieldName">Name of the date field.</param>
        /// <returns></returns>
        private DateTime EnsureAndGetDate(Item item, string dateFieldName)
        {
            Sitecore.Data.Fields.DateField dateField = item.Fields[dateFieldName];
            Sitecore.Diagnostics.Assert.IsNotNull(dateField, dateFieldName);
            DateTime result = dateField.DateTime;

            // if there is no value in the date field, then set it to now.
            if (string.IsNullOrEmpty(dateField.InnerField.Value))
            {
                using (new Sitecore.Data.Items.EditContext(item))
                {
                    dateField.Value = Sitecore.DateUtil.IsoNow;
                    result = Sitecore.DateUtil.IsoDateToDateTime(dateField.InnerField.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Create any necessary folders per date configuration
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        protected override Item CreateFolders(Item item, IMoverConfiguration config)
        {
            var c = (DateMoverConfiguration)config;
            DateTime articleDate = EnsureAndGetDate(item, c.DateField);

            // Assign each news item an explicit sortorder
            var sign = c.SortOrder == SortOrder.Descending ? "-" : string.Empty;
            string text = sign + DateUtil.ToIsoDate(articleDate).Substring(4).Replace("T", string.Empty);
            if (item[FieldIDs.Sortorder] != text)
            {
                using (new EditContext(item))
                {
                    item[FieldIDs.Sortorder] = text;
                }
            }

            Item root = GetRoot(item, c);

            // get/create the year folder
            root = GetOrCreateChild(root, c.YearFolder.Template, c.YearFolder.GetName(articleDate), config.SortOrder, articleDate.Year);

            // get/create any month -> day structure we need
            if (c.MonthFolder != null)
            {
                root = GetOrCreateChild(root, c.MonthFolder.Template, c.MonthFolder.GetName(articleDate), config.SortOrder, articleDate.Month);

                if (c.DayFolder != null)
                {
                    root = GetOrCreateChild(root, c.DayFolder.Template, c.DayFolder.GetName(articleDate), config.SortOrder, articleDate.Day);
                }
            }

            return root;
        }

        /// <summary>
        /// Gets the root of where we start organization.
        /// i.e. the parent of the 'year' node
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        private Item GetRoot(Item item, DateMoverConfiguration config)
        {
            Item parent = item.Parent;

            while (Templates.ContainsKey(parent.TemplateID) || IsMoverFolderItem(parent, config))
            {
                parent = parent.Parent;
            }

            return parent;
        }



        /// <summary>
        /// Determines whether the item is a year, month or day item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="config">The config.</param>
        /// <returns>
        ///   <c>true</c> if [is item year month or day] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMoverFolderItem(Item item, IMoverConfiguration config)
        {
            var c = (DateMoverConfiguration)config;
            return item.TemplateID == c.YearFolder.Template.ID
                    || (c.MonthFolder != null && item.TemplateID == c.MonthFolder.Template.ID)
                    || (c.DayFolder != null && item.TemplateID == c.DayFolder.Template.ID);
        }

        /// <summary>
        /// Gets or create child.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childName">Name of the child.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        protected Item GetOrCreateChild(Item parent, TemplateItem template, string childName, SortOrder sortOrder, int sortIndex)
        {
            Item child = parent.Children[childName];
            if (child == null)
            {
                child = parent.Add(childName, template);
            }

            if (sortOrder == SortOrder.Descending)
                sortIndex = sortIndex * -1;

            var sortValue = sortIndex.ToString();

            // enforce that sub-item sorting is set
            if (child[FieldIDs.Sortorder] != sortValue)
            {
                using (new Sitecore.Data.Items.EditContext(child))
                {
                    child.Fields[FieldIDs.Sortorder].Value = sortValue;
                }
            }

            return child;
        }
    }
}
