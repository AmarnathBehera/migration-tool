using Kentico.Xperience.Admin.Base.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience
{
    public class GlobalsettingDTO
    {
        [CodeEditorComponent(Label = "Script Head", Order = 1)]
        public string ScriptHeadKeyValue { get; set; } = string.Empty;

        [CodeEditorComponent(Label = "Marketo Munchkin Code", Order = 2)]
        public string MarketoMunchkinCodeKeyValue { get; set; } = string.Empty;

        [CodeEditorComponent(Label = "Script Executed When Cookies Are Accepted", Order = 3)]
        public string ScriptExecutedWhenCookiesAreAcceptedKeyValue { get; set; } = string.Empty;

        [CodeEditorComponent(Label = "Script In Bottom Of Body", Order = 4)]
        public string ScriptInBottomOfBodyKeyValue { get; set; } = string.Empty;

        [CodeEditorComponent(Label = "Script In Top Of Body", Order = 5)]
        public string ScriptInTopOfBodyKeyValue { get; set; } = string.Empty;

        [CodeEditorComponent(Label = "Scripts Top Of Page Head", Order = 6)]
        public string ScriptsTopOfPageHeadKeyValue { get; set; } = string.Empty;
    }
    
}
