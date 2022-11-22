using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;
using Kingdee.BOS.Core.DynamicForm.PlugIn.WizardForm;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.FormWizard;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Util;

using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;

using Kingdee.BOS.App.Data;
using System.Data;
using Kingdee.BOS.JSON;
using Newtonsoft.Json.Linq;

using Newtonsoft.Json;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Log;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [HotUpdate]
    [Description("采销向导")]
    public class EDI_SZDR : AbstractWizardFormPlugIn
    {
        private NetworkCtrlResult _networkCtrlResult;
        private void ReleaseNetCtrl()
        {
            if (this._networkCtrlResult != null)
            {
                NetworkCtrlServiceHelper.CommitNetCtrl(base.Context, this._networkCtrlResult);
                this._networkCtrlResult = null;
            }
        }

        private void startNetCtrl()
        {
            if (this._networkCtrlResult == null)
            {
                this._networkCtrlResult = this.StartNetCtl();
            }
        }

        private NetworkCtrlResult StartNetCtl()
        {
            LocaleValue name = new LocaleValue("计划向导", base.Context.UserLocale.LCID);
            NetworkCtrlObject networkCtrlObject = NetworkCtrlServiceHelper.AddNetCtrlObj(base.Context, name, base.GetType().Name, base.GetType().Name, NetworkCtrlType.BusinessObjOperateMutex, null, " ", true, true);
            NetworkCtrlServiceHelper.AddMutexNetCtrlObj(base.Context, networkCtrlObject.Id, networkCtrlObject.Id);
            NetWorkRunTimeParam para = new NetWorkRunTimeParam();
            NetworkCtrlResult networkCtrlResult = NetworkCtrlServiceHelper.BeginNetCtrl(base.Context, networkCtrlObject, para);
            if (networkCtrlResult.StartSuccess)
            {
                return networkCtrlResult;
            }
            base.View.ShowErrMessage("网络冲突：已经有其他进程正在运行计划向导，请稍等片刻再操作！", "", MessageBoxType.Notice);
            return null;
        }
        public override void WizardStepChanged(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm.WizardStepChangedEventArgs e)
        {
            base.WizardStepChanged(e);
            int stepindex = e.WizardStep.WizardIndex;
        }
        int currentIndex = 0;

        public override void WizardStepChanging(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.WizardForm.WizardStepChangingEventArgs e)
        {

            base.WizardStepChanging(e);
            currentIndex = e.NewWizardStep.WizardIndex;

        }



        /// <summary>
        /// 按钮点击事件,
        /// </summary>
        /// <param name="e"></param>
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            //DateTime closeDate = Convert.ToDateTime(this.Model.GetValue("F_PATC_ForecastPeriod"));
            //DynamicObject productLines = this.Model.GetValue("F_PATC_SYB") as DynamicObject;

            //if (e.Key.ToUpperInvariant() == "F_PATC_PCLABUTTON" || e.Key.ToUpperInvariant() == "F_PATC_ONPLLINK")
            //{
            //    if (this.View.CurrentWizardStep.Key.ToUpperInvariant() == "FWizard0".ToUpperInvariant())//
            //    {

            //        if (closeDate == null || closeDate < Convert.ToDateTime("1970-01-01"))//
            //        {
            //            this.View.ShowErrMessage("选择的日期不正确");
            //            e.Cancel = true;
            //            return;
            //        }
            //        if (productLines == null)
            //        {
            //            this.View.ShowErrMessage("事业部为必录项！");
            //            e.Cancel = true;
            //            return;
            //        }
            //    }
            //}
        }


        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.EqualsIgnoreCase("F_SHKD_ALLOCATEPEROID"))
            {
                DateTime allocateperoid = Convert.ToDateTime(this.Model.GetValue("F_SHKD_ALLOCATEPEROID"));
                DateTime ycperoid = allocateperoid.AddMonths(-1);
                this.Model.SetValue("F_SHKD_YCPEROID", ycperoid);
            }
        }

        private String getPeroid()
        {
            DateTime ycperoid = Convert.ToDateTime(this.Model.GetValue("F_SHKD_YCPEROID"));
            return ycperoid.ToString("yyyy") + ycperoid.ToString("MM");

        }

        private DateTime GetPeroidDate()
        {
            DateTime ycperoid = Convert.ToDateTime(this.Model.GetValue("F_SHKD_YCPEROID"));
            return ycperoid;

        }

        /// <summary>
        /// 按钮单击后事件,
        /// 用途：调用最终服务
        /// </summary>
        /// <param name="e"></param>
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);

            //DateTime ycPeroid = Convert.ToDateTime(this.Model.GetValue("F_PATC_ForecastPeriod"));
            //string ycQj = ycPeroid.ToString("yyyy-MM");
            //string startDate = "";// DataUtils.getDateBegin(ycPeroid);
            //string endDate = "";// DataUtils.getDateEnd(ycPeroid);

            //DynamicObject dosyb = this.Model.GetValue("F_PATC_SYB") as DynamicObject;
            //string fsyb = Convert.ToString(dosyb["Number"]);

            //string message = string.Empty;
            ////DynamicObjectCollection productLines = (DynamicObjectCollection)this.Model.GetValue("F_PATC_Entity") as DynamicObjectCollection;

            //DynamicObjectCollection productLines = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("F_PATC_Entity")) as DynamicObjectCollection;

            ////DynamicObjectCollection orgObjs1 = (DynamicObjectCollection)this.Model.DataObject["F_PATC_Entity"];

            //List<string> lines = new List<string>();
            //List<string> lines1 = new List<string>();
            //if (productLines != null && productLines.Count > 0)
            //{
            //    lines = (from p in productLines
            //             where Convert.ToInt32(p["F_PATC_DealerCode_Id"]) > 0
            //             select Convert.ToString(((DynamicObject)p["F_PATC_DealerCode"])["Number"])).Distinct().ToList();

            //    lines1 = (from p in productLines
            //              where Convert.ToInt32(p["F_PATC_MaterialNumber_Id"]) > 0
            //              select Convert.ToString(((DynamicObject)p["F_PATC_MaterialNumber"])["Number"])).Distinct().ToList();
            //}

            switch (e.Key.ToUpperInvariant())
            {
                case "F_RCQV_EDISZDR":
                    DynamicFormShowParameter showParam = new DynamicFormShowParameter();
                    showParam.FormId = "RCQV_EDISZDR";
                    // showParam.CustomParams.Add("FCustId", FCustId.ToString());
                    this.View.ShowForm(showParam, formResult =>
                    {
                        if (formResult != null && formResult.ReturnData != null)
                        {

                            //DynamicObject[] dataEntities = formResult.ReturnData as DynamicObject[];

                            //PushDataIntoSoEn(dataEntities);
                        }
                    }

                     );

                    break;
                case "F_PATC_ONPLLINK":
                    ListShowParameter parameter = new ListShowParameter();
                    parameter.FormId = "PATC_OrderDemandPlan";
                    parameter.PageId = Guid.NewGuid().ToString();
                    //parameter.ParentPageId = parentPageId;
                    parameter.OpenStyle.ShowType = ShowType.MainNewTabPage;
                    parameter.IsIsolationOrg = false;
                    //parameter.CustomParams.Add("", "");
                    ListRegularFilterParameter filterParameter = new ListRegularFilterParameter();
                    //filterParameter.Filter = string.Format(" ( FDATE between '{0}' and '{1}' ) ", startDate, endDate);
                    parameter.ListFilterParameter = filterParameter;
                    this.View.ShowForm(parameter);

                    break;

                case "F_SHKD_TOTALINV": break;

                default:
                    break;
            }
        }
        public object ExecuteService(DateTime ycPeroid, string fsyb, List<string> lines, List<string> lines1)
        {

            string various = string.Empty;
            string selSQL = string.Empty;
            string result = string.Empty;

            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            JSONObject obj = new JSONObject();
            JSONObject model = null;
            JSONObject data = null;

            JSONObject entryData = null;
            JSONArray entry = null;

            JSONArray models = new JSONArray();

            //obj.Add("IsAutoSubmitAndAudit", "true");


            obj.Add("BatchCount", "20");
            obj.Add("Model", models);
            for (int k = 0; k < lines.Count; k++)
            {
                ////校验
                selSQL = string.Format(@"/*dialect*/select a.FID  from PATC_t_OrderPlan a
left join PATC_t_OrderPlanEntry b on a.FID = b.FID
left join SHKD_CPXBill d on a.F_PATC_BA = d.FID
left join T_ORG_Organizations c on  a.F_PATC_CUSTOMER = c.FORGID

where  left(convert(varchar,a.FDATE,120),7) = '{0}' 
and d.fnumber ='{1}'
and c.FNUMBER='{2}' 
",
              ycPeroid.ToString("yyyy-MM"), fsyb, lines[k].ToString().Trim());

                result = "";// DataUtils.getKDMapData(this.Context, selSQL, "FID");
                if (!string.IsNullOrEmpty(result))
                {
                    dictionary["Result"] = "Fail";
                    dictionary["Msg"] = "经销商编码为 " + lines[k].ToString().Trim().ToString() + "存在已有的“订购需求计划单”";
                    dictionary["Data"] = "";
                    return dictionary;
                }
                ////
                model = new JSONObject();
                entry = new JSONArray();
                various = lines[k].ToString().Trim();
                if (!string.IsNullOrEmpty(various))
                {
                    data = new JSONObject();
                    data.Add("FNUMBER", various);
                    model.Add("F_PATC_Customercode", data);
                    data = new JSONObject();
                    data.Add("FNUMBER", various);
                    model.Add("F_PATC_CUSTOMER", data);
                }

                model.Add("FDATE", ycPeroid);
                model.Add("F_PATC_PlanPeriod", ycPeroid.AddMonths(1).ToString("yyyy.MM") + "-" + ycPeroid.AddMonths(6).ToString("yyyy.MM"));

                data = new JSONObject();
                data.Add("FNUMBER", fsyb);
                model.Add("F_PATC_BA", data);
                //--------分录
                model.Add("FEntity", entry);
                for (int j = 0; j < lines1.Count; j++)
                {
                    entryData = new JSONObject();

                    various = lines1[j].ToString().Trim();
                    if (!string.IsNullOrEmpty(various))
                    {
                        ////校验
                        selSQL = string.Format(@"/*dialect*/select a.FID
                                        from SHKD_T_CUSTOMERMATERIAL a
                                        left join SHKD_T_CUSTMATERIALENTRY b on a.FID = b.FID
                                        left join T_BD_CUSTOMER c on  a.F_SHKD_CUSTOMER = c.FCUSTID
                                        left join T_BD_MATERIAL d on b.F_PATC_MATERIAL = d.FMATERIALID
                                        left join SHKD_CPXBill e on e.FID = d.F_SHKD_CPX
                                        where  left(convert(varchar,a.F_SHKD_QJ,120),7) = '{0}' 
                                        and e.FNUMBER='{3}'
                                        and c.FNUMBER ='{1}'
                                        and d.FNUMBER='{2}' 
                                            ",
                    ycPeroid.ToString("yyyy-MM"), lines[k].ToString().Trim(), lines1[j].ToString().Trim(), fsyb);

                        result = "";// DataUtils.getKDMapData(this.Context, selSQL, "FID");
                        if (string.IsNullOrEmpty(result))
                        {
                            continue;
                        }
                        data = new JSONObject();
                        data.Add("FNUMBER", various);
                        entryData.Add("F_PATC_MATERIAL", data);
                    }
                    entry.Add(entryData);
                }
                if (entry.Count < 1)
                {
                    continue;
                }
                models.Add(model);
            }
            if (models.Count < 1)
            {
                throw new KDBusinessException("物料", "没有符合条件的数据（物料：事业部和可销控制）");
            }
            StringBuilder stringBuilder = new StringBuilder();
            #region 开始保存


            //using (KDTransactionScope trans = new KDTransactionScope(TransactionScopeOption.Required))
            //{
            string odpJson = JsonConvert.SerializeObject(obj);
            //保存
            object saveResult = WebApiServiceCall.BatchSave(this.Context, "PATC_OrderDemandPlan", odpJson);
            Dictionary<String, object> responseStatus = ((Dictionary<String, object>)((Dictionary<String, object>)saveResult)["Result"])["ResponseStatus"] as Dictionary<String, object>;
            Dictionary<String, object> responseResult = ((Dictionary<String, object>)((Dictionary<String, object>)saveResult)["Result"]) as Dictionary<String, object>;

            if (responseStatus["IsSuccess"].ToString() == "True")
            {
                // {"Result":{"ResponseStatus":{"IsSuccess":true,"Errors":[],"SuccessEntitys":[{"Id":100074,"Number":"FKSQ000016","DIndex":0}],"SuccessMessages":[],"MsgCode":0},"Id":100074,"Number":"FKSQ000016","NeedReturnData":[{}]}}


                List<object> success = responseStatus["SuccessEntitys"] as List<object>;

                //trans.Complete();
                dictionary["Result"] = "Success";
                dictionary["Msg"] = "执行成功";
                dictionary["Data"] = success;
            }
            else
            {
                stringBuilder.AppendLine("保存失败,原因如下：");
                List<object> errors = responseStatus["Errors"] as List<object>;
                for (int i = 0; i < errors.Count(); i++)
                {
                    Dictionary<string, object> keyValues = errors[i] as Dictionary<string, object>;
                    StringBuilder tip = new StringBuilder();
                    string errorsTip = Convert.ToString(keyValues["FieldName"]);

                    stringBuilder.AppendLine(errorsTip + "\r\n" + "第" + keyValues["DIndex"] + "行分录:" + keyValues["Message"]);
                }
                dictionary["Result"] = "Fail";
                dictionary["Msg"] = stringBuilder.ToString();
                dictionary["Data"] = errors;
            }
            //}
            #endregion
            return dictionary;
        }

        public void ExecuteServiceBySql(DateTime ycPeroid, string fsyb, List<string> custlines, List<string> matlines)
        {

            string various = string.Empty;
            string selSQL = string.Empty;
            Dictionary<int, string> headDic = new Dictionary<int, string>();
            Dictionary<int, string> entryDic = new Dictionary<int, string>();
            string headExceSQL = string.Empty;
            string headSQL = string.Format(@"/*dialect*/insert into PATC_t_OrderPlan
                            (FID,FBILLNO,FDOCUMENTSTATUS,FCREATORID,FCREATEDATE,
                            FMODIFIERID,FMODIFYDATE,F_PATC_ORGID,FDATE,
                            F_PATC_CUSTOMER,F_PATC_BA,F_PATC_ISPUBLISH,F_PATC_ISPUBLISH1,F_PATC_CUSTOMERCODE,
                            F_PATC_NOTE,	F_PATC_PLANPERIOD		) 
                            values");
            string entryExceSQL = string.Empty;
            string entrySQL = string.Format(@"/*dialect*/insert into PATC_t_OrderPlanEntry
                            (FID,FEntryID,F_PATC_MATERIAL,F_PATC_ISTB,
                            F_PATC_QUANTITYONE,F_PATC_QUANTITYTWO,F_PATC_QUANTITYTHREE,F_PATC_QUANTITYFOUR,
                            F_PATC_QUANTITYFIVE,	F_PATC_QUANTITYSIX)
                            values");
            string headSQLs = string.Empty;
            string entrySQLs = string.Empty;

            string result = string.Empty;

            string mats = string.Empty;//string.Join(",", custlines);
            foreach (string item in matlines)
            {
                mats += "'" + item + "',";
            }
            mats = mats.TrimEnd(',');
            string custs = string.Empty;//string.Join(",", custlines);
            foreach (string item in custlines)
            {
                custs += "'" + item + "',";
            }
            custs = custs.TrimEnd(',');
            selSQL = string.Format(@"/*dialect*/select c.FNUMBER
from PATC_t_OrderPlan a
left join PATC_t_OrderPlanEntry b on a.FID = b.FID
left join SHKD_CPXBill d on a.F_PATC_BA = d.FID
left join T_ORG_Organizations c on  a.F_PATC_CUSTOMER = c.FORGID
where  left(convert(varchar,a.FDATE,120),7) = '{0}' 
and d.fnumber ='{1}'
and c.FNUMBER in ({2})
",
              ycPeroid.ToString("yyyy-MM"), fsyb, custs);
            using (DataTable dt = DBUtils.ExecuteDataSet(this.Context, selSQL).Tables[0])
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    custlines.Remove(Convert.ToString(dt.Rows[i]["FNUMBER"]));
                }
            }
            int headCount = 0;
            int entryCount = 0;

            int headCountIndex = 0;
            int entryCountIndex = 0;
            for (int k = 0; k < custlines.Count; k++)
            {
                headCount++;
                if (headCount == 1000)
                {
                    headCountIndex++;
                    headCount = 0;
                    headDic.Add(headCountIndex, headSQLs.TrimEnd(','));
                    headSQLs = string.Empty;
                }
                selSQL = string.Format(@"/*dialect*/ select a.FORGID,b.fcustid,c.FID  from
(select top 1 FORGID from T_ORG_Organizations where fnumber ='{0}') a,
(select top 1 fcustid  from T_BD_CUSTOMER where fnumber = '{1}' and FUSEORGID = {2}) b,
(select top 1  FID  from SHKD_CPXBill where FNUMBER='{3}') c ", custlines[k].ToString().Trim(), custlines[k].ToString().Trim(), 1, fsyb);
                DataTable dt = DBUtils.ExecuteDataSet(this.Context, selSQL).Tables[0];
                string FORGID = string.Empty;
                string fcustid = string.Empty;
                string BA = string.Empty;
                if (dt.Rows.Count < 1)
                {
                    continue;
                }
                FORGID = (Convert.ToString(dt.Rows[0]["FORGID"]));
                fcustid = (Convert.ToString(dt.Rows[0]["fcustid"]));
                BA = (Convert.ToString(dt.Rows[0]["FID"]));



                selSQL = string.Format(@"/*dialect*/select d.FNUMBER
                                        from SHKD_T_CUSTOMERMATERIAL a
                                        left join SHKD_T_CUSTMATERIALENTRY b on a.FID = b.FID
                                        left join T_BD_CUSTOMER c on  a.F_SHKD_CUSTOMER = c.FCUSTID
                                        left join T_BD_MATERIAL d on b.F_PATC_MATERIAL = d.FMATERIALID
                                        left join SHKD_CPXBill e on e.FID = d.F_SHKD_CPX
                                        where  left(convert(varchar,a.F_SHKD_QJ,120),7) = '{0}' 
                                        and e.FNUMBER='{3}'
                                        and c.FNUMBER ='{1}'
                                        and d.FNUMBER in({2})
                                            ",
                    ycPeroid.ToString("yyyy-MM"), custlines[k].ToString().Trim(), mats, fsyb);
                dt = DBUtils.ExecuteDataSet(this.Context, selSQL).Tables[0];
                matlines.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    matlines.Add(Convert.ToString(dt.Rows[i]["FNUMBER"]));
                }

                if (matlines.Count < 1)
                {
                    continue;
                }
                long fid = DBServiceHelper.GetSequenceInt64(this.Context, "PATC_t_OrderPlan", 1).First();
                headSQLs += string.Format(@"('{0}','{1}','A','{2}','{3}'
                            ,'{6}','{7}','{8}','{9}',
                            '{10}','{11}','0','0','{12}',
                            '','{13}'),",
                            fid, "OP" + DateTime.Now.ToString("yyyyMMdd") + fid, this.Context.UserId, DateTime.Now, "",
                            "", this.Context.UserId, DateTime.Now, this.Context.CurrentOrganizationInfo.ID, ycPeroid,
                            FORGID, BA, fcustid,
                           ycPeroid.AddMonths(1).ToString("yyyy.MM") + "-" + ycPeroid.AddMonths(6).ToString("yyyy.MM"));

                for (int i = 0; i < matlines.Count; i++)
                {
                    entryCount++;
                    if (entryCount == 1000)
                    {
                        entryCountIndex++;
                        entryCount = 0;
                        entryDic.Add(entryCountIndex, entrySQLs.TrimEnd(','));
                        entrySQLs = string.Empty;
                    }

                    long FEntryID = DBServiceHelper.GetSequenceInt64(this.Context, "PATC_t_OrderPlanEntry", 1).First();
                    selSQL = string.Format("select FMATERIALID  from T_BD_MATERIAL where fnumber = '{0}' and FUSEORGID = {1}", matlines[i], 1);
                    string matId = "";//DataUtils.getKDMapData(this.Context, selSQL, "FMATERIALID");
                    entrySQLs += string.Format(@"('{0}','{1}','{2}','0',
                            0,0,0,0,0,0),",
                                  fid, FEntryID, matId);
                }
            }
            if (string.IsNullOrEmpty(headSQLs))
            {
                throw new KDBusinessException("数据未匹配", "没有符合生成【订购需求计划】条件的数据。");
            }

            headDic.Add(headCountIndex + 1, headSQLs.TrimEnd(','));
            foreach (int item in headDic.Keys)
            {
                headExceSQL = (headSQL + headDic[item].ToString().TrimEnd(','));
                DBUtils.Execute(this.Context, headExceSQL);
            }

            entryDic.Add(entryCountIndex + 1, entrySQLs.TrimEnd(','));
            foreach (int item in entryDic.Keys)
            {
                entryExceSQL = (entrySQL + entryDic[item].ToString().TrimEnd(','));
                DBUtils.Execute(this.Context, entryExceSQL);
            }

        }

        public void ExecuteStoredProcedureService(DateTime ycPeroid, string fsyb, List<string> custlines, List<string> matlines)
        {
            List<SqlParam> sqlParam = new List<SqlParam>();
            DBUtils.ExecuteStoreProcedure(this.Context, "sp_OrderDemandPlanCal01", sqlParam);

            string various = string.Empty;
            string selSQL = string.Empty;
            Dictionary<int, string> custDic = new Dictionary<int, string>();
            Dictionary<int, string> matDic = new Dictionary<int, string>();

            string custs = string.Empty;
            string mats = string.Empty;
            int custCount = 0;
            int matCount = 0;

            int custCountIndex = 0;
            int matCountIndex = 0;

            foreach (string item in custlines)
            {
                custCount++;
                if (custCount == 1000)
                {
                    custCountIndex++;
                    custCount = 0;
                    custDic.Add(custCountIndex, custs.TrimEnd(','));
                    custs = string.Empty;
                }
                custs += "('" + item + "'),";
            }
            custs = custs.TrimEnd(',');
            custDic.Add(custCountIndex + 1, custs);
            foreach (int item in custDic.Keys)
            {
                selSQL = ("/*dialect*/insert into T_SHKD_Omip_cust(F_SHKD_CustNumber) values" + custDic[item].ToString());
                DBUtils.Execute(this.Context, selSQL);
            }

            foreach (string item in matlines)
            {
                matCount++;
                if (matCount == 1000)
                {
                    matCountIndex++;
                    matCount = 0;
                    matDic.Add(matCountIndex, mats.TrimEnd(','));
                    mats = string.Empty;
                }
                mats += "('" + item + "'),";
            }
            mats = mats.TrimEnd(',');
            matDic.Add(matCountIndex + 1, mats);
            foreach (int item in matDic.Keys)
            {
                selSQL = ("/*dialect*/insert into T_SHKD_Omip_mat(F_SHKD_MatNumber) values" + matDic[item].ToString());
                DBUtils.Execute(this.Context, selSQL);
            }
            /////////////
            sqlParam.Add((new SqlParam("@ycPeroid", KDDbType.DateTime, ycPeroid)));//配货期间
            sqlParam.Add((new SqlParam("@fsyb", KDDbType.String, fsyb)));
            sqlParam.Add((new SqlParam("@userId", KDDbType.Int64, this.Context.UserId)));
            sqlParam.Add((new SqlParam("@orgId", KDDbType.Int64, this.Context.CurrentOrganizationInfo.ID)));
            DBUtils.ExecuteStoreProcedure(this.Context, "sp_OrderDemandPlanCal", sqlParam);

            StringBuilder sb = new StringBuilder();
            selSQL = string.Format(@"/*dialect*/select F_SHKD_CustNumber from T_SHKD_Omip_cust");
            using (DataTable refReader = DBUtils.ExecuteDataSet(this.Context, selSQL).Tables[0])
            {
                for (int i = 0; i < refReader.Rows.Count; i++)
                {
                    sb.AppendLine(Convert.ToString(refReader.Rows[0]["F_SHKD_CustNumber"]));
                }
            }
            if (sb.Length > 0)
            {
                throw new KDBusinessException("OrderDemandPlanCal", "以下经销商计算失败:\n" + sb.ToString() + "\n  原因：经销商在" + ycPeroid.Year + "年" + ycPeroid.Month + "月" + fsyb + "事业部，已有订购需求计划生成。");
            }
        }

        /// <summary>
        /// zsh  更新内容
        /// </summary>
        /// <returns></returns>
        private DateTime getPlanDate()
        {
            DateTime planDate = Convert.ToDateTime(this.Model.GetValue("F_SHKD_ALLOCATEPEROID"));
            return planDate;
        }


        /// <summary>
        /// 校验操作结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool CheckResult(IOperationResult result, string formName)
        {
            bool isPass = true;
            if (result == null)
            {
                this.View.ShowMessage(string.Format("未获取到可生成{0}的数据！", formName), MessageBoxType.Advise);
            }
            else if (result.IsSuccess)
            {
                this.View.ShowMessage("操作完成。", MessageBoxType.Notice);
            }
            else
            {
                isPass = false;
                List<string> errList = new List<string>();
                if (result.GetFatalErrorResults().Count > 0)
                {
                    errList = (from err in result.GetFatalErrorResults() select err.Message).Distinct().ToList();
                }
                else if (result.GetWarningResults().Count > 0)
                {
                    errList = (from err in result.GetWarningResults() select err.Message).Distinct().ToList();
                }
                else if (result.ValidationErrors.Count > 0)
                {
                    errList = (from err in result.ValidationErrors select err.Message).Distinct().ToList();
                }
                else
                {
                    errList = new List<string>() { "未知错误" };
                }
                this.View.ShowK3Displayer(string.Format("生成{0}时发生错误", formName), errList);
            }
            return isPass;
        }

    }
}
