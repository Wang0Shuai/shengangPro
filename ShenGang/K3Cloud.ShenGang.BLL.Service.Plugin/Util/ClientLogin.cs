using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace K3Cloud.ShenGang.BLL.Service.Plugin.Utils
{
    public  class ClientLogin
    {

        public static string KingdeeLogin(K3CloudApiClient client, string dbId, string userName, string userPwd)
        {
            var loginResult = client.ValidateLogin(dbId, userName, userPwd, 2052);
            return loginResult;
        }
        public static string PdfSave(K3CloudApiClient client,  String formid, String billid, List<string> templateIds, string pdfName)
        {
            JObject json = new JObject();
            json.Add("formid", formid);
            json.Add("billid", billid);
            json.Add("templateIds", string.Join(",",templateIds));
            json.Add("pdfName", pdfName);
            var param = new object[] { json.ToString() };
            var responseOut = client.Execute<JObject>("SHKD.SSC.ROCHE.OMIPThirdPeriod.WebApi.ServicesStub.PdfService.PdfSave,SHKD.SSC.ROCHE.OMIPThirdPeriod.WebApi.ServicesStub", param);
            return responseOut.ToString();
        }
        public static Dictionary<string, string> GetLoginConfig(Context ctx)
        {
            Dictionary<string, string> loginConfig = new Dictionary<string, string>();
            string sql = "select top 1 F_PATC_APPIPANDPORT,F_PATC_TEXT1,F_PATC_TEXT2,F_PATC_ACCTID from PATC_t_OMIPComRefSet";
            using (DataTable refReader = DBUtils.ExecuteDataSet(ctx, sql).Tables[0])
            {
                if (refReader.Rows.Count == 0)
                {
                    throw new Exception("获取OMIP公告参数配置失败");
                }
                for (int i = 0; i < refReader.Rows.Count; i++)
                {
                    loginConfig.Add("apiHost", Convert.ToString(refReader.Rows[i]["F_PATC_APPIPANDPORT"]) + "/k3cloud/");
                    loginConfig.Add("userName", Convert.ToString(refReader.Rows[i]["F_PATC_TEXT1"]));
                    loginConfig.Add("userPwd", Convert.ToString(refReader.Rows[i]["F_PATC_TEXT2"]));
                    loginConfig.Add("dbId", Convert.ToString(refReader.Rows[i]["F_PATC_ACCTID"]));
                }
            }
            return loginConfig;
        }
    }
}
