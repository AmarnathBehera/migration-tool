using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using CMS.DataEngine;
using CMS.OnlineForms;

namespace BDO.GWT.Core.Services;

public class FormsService
{

    private readonly IInfoProvider<BizFormInfo> formInfoProvider;

    public FormsService(IInfoProvider<BizFormInfo> formInfoProvider)
    {

        this.formInfoProvider = formInfoProvider;
    }

        public IEnumerable<BizFormInfo> GetAllForms() =>
        formInfoProvider.Get()
            .OrderByAscending(nameof(BizFormInfo.FormDisplayName))
            .ToList();

  
}
        

        
    

    


