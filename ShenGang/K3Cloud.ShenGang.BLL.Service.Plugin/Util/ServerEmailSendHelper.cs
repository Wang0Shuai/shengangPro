using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.BusinessEntity;
using System.IO;
//using Kingdee.BOS.Open;

using System.Web;
using Kingdee.BOS.Business.PlugIn.MailSetting;
using System.Data;
using Kingdee.BOS.Core.Log;
using System.Net;
using Kingdee.BOS.Core;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.App.Data;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    class ServerEmailSendHelper
    {


        public static bool SendMail(Context ctx, string formId, string FACCOUNT, string[] ids, Dictionary<string, string> attDic, DataTable dataRec1, List<string> address, List<string> addressCC, String title, String message)
        {
            bool result = false;
            if (ids.IsEmpty())
            {
                saveLog(ctx, formId + " ids is Empty");
                return result;
            }
            //
            Dictionary<string, System.IO.Stream> dictStreamSon = new Dictionary<string, System.IO.Stream>();
            string attName1 = string.Empty;
            string FFILEID = string.Empty;
            string attLocationPath = string.Empty;
            string localFullPath = string.Empty;
            string serviceAddress = string.Empty;
            string WebSiteUrl = string.Empty;
            if (dataRec1.Rows.Count > 0)
            {
                serviceAddress = "";// DataUtils.getKDMapData(ctx, "select F_PATC_APPIPANDPORT from PATC_t_OMIPComRefSet", "F_PATC_APPIPANDPORT");
                if (string.IsNullOrEmpty(serviceAddress))
                {
                    throw new Exception("邮件模板附件服务地址未维护");
                }
                WebSiteUrl = serviceAddress + "/K3Cloud/";
                //FileUpLoadServices/Download.aspx

            }
            for (int i = 0; i < dataRec1.Rows.Count; i++)
            {
                FFILEID = Convert.ToString(dataRec1.Rows[i]["FFILEID"]);
                attName1 = Convert.ToString(dataRec1.Rows[i]["FATTACHMENTNAME"]);
                attLocationPath = Convert.ToString(dataRec1.Rows[i]["FLOCATIONPATH"]);
                if (!string.IsNullOrEmpty(attName1) && !string.IsNullOrEmpty(FFILEID) && !attDic.ContainsKey(attName1))
                {

                    //localFullPath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, attName1);
                    //方式一 文件从文件云端下载到本地
                    //Download(ctx, localFullPath, FFILEID, WebSiteUrl);
                    //attDic.Add(attName, localFullPath);
                    //方式二 直接进行流存储
                    dictStreamSon.Add(attName1, DownloadStream(ctx, localFullPath, FFILEID, WebSiteUrl));
                }
            }


            //
            string pkValue = ids.FirstOrDefault();
            var view = CreateView(ctx, formId, pkValue);
            Kingdee.BOS.Core.Import.IImportView importView = view as Kingdee.BOS.Core.Import.IImportView;
            if (importView == null)
            {
                saveLog(ctx, formId + " importView is null");
                return result;
            }
            importView.AddViewSession();//重要，不能删除           
            try
            {
                ISendMailService service = ServiceFactory.GetService<ISendMailService>(ctx);
                //String filepath = service.GetPdfExportFileName(view, ctx.UserId, ids, noteTempId);


                string sqlScript = string.Format(@"select FUSERID  from t_sec_user where fname ='Administrator'");
                string userId = "";// Convert.ToString(DataUtils.getKDMapData(ctx, sqlScript, "FUSERID"));
                if (string.IsNullOrEmpty(userId))
                {
                    throw new KDBusinessException("SendMail", "邮件发送失败:未获取到管理员ID！");
                }
                List<long> user = new List<long>();
                user.Add(Convert.ToInt64(userId));
                List<DynamicObject> listc = SendMailServiceHelper.GetUserSendMailVirtualAccount(ctx, user);
                listc = (from p in listc
                         where Convert.ToString(p["FAccount"]) == FACCOUNT
                         select p).Distinct().ToList();
                if (listc.Count > 0)
                {

                    DynamicObject virtualAccount = listc[0];
                    string displayName = Convert.ToString(virtualAccount["FAccount"]);
                    string fromMail = Convert.ToString(virtualAccount["FFromEmail"]);
                    string smtpHost = Convert.ToString(virtualAccount["FSMTPHost"]);
                    int port = Convert.ToInt32(virtualAccount["FSMTPPort"]);
                    string mailAccount = Convert.ToString(virtualAccount["FUSERNAME"]);
                    string mailPassword = Convert.ToString(virtualAccount["FPASSWORD"]);
                    bool enableSSL = "0".Equals(virtualAccount["FSSL"].ToString()) ? false : true;
                    //String name = HttpUtility.UrlDecode(filepath.Substring(13), Encoding.UTF8);
                    //PathUtils.GetPhysicalPath("tempfilePath", name);
                    Dictionary<string, System.IO.Stream> dictStream = new Dictionary<string, System.IO.Stream>();
                    foreach (string attName in dictStreamSon.Keys)
                    {
                        dictStream.Add(attName, dictStreamSon[attName]);
                    }
                    foreach (string attName in attDic.Keys)
                    {
                        if (attName != "" && attDic[attName] != "")
                        {
                            Stream fileStream = OpenFileStream(attDic[attName].ToString());
                            dictStream.Add(attName, fileStream);
                        }
                    }
                    string pass = DoProclaimText(mailPassword);
                    //"UTXGEJFQTCJNDAHQ"
                    string vartime1 = string.Empty;
                    string vartime2 = string.Empty;
                    DateTime dt1 = DateTime.Now;
                    vartime1 = dt1.ToString("yyyy-MM-dd hh:mm:ss ffff");

                    bool emailEx = false;
                    bool isEmailEx = false;
                    string emailExMsg =string.Empty;
                    try
                    {
                        MailTools.SendmailWithCC(fromMail, displayName, address, addressCC, title, message, smtpHost, port, enableSSL, dictStream, mailAccount, pass);
                    }
                    catch (Exception emailE1)
                    {
                        emailEx = CheackEmailEx(ctx, emailE1.Message);
                        isEmailEx = true;
                        emailExMsg = emailE1.Message;
                    }
                    if (emailEx)
                    {
                        try
                        {

                            saveLogForTime(ctx, "超时-重发1");
                            MailTools.SendmailWithCC(fromMail, displayName, address, addressCC, title, message, smtpHost, port, enableSSL, dictStream, mailAccount, pass);
                            emailEx = false;
                            isEmailEx = false;
                        }
                        catch (Exception emailE2)
                        {
                            emailEx = CheackEmailEx(ctx, emailE2.Message);
                            isEmailEx = true;
                            emailExMsg = emailE2.Message;
                        }

                    }
                    else if (isEmailEx)
                    {
                        throw new Exception(emailExMsg);
                    }

                    if (emailEx)
                    {
                        saveLogForTime(ctx, "超时-重发2");
                        MailTools.SendmailWithCC(fromMail, displayName, address, addressCC, title, message, smtpHost, port, enableSSL, dictStream, mailAccount, pass);
                    }
                    else if (isEmailEx)
                    {
                        throw new Exception(emailExMsg);
                    }

                    DateTime dt2 = DateTime.Now;
                    vartime2 = dt2.ToString("yyyy-MM-dd hh:mm:ss ffff");
                    string longtime = Convert.ToInt64((dt2 - dt1).TotalMilliseconds).ToString();
                    saveLogForTime(ctx, "ST:" + vartime1 +
                               " ET:" + vartime2 + " TD(ms):" + longtime);

                    return true;
                }
                else
                {
                    throw new KDBusinessException("SendMail", "邮件发送失败:虚拟邮箱未维护，请联系管理员处理！");
                }

            }
            catch (Exception ex)
            {
                Kingdee.BOS.Log.Logger.Error("SendMail", "邮件发送失败", ex);
                throw new KDBusinessException("SendMail", "邮件发送失败:" + ex.Message.ToString());
            }
            importView.RemoveViewSession();
        }
        private static bool CheackEmailEx(Context ctx, string inData)
        {
            if (inData.Contains("发送邮件失败") || inData.Contains("超时") || inData.Contains("timed out"))
                return true;
            return false;
        }
        private static void saveLogForTime(Context ctx, string inData)
        {
            var logs = new List<LogObject>();
            var log = new LogObject();
            //log.pkValue = "";
            log.Description = inData;
            log.OperateName = "OMIP_给经销商发送邮件";
            log.ObjectTypeId = "PATC_EmailTemplateForOmip";
            log.SubSystemId = "";
            log.Environment = OperatingEnvironment.BizOperate;
            logs.Add(log);
            LogServiceHelper.BatchWriteLog(ctx, logs);
        }
        public static Stream DownloadStream(Context ctx, string filePath, string _fileId, string WebSiteUrl)
        {
            string nail = "1"; //是否下载缩略图，1为缩略图，其余为原图。
            //ApiClient client = new ApiClient(DataUtils.getKDMapData(ctx, "select F_PATC_APPIPANDPORT from PATC_t_OMIPComRefSet", "F_PATC_APPIPANDPORT").ToString()+"/k3cloud/");
            //string dbId = ctx.DBId; //AotuTest117
            //bool bLogin = client.Login(dbId, "administrator", "dopas_2019", 2052);
            //if (bLogin)
            //{
            //    client.//todo:登陆成功处理业务
            //}
            //else
            //{
            //    throw new KDBusinessException("LoginException","administrator登陆异常！");
            //}
            string userToken = AppLogin(ctx);
            //fileId文件编码可由 T_BAS_ATTACHMENT 附件明细表查得，此处直接拿上面上传文件的编码来做示例。
            string url = string.Format("{0}FileUpLoadServices/Download.aspx?fileId={1}&token={2}&nail={3}",
                WebSiteUrl, _fileId, userToken, nail);
            using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                //return webClient.OpenRead(url);
                byte[] array = webClient.DownloadData(url);
                return new MemoryStream(array);
            }
        }

        private static string AppLogin(Context ctx)
        {
            string excSql = @"select F_PATC_APPIPANDPORT,F_PATC_TEXT1,F_PATC_TEXT2 from PATC_t_OMIPComRefSet";
            string url = string.Empty;
            string userName = string.Empty;
            string passWord = string.Empty;
            using (DataTable refReader = DBUtils.ExecuteDataSet(ctx, excSql).Tables[0])
            {
                if (refReader.Rows.Count > 0)
                {
                    url = Convert.ToString(refReader.Rows[0]["F_PATC_APPIPANDPORT"]);
                    userName = Convert.ToString(refReader.Rows[0]["F_PATC_TEXT1"]);
                    passWord = Convert.ToString(refReader.Rows[0]["F_PATC_TEXT2"]);
                }
                else
                {
                    throw new KDBusinessException("LoginException", "登陆异常,无法获取附件文件！");
                }

            }
            HttpClient httpClient = new HttpClient();
            httpClient.Url = url + "/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc";
            List<object> Parameters = new List<object>();
            Parameters.Add(ctx.DBId);//帐套Id
            Parameters.Add(userName);//用户名F_PATC_TEXT1
            Parameters.Add(passWord);//密码
            Parameters.Add(2052);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            var iResult = JObject.Parse(httpClient.AsyncRequest())["LoginResultType"].Value<int>();
            string userToken = string.Empty;
            if (iResult == 1)
            {
                userToken = JObject.Parse(httpClient.AsyncRequest())["Context"]["UserToken"].Value<string>();
                if (string.IsNullOrEmpty(userToken))
                {
                    throw new KDBusinessException("LoginException", "登陆异常,无法获取附件文件！");
                }
            }
            else
            {
                throw new KDBusinessException("LoginException", "登陆异常,无法获取附件文件！");
            }
            return userToken;
        }



        private static void saveLog(Context ctx, string inData)
        {
            var logs = new List<LogObject>();
            var log = new LogObject();
            //log.pkValue = "";
            log.Description = "异常：" + inData;
            log.OperateName = "OMIP_发邮件";
            log.ObjectTypeId = "PATC_EmailTemplateForOmip";
            log.SubSystemId = "";
            log.Environment = OperatingEnvironment.BizOperate;
            logs.Add(log);
            LogServiceHelper.BatchWriteLog(ctx, logs);
        }
        public static string DoProclaimText(string CipherText)
        {
            string result;
            if (StringUtils.IsEmpty(CipherText))
            {
                result = CipherText;
            }
            else
            {
                byte[] bytes = Encoding.BigEndianUnicode.GetBytes(CipherText);
                int num = bytes.Length;
                byte[] array = new byte[num / 2];
                for (int i = 0; i < num; i += 4)
                {
                    byte b = bytes[i + 1];
                    byte b2 = bytes[i + 3];
                    int num2 = (int)(b & 15) << 4;
                    int num3 = (int)(b & 240);
                    int num4 = (int)(b2 & 15);
                    int num5 = (b2 & 240) >> 4;
                    array[i / 2] = Convert.ToByte(num2 | num5);
                    array[i / 2 + 1] = Convert.ToByte(num3 | num4);
                }
                result = Encoding.BigEndianUnicode.GetString(array, 0, array.Length);
            }
            return result;
        }


        private static IDynamicFormView CreateView(Context ctx, string formId, string pkValue)
        {
            IMetaDataService metaDataService = Kingdee.BOS.App.ServiceHelper.GetService<IMetaDataService>();
            FormMetadata metadata = (FormMetadata)metaDataService.Load(ctx, formId);
            var openParameter = CreateOpenParameter(ctx, metadata, pkValue);
            var provider = metadata.BusinessInfo.GetForm().GetFormServiceProvider();
            string importViewClass = "Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web";
            Type type = Type.GetType(importViewClass);
            IDynamicFormViewService billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            billView.Initialize(openParameter, provider);
            billView.LoadData(); return (IDynamicFormView)billView;
        }
        private static BillOpenParameter CreateOpenParameter(Context ctx, FormMetadata metadata, string pkValue)
        {
            var form = metadata.BusinessInfo.GetForm();
            BillOpenParameter openPara = new BillOpenParameter(form.Id, metadata.GetLayoutInfo().Id);
            openPara.Context = ctx;
            openPara.PageId = Guid.NewGuid().ToString();
            openPara.Status = OperationStatus.VIEW;
            openPara.CreateFrom = CreateFrom.Default;
            openPara.DefaultBillTypeId = string.Empty;
            openPara.PkValue = pkValue;
            openPara.FormMetaData = metadata;
            openPara.SetCustomParameter(Kingdee.BOS.Core.FormConst.PlugIns, form.CreateFormPlugIns());
            openPara.ServiceName = form.FormServiceName;
            return openPara;
        }

        public static Stream OpenFileStream(string filePath)
        {
            FileStream fileStream = File.Open(filePath, FileMode.Open);
            byte[] array = new byte[fileStream.Length];
            fileStream.Read(array, 0, array.Length);
            fileStream.Close();
            return new MemoryStream(array);
        }


    }
}
