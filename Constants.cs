using Sitecore.Data;
using System.Collections.Generic;

namespace Sitecore.Sharedsource.NewsMover
{
    public static class Constants
    {
        public static Dictionary<SortOrder, ID> SortOrderValueIDs = new Dictionary<SortOrder, ID>()
        {
            { SortOrder.Ascending, new ID("{781247D2-9785-400F-8935-C818EC757967}") },
            { SortOrder.Descending, new ID("{C3E3F0E3-0162-4F1F-AB3E-40348E371A3F}") }
        };

        // content items under news mover management need a string value for the "News Mover Id" field;
        // here we need the ID of that field
        // (the field value must match one of the itemKey attributes in NewsMover.config)
        public static ID NewsMoverTargetIdField = new ID("{C35FD03F-9A2F-4F1C-8998-FF0195999D3B}");

        // years, months, days, alphabet folders need a string value for the "Folder Id" field;
        // here we need the ID of that field
        // (the field value must match one of the folderKey attributes in NewsMover.config)
        public static ID NewsMoverFolderIdField = new ID("{A8771BF3-7FF6-4DEB-A5BC-FF0057BECA07}");

        // string needs to not accidentally match to any of the itemKey or folderKey values
        public static string NotManagedKey = "NOT NEWS MOVER MANAGED";

        //
        // for publishing newly created folder items, is there a workflow?
        //

        // string value of the workflow id, else empty string
        public static string FolderWorkflowId = "{CB5F73A9-9142-49CC-BB82-783229C66B76}";

        // if there is a workflow what is the final state?  string value of the final state, else empty string
        public static string FolderFinalWorkflowState = "{B8303640-1E77-4B01-A179-C921867C335C}";
    }
}
