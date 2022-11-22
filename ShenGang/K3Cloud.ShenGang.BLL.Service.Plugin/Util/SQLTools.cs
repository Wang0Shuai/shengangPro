using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
     public class SQLTools
    {
         public static string getKDMapData(Context ctx, string sql, string returnValue)
         {
             string value = string.Empty;
             using (DataTable refReader = DBUtils.ExecuteDataSet(ctx, sql).Tables[0])
             {
                 if (refReader.Rows.Count > 0)
                 {
                     value = Convert.ToString(refReader.Rows[0][returnValue]);
                 }
             }
             return value;
         }

    }
}
