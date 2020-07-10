using Sitecore.Configuration;
using Sitecore.Sharedsource.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Sharedsource.NewsMover
{
    public static class DefaultSettings
    {
        public static SortOrder SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), Settings.GetSetting("NewsMover.DefaultSort", "Ascending"), true);
        public static string YearFormat = Sitecore.Configuration.Settings.GetSetting("NewsMover.DefaultYearFormat", "yyyy");
        public static string MonthFormat = Sitecore.Configuration.Settings.GetSetting("NewsMover.DefaultMonthFormat", "MM");
        public static string DayFormat = Sitecore.Configuration.Settings.GetSetting("NewsMover.DefaultDayFormat", "dd");
    }
}
