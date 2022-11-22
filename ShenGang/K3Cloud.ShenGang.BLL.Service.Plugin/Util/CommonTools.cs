using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Log;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.FileServer.Core;
using Kingdee.BOS.FileServer.Core.Object;
using Kingdee.BOS.FileServer.ProxyService;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using K3Cloud.ShenGang.BLL.Service.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    public class CommonTools
    {
        public static string EmailSendListFormId ="";// BBCConst.EmailSendListFormId;//邮件发送列表标识
        public static bool IsNullOrWhiteSpaceOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// BASE64编码
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Base64Encode(string s)
        {
            try
            {
                s = Convert.ToBase64String(Encoding.Default.GetBytes(s));
                return Convert.ToBase64String(Encoding.Default.GetBytes(s));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// BASE64解码
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Base64Decode(string s)
        {
            try
            {
                s = Encoding.Default.GetString(Convert.FromBase64String(s));
                return Encoding.Default.GetString(Convert.FromBase64String(s));
            }
            catch
            {
                return null;
            }
        }
        public static void OMIPCheckTask(Context ctx, Schedule schedule, Exception e, string taskType)
        {
            LogServiceHelper.WriteLog(ctx, new LogObject()
            {
                ObjectTypeId = "PATC_EmailTemplateForOmip",
                OperateName = "OMIP_发送邮件",
                Environment = OperatingEnvironment.BizOperate,
                Description = "(OMIPCheckTask)执行计划:" + taskType
            });

            string excSql = string.Format(@"
select  b.F_PATC_IsIntervene,b.F_PATC_IsExToStart,b.F_PATC_TimeInterval,c.FNUMBER,d.FNAME            
                from PATC_t_OMIPComRefSet a
				left join PATC_t_OMIPComRefSetEntry b on a.FID = b.FID
				left join PATC_t_OMIPTaskExCtrl c on b.F_PATC_TASKNAME = c.FID
				left join PATC_t_OMIPTaskExCtrl_L d on c.FID = d.FID and d.FLocaleID=2052
where d.FNAME ='{0}'", taskType);
            string IsIntervene = string.Empty;
            string IsExToStart = string.Empty;
            double TimeInterval = 0;
            using (DataTable dt = DBUtils.ExecuteDataSet(ctx, excSql).Tables[0])
            {
                if (dt.Rows.Count > 0)
                {
                    IsIntervene = dt.Rows[0]["F_PATC_IsIntervene"].ToString();
                    IsExToStart = dt.Rows[0]["F_PATC_IsExToStart"].ToString();
                    TimeInterval = Convert.ToDouble(dt.Rows[0]["F_PATC_TimeInterval"]);

                    LogServiceHelper.WriteLog(ctx, new LogObject()
                    {
                        ObjectTypeId = "PATC_EmailTemplateForOmip",
                        OperateName = "OMIP_发送邮件",
                        Environment = OperatingEnvironment.BizOperate,
                        Description = "执行计划-" + taskType + "：" + IsIntervene + "|" + IsExToStart + "|" + TimeInterval
                    });

                    if ("1".Equals(IsIntervene) && "1".Equals(IsExToStart))
                    {
                        // 恢复任务状态、下次执行时间
                        schedule.Status = ScheduleStatus.Ready;
                        schedule.ExecuteTime = TimeServiceHelper.GetSystemDateTime(ctx).AddMinutes(TimeInterval - schedule.ExecuteInterval);
                    }
                }
            }
        }
        
        public static void WritingLog(Context ctx, bool IsSuccess, string Types, string billNo,
                                      bool IsFromTask, string Description, string reason, string data)
        {
            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;

            //IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            //FormMetadata matMetaData = metaService.Load(this.KDContext.Session.AppContext, "PATC_MaterialLogs") as FormMetadata;
            //BusinessInfo materialInfo = matMetaData.BusinessInfo;
            //DynamicObject DynamicObject = null;

            //ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            //List<DynamicObject> objs = new List<DynamicObject>();
            //IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            //DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            //DynamicObject["F_PATC_IsSuccess"] = false;
            //DynamicObject["F_PATC_Types"] = "SAPToMT";
            //DynamicObject["F_PATC_MD5"] = md5Res;
            //DynamicObject["F_PATC_FailReason"] = reason + "\r\n" + data;

            //objs.Add(DynamicObject);

            ////IOperationResult result = saveService.Save(this.KDContext.Session.AppContext, materialInfo, objs.ToArray(), null, "Save");
            //IOperationResult result = BusinessDataServiceHelper.Save(this.KDContext.Session.AppContext, materialInfo, objs.ToArray(), null, "Save");
            JObject jData = new JObject();
            JObject jModle = new JObject();
            jData.Add("Model", jModle);

            jModle.Add("F_PATC_IsSuccess", IsSuccess);
            jModle.Add("F_PATC_Datetime", DateTime.Now);
            jModle.Add("F_PATC_Types", Types);
            jModle.Add("F_PATC_IsFromTask", IsFromTask);
            jModle.Add("F_PATC_BillNo", billNo);
            jModle.Add("F_PATC_Description", Description);
            jModle.Add("F_PATC_FailReason", reason);
            jModle.Add("F_PATC_Json", data);
            
            string inData = JsonConvert.SerializeObject(jData);
            object saveResult = WebApiServiceCall.Save(ctx, "PATC_BBCInterfaceLogs", inData);
        }

        public static void WritingLogForCustmoer(Context ctx, bool IsSuccess, string billNo,
                                       string reason, string data)
        {
            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;

            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, "PATC_CustomerInterfaceLogs") as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject DynamicObject = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            DynamicObject["F_PATC_IsSuccess"] = IsSuccess;
            DynamicObject["F_PATC_BillNo"] = billNo;
            DynamicObject["FCreateDate"] = DateTime.Now;
            
            DynamicObject["F_PATC_Datetime"] = DateTime.Now;
            DynamicObject["F_PATC_FailReason"] = reason ;
            DynamicObject["F_PATC_Json"] = data;
            objs.Add(DynamicObject);

            //IOperationResult result = saveService.Save(this.KDContext.Session.AppContext, materialInfo, objs.ToArray(), null, "Save");
            IOperationResult result = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");

            //JObject jData = new JObject();
            //JObject jModle = new JObject();
            //jData.Add("Model", jModle);

            //jModle.Add("F_PATC_IsSuccess", IsSuccess);

            //jModle.Add("F_PATC_BillNo", billNo);

            //jModle.Add("F_PATC_FailReason", reason + "\r\n" + data);

            //string inData = JsonConvert.SerializeObject(jData);
            //object saveResult = WebApiServiceCall.Save(ctx, "PATC_DOInterfaceLogs", inData);
        }
        public static void WritingLogForPriceSalTab(Context ctx, bool IsSuccess, string billNo,
                                      string reason, string data)
        {
            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;

            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, "PATC_SalPriceTabLogs") as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject DynamicObject = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            DynamicObject["F_PATC_IsSuccess"] = IsSuccess;
            DynamicObject["F_PATC_BillNo"] = billNo;
            DynamicObject["FCreateDate"] = DateTime.Now;
            DynamicObject["F_PATC_Datetime"] = DateTime.Now;
            
            DynamicObject["F_PATC_FailReason"] = reason;
            DynamicObject["F_PATC_Json"] = data;
            objs.Add(DynamicObject);

            //IOperationResult result = saveService.Save(this.KDContext.Session.AppContext, materialInfo, objs.ToArray(), null, "Save");
            IOperationResult result = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");

            //JObject jData = new JObject();
            //JObject jModle = new JObject();
            //jData.Add("Model", jModle);

            //jModle.Add("F_PATC_IsSuccess", IsSuccess);

            //jModle.Add("F_PATC_BillNo", billNo);

            //jModle.Add("F_PATC_FailReason", reason + "\r\n" + data);

            //string inData = JsonConvert.SerializeObject(jData);
            //object saveResult = WebApiServiceCall.Save(ctx, "PATC_DOInterfaceLogs", inData);
        }

        public static void WritingLogForDO(Context ctx, bool IsSuccess,  string billNo,
                                      string reason, string data)
        {

            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;
            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, "PATC_DOInterfaceLogs") as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject DynamicObject = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            DynamicObject["F_PATC_IsSuccess"] = IsSuccess;
            DynamicObject["F_PATC_BillNo"] = billNo;
            DynamicObject["FCreateDate"] = DateTime.Now;
            DynamicObject["F_PATC_Datetime"] = DateTime.Now;
            DynamicObject["F_PATC_FailReason"] = reason ;
            DynamicObject["F_PATC_Json"] = data;

            objs.Add(DynamicObject);

            //IOperationResult result = saveService.Save(this.KDContext.Session.AppContext, materialInfo, objs.ToArray(), null, "Save");
            IOperationResult result = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");

            //JObject jData = new JObject();
            //JObject jModle = new JObject();
            //jData.Add("Model", jModle);

            //jModle.Add("F_PATC_IsSuccess", IsSuccess);

            //jModle.Add("F_PATC_BillNo", billNo);

            //jModle.Add("F_PATC_FailReason", reason + "\r\n" + data);

            //string inData = JsonConvert.SerializeObject(jData);
            //object saveResult = WebApiServiceCall.Save(ctx, "PATC_DOInterfaceLogs", inData);
        }
        /// <summary>
        /// So接口日志
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="IsSuccess"></param>
        /// <param name="billNo"></param>
        /// <param name="reason"></param>
        /// <param name="data"></param>
        public static void WritingLogForSO(Context ctx, bool IsSuccess, string billNo,
                                      string reason, string data)
        {
            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;

            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, "PATC_SoLog") as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject DynamicObject = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            DynamicObject["F_PATC_IsSuccess"] = IsSuccess;
            DynamicObject["F_PATC_SONo"] = billNo;
            DynamicObject["FCreateDate"] = DateTime.Now;
            DynamicObject["F_PATC_Datetime"] = DateTime.Now;
            DynamicObject["F_PATC_FailReason"] = reason;
            DynamicObject["F_PATC_Json"] = data;

            objs.Add(DynamicObject);
            IOperationResult result = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");

        }
        /// <summary>
        /// 发票接口日志
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="IsSuccess"></param>
        /// <param name="billNo"></param>
        /// <param name="reason"></param>
        /// <param name="data"></param>
        public static void WritingLogForBilling(Context ctx, bool IsSuccess, string billNo,
                                   string reason, string data)
        {
            reason = reason.Length > 1999 ? reason.Substring(0, 1999) : reason;
            data = data.Length > 1999 ? data.Substring(0, 1999) : data;

            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, "PATC_BillingLog") as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject DynamicObject = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            DynamicObject = new DynamicObject(materialInfo.GetDynamicObjectType());
            DynamicObject["F_PATC_IsSuccess"] = IsSuccess;
            DynamicObject["F_PATC_InvoiceNo"] = billNo;
            DynamicObject["FCreateDate"] = DateTime.Now;
            DynamicObject["F_PATC_Datetime"] = DateTime.Now;
            DynamicObject["F_PATC_FailReason"] = reason;
            DynamicObject["F_PATC_Json"] = data;

            objs.Add(DynamicObject);
            IOperationResult result = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");

        }
        /// <summary>
        /// 邮件发送保存
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="title">标题</param>
        /// <param name="recivers">接收人</param>
        /// <param name="sender">发送人</param>
        /// <param name="content">邮件内容</param>
        /// <param name="targetFormId">需要生成附件的单据标识</param>
        /// <param name="templateId">需要生成附件的单据套打模板</param>
        /// <param name="attachment">单据是否需要生成附件</param>
        /// <param name="targetFids">需要生成附件的单据内码集合</param>
        public static string SaveEmail(Context ctx, string title, string recivers, string sender, string content, string wfnumber, string targetFormId, string templateId, bool attachment = false, string targetFid = null, K3CloudApiClient client = null)
        {

            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, EmailSendListFormId) as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            DynamicObject dyn = null;

            ISaveService saveService = Kingdee.BOS.App.ServiceHelper.GetService<ISaveService>();
            List<DynamicObject> objs = new List<DynamicObject>();
            IViewService viewService = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>();

            dyn = new DynamicObject(materialInfo.GetDynamicObjectType());
            dyn["F_PATC_Sender_Id"] = sender;
            dyn["F_PATC_Recivers"] = recivers;
            dyn["FCreateDate"] = DateTime.Now;
            dyn["F_PATC_Title"] = title;
            dyn["F_PATC_CONTENT"] = content;
            dyn["F_PATC_WFNumber"] = wfnumber;
            dyn["F_PATC_Status"] = 0;
            objs.Add(dyn);
            IOperationResult saveResult = BusinessDataServiceHelper.Save(ctx, materialInfo, objs.ToArray(), null, "Save");
            if (saveResult.IsSuccess)
            {
                long fid = 0;
                string billNo = string.Empty;

                foreach (var dataResult in saveResult.SuccessDataEnity)
                {
                    if (dataResult["Id"] != null)
                    {
                        fid = long.Parse(dataResult["Id"].ToString());
                        billNo = dataResult["BillNo"].ToString();
                    }
                }
                if (attachment && !string.IsNullOrEmpty(templateId)&&!string.IsNullOrEmpty(targetFid))//是否生成单据附件
                {
                    SaveAttatch(ctx, billNo, fid.ToString(),targetFid, targetFormId, templateId, EmailSendListFormId, client);
                }
                //if (!string.IsNullOrEmpty(targetFid))//获取目标单据附件
                //{
                //    var targetFids = new List<string>();
                //    targetFids.Add(targetFid);
                //    var dyns = FileHelper.GetFileData(ctx, targetFids, targetFormId);

                //    FileHelper.SaveAttachmentWithoutUpdate(ctx, dyns, EmailSendListFormId, billNo, fid.ToString());
                //}
                return fid.ToString();
            }
            return string.Empty;
        }
    
        /// <summary>
        /// 保存邮件发送附件
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="successDataEnity">邮件保存结果</param>
        /// <param name="targetFormId">需要生成附件的单据标识</param>
        /// <param name="templateId">需要生成附件的单据套打模板</param>
        /// <param name="formId">邮件发送单据标识</param>
        private static void SaveAttatch(Context ctx, string billNo, string fid,string targetFid, string targetFormId, string templateId, string formId, K3CloudApiClient client = null)
        {
            List<Dictionary<string, string>> uploadFiles = new List<Dictionary<string, string>>();
            int storageType = Kingdee.BOS.ServiceHelper.FileServer.FileServerHelper.GetFileStorgaeType(ctx);//文件服务设置
            List<string> templateIds = new List<string>();
            templateIds.Add(templateId);
            if (client!=null)//工作流情况需要调用api生成
            {
                ClientLogin.PdfSave(client, targetFormId, targetFid, templateIds, billNo);
            }
            else
            {
                TDPdfExportHelper.export(ctx, targetFormId, targetFid, templateIds, billNo);
            }
            string fileName = billNo + ".PDF";
            string filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, fileName);//文件存储路径
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("FID", fid);
            dic.Add("FBillNo", billNo);
            dic.Add("FilePath", filePath);
            dic.Add("FileName", fileName);
            uploadFiles.Add(dic);
            FileHelper.SaveAttachment(ctx, uploadFiles, formId, storageType);
          
        }

        /// <summary>
        /// 根据套打模板名称获取套打模板id
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public static string GetTemplateId(Context ctx,string templateName)
        {
            string templateSql = string.Format(@"select fnumber
                                                 from V_BOS_PRINTTEMPLATE a
                                                 left join V_BOS_PRINTTEMPLATE_l b on a.FID = b.FID 
                                                 where  b.FNAME ='{0}'  and  b.FLOCALEID='2052'
                                               ", templateName);
           var dyns= DBUtils.ExecuteDynamicObject(ctx, templateSql);
           if (dyns.Count>0)
           {
               return dyns[0][0].ToString();
            }
               return string.Empty;
        }

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="templateEnum"></param>
        /// <returns></returns>
        public static DynamicObject GetEmailTemplate(Context ctx, string templateEnum)
        {
            var filter = OQLFilter.CreateHeadEntityFilter(string.Format("FTYPE ='{0}'", templateEnum));
            var dyns = BusinessDataServiceHelper.Load(ctx, "PATC_EmailTemplateForOmip", null, filter);
            if (dyns.Count() > 0)
            {
                return dyns[0];
            }
            return null;
        }
        /// <summary>
        /// 转换Email模板
        /// </summary>
        /// <param name="bill">单据</param>
        /// <param name="str">待转换字符串</param>
        /// <returns>转换过的字符串</returns>
        public static string ReplaceValue(DynamicObject bill, string str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"\{[^\}]+\}*", options);
            MatchCollection matches = regex.Matches(str);
            for (int i = 0; i < matches.Count; i++)
            {
                string match = matches[i].Value;
                string matchFiled = match.Replace("{", "").Replace("}", "");
                if (matchFiled.Contains("."))
                {
                    var fileds = matchFiled.Split('.');
                    var dynObj = bill[fileds[0]] as DynamicObject;
                    string name = string.Empty;
                    if (dynObj != null)
                    {
                        name = dynObj[fileds[1]].ToString();
                    }
                    str = str.Replace(match, name);
                }
                else
                {
                    string name = bill[matchFiled].ToString();
                    str = str.Replace(match, name);
                }
            }
            return str;
        }
        /// <summary>
        /// 获取审核节点审核人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public static List<string> GetAuditor(Context ctx, string formId, string fid)
        {
            List<string> emailAddress = new List<string>();
            string sql = string.Format(@"SELECT 
                                        t_WF_Receiver.FReceiverId,
                                        t_SEC_User.FName,
                                        t_SEC_User.FEMAIL,
                                        t_WF_PiBiMap.FKeyValue
                                        FROM  t_WF_PiBiMap 
                                        INNER JOIN t_WF_ProcInst 
                                        ON (t_WF_ProcInst.FProcInstId = t_WF_PiBiMap.FProcInstId)
                                        INNER  JOIN t_WF_ActInst 
                                        on (t_WF_ActInst.FProcInstId = t_WF_ProcInst.FProcInstId)
                                        INNER JOIN t_WF_Assign on (t_WF_Assign.FActInstId = t_WF_ActInst.FActInstId)
                                        INNER JOIN t_WF_Receiver on (t_WF_Receiver.FAssignId = t_WF_Assign.FAssignId)
                                        INNER JOIN t_SEC_User ON (t_SEC_User.FUserId = t_WF_Receiver.FReceiverId)
                                        INNER JOIN t_WF_ApprovalAssign on (t_WF_Assign.FAssignId = t_WF_ApprovalAssign.FAssignId)
                                        LEFT JOIN t_WF_ApprovalItem on (t_WF_ApprovalItem.FApprovalAssignId = t_WF_ApprovalAssign.FApprovalAssignId AND t_WF_ApprovalItem.FReceiverId = t_WF_Receiver.FReceiverId)
                                        WHERE  t_WF_PiBiMap.FObjectTypeId = '{0}' 
                                        AND t_WF_PiBiMap.FKeyValue = '{1}'
                                        group by t_WF_Receiver.FReceiverId,
                                        t_SEC_User.FName,
                                        t_SEC_User.FEMAIL,
                                        t_WF_PiBiMap.FKeyValue", formId, fid);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dyns.Count > 0)
            {
                foreach(var dyn in dyns){
                    string address = dyn["FEMAIL"].ToString().Trim();
                    if (!string.IsNullOrEmpty(address))
                    {
                        emailAddress.Add(address);
                    }
                }

                
            }
            return emailAddress;
        }

        /// <summary>
        /// 获取工作流自动审核须发邮件人
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetWfAuditors(Context ctx, string formId)
        {
            string sql = string.Format(@"/*dialect*/SELECT t_SEC_User.FEMAIL,
                                                t_WF_PiBiMap.FKeyValue,
                                                t_WF_ProcInst.FNUMBER
                                                FROM  t_WF_PiBiMap 
                                                INNER JOIN t_WF_ProcInst 
                                                ON t_WF_ProcInst.FProcInstId = t_WF_PiBiMap.FProcInstId  and t_WF_ProcInst.FSTATUS=2
                                                INNER  JOIN t_WF_ActInst 
                                                on t_WF_ActInst.FProcInstId = t_WF_ProcInst.FProcInstId
                                                INNER JOIN t_WF_Assign on (t_WF_Assign.FActInstId = t_WF_ActInst.FActInstId)
                                                INNER JOIN t_WF_Receiver on (t_WF_Receiver.FAssignId = t_WF_Assign.FAssignId)
                                                INNER JOIN t_SEC_User ON (t_SEC_User.FUserId = t_WF_Receiver.FReceiverId)
                                                INNER JOIN t_WF_ApprovalAssign on (t_WF_Assign.FAssignId = t_WF_ApprovalAssign.FAssignId)
                                                LEFT JOIN t_WF_ApprovalItem on (t_WF_ApprovalItem.FApprovalAssignId = t_WF_ApprovalAssign.FApprovalAssignId AND t_WF_ApprovalItem.FReceiverId = t_WF_Receiver.FReceiverId)
                                                WHERE  t_WF_PiBiMap.FObjectTypeId = '{0}' 
                                                group by t_SEC_User.FEMAIL,
                                                t_WF_PiBiMap.FKeyValue,
                                                t_WF_ProcInst.FNUMBER", formId);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            return dyns;
        }
        /// <summary>
        /// 获取经销商通知人列表邮箱
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="distributorId"></param>
        /// <param name="sybId"></param>
        /// <param name="status"></param>
        /// <param name="billType"></param>
        /// <returns></returns>
        public static List<string> GetDistributorNoticeEmails(Context ctx, string distributorId, string sybId, string status, string billType)
        {
            List<string> emailAddress = new List<string>();
            string sql = string.Format(@"select noticeEntry.F_PATC_RECIPIENTMAILBOX  FEMAIL
                                          from PATC_t_DONotifier notice
                                          inner join PATC_t_DONotifierEntry noticeEntry
                                          on noticeEntry.FID=notice.FID and noticeEntry.F_PATC_BBCORDERSTATUAS='{0}'
                                          inner join T_BD_CUSTOMER cust
										  on cust.FCUSTID =notice.F_PATC_CUSTOMERCODE
                                          inner join T_ESS_CHANNEL channel
										  on channel.FNUMBER=cust.FNUMBER
                                          where channel.FCHANNELID={1}
                                            and notice.F_PATC_SYB={2} 
                                            and notice.F_PATC_TYPES='{3}'", status, distributorId, sybId, billType);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dyns.Count > 0)
            {
                foreach (var dyn in dyns)
                {
                    string address = dyn["FEMAIL"].ToString().Trim();
                    if (!string.IsNullOrEmpty(address))
                    {
                        emailAddress.Add(address);
                    }
                }
            }
            return emailAddress;
        }

        /// <summary>
        /// 获取经销商通知人列表邮箱
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="distributorId"></param>
        /// <param name="sybId"></param>
        /// <param name="status"></param>
        /// <param name="billType"></param>
        /// <returns></returns>
        public static List<string> GetDistributorNoticeEmailsForBBCReturn(Context ctx, string distributorId, string sybId, string status, string billType)
        {
            List<string> emailAddress = new List<string>();
            string sql = string.Format(@"select noticeEntry.F_PATC_RECIPIENTMAILBOX  FEMAIL
                                          from PATC_t_DONotifier notice
                                          inner join PATC_t_DONotifierEntry noticeEntry
                                          on noticeEntry.FID=notice.FID and noticeEntry.F_PATC_BBCORDERSTATUAS='{0}'
                                          where notice.F_PATC_CUSTOMERCODE={1}
                                            and notice.F_PATC_SYB={2} 
                                            and notice.F_PATC_TYPES='{3}'", status, distributorId, sybId, billType);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dyns.Count > 0)
            {
                foreach (var dyn in dyns)
                {
                    string address = dyn["FEMAIL"].ToString().Trim();
                    if (!string.IsNullOrEmpty(address))
                    {
                        emailAddress.Add(address);
                    }
                }
            }
            return emailAddress;
        }

        /// <summary>
        /// 获取bbc前台用户邮箱
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="createId"></param>
        /// <returns></returns>
        public static string GetBBCUserEmail(Context ctx, string createId)
        {
            string sql = string.Format(@"select FEMAIL
                                         from T_ESS_USERDETAILS
                                         where FUSERID={0}", createId);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dyns.Count > 0)
            {
                string address = dyns[0]["FEMAIL"].ToString().Trim();
                return address;
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取星空用户邮箱
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="createId"></param>
        /// <returns></returns>
        public static string GetUserEmail(Context ctx, string createId)
        {
            string sql = string.Format(@"select FEMAIL from T_SEC_USER where FUSERID={0}", createId);
            var dyns = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dyns.Count > 0)
            {
                string address = dyns[0]["FEMAIL"].ToString().Trim();
                return address;
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取单据创建人id
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetOrderUserId(Context ctx, string fid, string tableName, string filedId = "FUSERID")
        {
            string sql = string.Format(@"select {2} FUSERID  from {1} where FID={0}", fid, tableName, filedId);
            var dnys = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (dnys.Count > 0)
            {
                return dnys[0]["FUSERID"].ToString();
            }
            return "";
        }
        /// <summary>
        /// 修改邮件发送状态
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fid"></param>
        public static void UpdateEmailStatus(Context ctx, string fid ,int status=1)
        {
            var filter = OQLFilter.CreateHeadEntityFilter(string.Format("FID ={0}", fid));
            var dyns = BusinessDataServiceHelper.Load(ctx, EmailSendListFormId, null, filter);
            IMetaDataService metaService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata matMetaData = metaService.Load(ctx, EmailSendListFormId) as FormMetadata;
            BusinessInfo materialInfo = matMetaData.BusinessInfo;
            if (dyns.Count() > 0)
            {
                dyns[0]["F_PATC_Status"] = status;
                dyns[0]["F_PATC_SendTime"] = DateTime.Now;
               
            }
            IOperationResult saveResult = BusinessDataServiceHelper.Save(ctx, materialInfo, dyns.ToArray(), null, "Save");
        }
        /// <summary>
        /// 获取罗氏退货通知人邮箱
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="staffPosition"></param>
        /// <returns></returns>
        public static List<string> GetRocheRORecEmails(Context ctx, List<string> staffPosition){
            string formId = "PATC_RocheReturnOrderRecipient";
            string whereStr = "";
            foreach(var staff in staffPosition){
                whereStr = whereStr + ",'" + staff + "'";
            }
            whereStr = string.Format("FDOCUMENTSTATUS='C' and F_PATC_StaffPosition.FName in ({0})", whereStr.Substring(1));
            var filter = OQLFilter.CreateHeadEntityFilter(whereStr);
            var dyns = BusinessDataServiceHelper.Load(ctx, formId, null, filter);
            List<string> list = new List<string>();
           foreach(var dyn in dyns){
               list.Add(dyn["F_PATC_RecipientMailbox"].ToString());
           }
           return list;
        }
    }
}
