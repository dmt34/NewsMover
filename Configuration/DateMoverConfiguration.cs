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
    using System.Xml;
    using Sitecore.Data;
    using Sitecore.Sharedsource.NewsMover;

    public class DateMoverConfiguration : BaseMoverConfiguration
    {
        private string _yearTemplate, _monthTemplate, _dayTemplate, _yearFormat, _monthFormat, _dayFormat;

        /// <summary>
        /// Gets the year folder.
        /// </summary>
        public Folder YearFolder { get; private set; }

        /// <summary>
        /// Gets the month folder.
        /// </summary>
        public Folder MonthFolder { get; private set; }

        /// <summary>
        /// Gets the day folder.
        /// </summary>
        public Folder DayFolder { get; private set; }

        /// <summary>
        /// Gets the date field.
        /// </summary>
        public string DateField { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateConfiguration"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="itemKey">The target item id string.</param>
        /// <param name="folderKey">Id string for years, months, days.</param>
        /// <param name="sort">The sort.</param>
        /// <param name="dateField">The date field.</param>
        /// <param name="yearTemplate">The year template.</param>
        /// <param name="monthTemplate">The month template.</param>
        /// <param name="dayTemplate">The day template.</param>
        /// <param name="yearFormat">The year format.</param>
        /// <param name="monthFormat">The month format.</param>
        /// <param name="dayFormat">The day format.</param>
        internal DateMoverConfiguration(Database database, string itemKey, string folderKey, SortOrder sort, string dateField, string yearTemplate, string monthTemplate, string dayTemplate, string yearFormat = null, string monthFormat = null, string dayFormat = null)
            : base (database, itemKey, folderKey, sort)
        {

            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(dateField, "DateField");
            Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(yearTemplate, "YearTemplate");

            _yearTemplate = yearTemplate;
            _monthTemplate = monthTemplate;
            _dayTemplate = dayTemplate;
            _yearFormat = yearFormat ?? DefaultSettings.YearFormat;
            _monthFormat = monthFormat ?? DefaultSettings.MonthFormat;
            _dayFormat = dayFormat ?? DefaultSettings.DayFormat;
            DateField = dateField;

            CreateFolders();
        }

        /// <summary>
        /// Creates the folders.
        /// </summary>
        private void CreateFolders()
        {
            // we at least need a year template
            YearFolder = new Folder(_yearTemplate, _yearFormat);

            // we may want to organize in months too
            if (!string.IsNullOrEmpty(_monthTemplate))
            {
                MonthFolder = new Folder(_monthTemplate, _monthFormat);
            }

            // we may also want to put in day folders
            if (!string.IsNullOrEmpty(_dayTemplate))
            {
                DayFolder = new Folder(_dayTemplate, _dayFormat);
            }

            // make sure we have a Month if we have a Day
            Sitecore.Diagnostics.Assert.IsFalse(MonthFolder == null && DayFolder != null, "dayTemplate without monthTemplate");
        }

    }
}
