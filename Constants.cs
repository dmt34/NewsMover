using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Sharedsource.NewsMover
{
    public static class Constants
    {
        public static Dictionary<SortOrder, ID> SortOrderValueIDs = new Dictionary<SortOrder, ID>()
        {
            { SortOrder.Ascending, new ID("{781247D2-9785-400F-8935-C818EC757967}") },
            { SortOrder.Descending, new ID("{C3E3F0E3-0162-4F1F-AB3E-40348E371A3F}") }
        };
    }
}
