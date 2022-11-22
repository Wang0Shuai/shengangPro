using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin.BillPlugin
{
    [HotUpdate]
    [Description("应付单")]
    public  class PayableBill : AbstractDynamicFormPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            DynamicObject obj_ImportBil = this.View.Model.DataObject;
        }

    }
}
