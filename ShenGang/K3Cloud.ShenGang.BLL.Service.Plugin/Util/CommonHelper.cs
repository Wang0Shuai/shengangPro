
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceClient.BusinessData;
using Kingdee.BOS.ServiceFacade.KDServiceClient.DataMigration;
using Kingdee.BOS.ServiceFacade.KDServiceClient.DB;
using Kingdee.BOS.ServiceFacade.KDServiceClient.Metadata;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.BOS.Core;
//using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    public class CommonHelper
    {
        /// <summary>
        /// 登录地址  eg:"http://116.236.226.214:8081/k3cloud/"
        /// </summary>
        public static String BASEURL = null;//登录地址

        /// <summary>
        /// 数据中心ID
        /// </summary>
        public static String DBID =   null;

        /// <summary>
        /// 用户名
        /// </summary>
        public static String USERNAME =  null;

        /// <summary>
        /// 密码
        /// </summary>
        public static String PASSWROD =  null;

        /// <summary>
        /// 密码
        /// </summary>
        public static bool DEBUG = false;
        public static K3CloudApiClient client = null;

        /// <summary>
        /// 模拟WebApi登录
        /// </summary>
        public static void webapiLogin() {
            client = new K3CloudApiClient(BASEURL);
            client.ValidateLogin(DBID, USERNAME,PASSWROD, 2052);
        }

        /// <summary>
        /// 通过代理登录
        /// </summary>
        /// <returns></returns>
        public static Context proxyLogin()
        {
           
            UserServiceProxy proxy = new UserServiceProxy();// 引用Kingdee.BOS.ServiceFacade.KDServiceClient.dll
            proxy.HostURL = BASEURL;//k/3cloud地址
            Context ctx = proxy.ValidateUser("", DBID, USERNAME, PASSWROD, 2052).Context;//guid为
            return ctx;
        }

        /// <summary>
        /// 通过ID获取对象
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="id">id</param>
        /// <param name="formid">formid</param>
        /// <returns></returns>
        public static DynamicObject getDynamicObjectByID(Context ctx, int id, String formid)
        {
            DynamicObject[] objs = null;
            if (CommonHelper.DEBUG)
            {

                MetadataServiceProxy metaServiceProxy = new MetadataServiceProxy();
                metaServiceProxy.HostURL = BASEURL;
              
                BusinessDataServiceProxy businessProxy = new BusinessDataServiceProxy();
                businessProxy.HostURL = BASEURL;
                FormMetadata metadata = metaServiceProxy.GetFormMetadata(formid);
                objs = businessProxy.Load(new object[] { id }, metadata.BusinessInfo.GetDynamicObjectType());
            }
            else
            {
                FormMetadata metadata =
                MetaDataServiceHelper.Load(ctx, formid) as FormMetadata;
                objs = BusinessDataServiceHelper.Load(ctx, new object[] { id },
                              metadata.BusinessInfo.GetDynamicObjectType());

            }
            DynamicObject model = objs[0];
            return model;
        }

        /// <summary>
        /// 执行更新SQL
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        public static void executeSql(Context ctx,string sql, List<SqlParam> paramList) {
            if (CommonHelper.DEBUG)
            {
                SQLScriptServiceProxy scriptProxy = new SQLScriptServiceProxy();
                scriptProxy.HostURL = BASEURL;
                scriptProxy.Execute(sql, paramList);
            }
            else
            {
                DBUtils.Execute(ctx, sql, paramList);
            }
        
        }

        /// <summary>
        /// WEBAPI执行 
        /// </summary>
        /// <param name="ctx">上下文</param>
        /// <param name="formid">FORMID</param>
        /// <param name="data">数据</param>
        /// <param name="method">方法名:save,submit,audit,query</param>
        /// <returns></returns>
        public static String webapiInvoke(Context ctx, String formid, String data, String method)
        {
             String rs=null;
            if (CommonHelper.DEBUG)
            {
                webapiLogin();
                if("save".Equals(method)){
                    rs= client.Save(formid, data);
                }
                else if ("submit".Equals(method))
                {
                    rs = client.Submit(formid, data);
                }
                else if ("audit".Equals(method))
                {
                    rs = client.Audit(formid, data);
                }
                else if ("query".Equals(method))
                {
                    rs = JsonConvert.SerializeObject( client.ExecuteBillQuery(data));
                } 
            }
            else {

                object rsobj = null;
                if ("save".Equals(method))
                {
                    rsobj = WebApiServiceCall.Save(ctx, formid, data);
                    rs = JsonConvert.SerializeObject(rsobj);
                }
                else if ("submit".Equals(method))
                {
                    rsobj = WebApiServiceCall.Submit(ctx, formid, data);
                    rs = JsonConvert.SerializeObject(rsobj);
                }
                else if ("audit".Equals(method))
                {
                    rs = WebApiServiceCall.Audit(ctx, formid, data).ToString();
                }
                else if ("query".Equals(method))
                {
                    rs = JsonConvert.SerializeObject(WebApiServiceCall.ExecuteBillQuery(ctx,data));
                }
            }
            return rs;
          
        }


        public static DynamicObject[] getDynamicObjectsByFilter(Context ctx, string formid, List<SelectorItemInfo> selector, OQLFilter ofilter)
       {
           DynamicObject[] objs = null;
           if (CommonHelper.DEBUG)
           {
               BusinessDataServiceProxy businessProxy = new BusinessDataServiceProxy();
               businessProxy.HostURL = CommonHelper.BASEURL;
               objs = businessProxy.Load(formid, selector, ofilter);
           }
           else
           {
               objs = BusinessDataServiceHelper.Load(ctx, formid, selector, ofilter);
           }
          // DynamicObject model = objs[0];
           return objs;
       }

        public static DynamicObject getDynamicObjectByFilter(Context ctx, string formid, List<SelectorItemInfo> selector, OQLFilter ofilter)
        {

            DynamicObject[] objs = getDynamicObjectsByFilter(ctx, formid, selector, ofilter);
            DynamicObject model = objs[0];
            return model;
        }
        public static IBillView CreateK3BillView(Context ctx, string formId)
        {
            // 读取应收单的元数据
            FormMetadata meta = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
            Form form = meta.BusinessInfo.GetForm();
            // 创建用于引入数据的单据view
            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            var billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            // 开始初始化billView：
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = CreateOpenParameter(meta, ctx);
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            var provider = form.GetFormServiceProvider();
            billView.Initialize(openParam, provider);
            return billView as IBillView;
        }
        public static BillOpenParameter CreateOpenParameter(FormMetadata meta, Context ctx)
        {
            Form form = meta.BusinessInfo.GetForm();
            // 指定FormId, LayoutId
            BillOpenParameter openParam = new BillOpenParameter(form.Id, meta.GetLayoutInfo().Id);
            // 数据库上下文
            openParam.Context = ctx;
            // 本单据模型使用的MVC框架
            openParam.ServiceName = form.FormServiceName;
            // 随机产生一个不重复的PageId，作为视图的标识
            openParam.PageId = Guid.NewGuid().ToString();
            // 元数据
            openParam.FormMetaData = meta;
            // 界面状态：新增 (修改、查看)
            openParam.Status = OperationStatus.ADDNEW;
            // 单据主键：本案例演示新建物料，不需要设置主键
            openParam.PkValue = null;
            // 界面创建目的：普通无特殊目的 （为工作流、为下推、为复制等）
            openParam.CreateFrom = CreateFrom.Default;
            // 基础资料分组维度：基础资料允许添加多个分组字段，每个分组字段会有一个分组维度
            // 具体分组维度Id，请参阅 form.FormGroups 属性
            openParam.GroupId = "";
            // 基础资料分组：如果需要为新建的基础资料指定所在分组，请设置此属性
            openParam.ParentId = 0;
            // 单据类型
            openParam.DefaultBillTypeId = "";
            // 业务流程
            openParam.DefaultBusinessFlowId = "";
            // 主业务组织改变时，不用弹出提示界面
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            var plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            foreach (var plug in plugs)
            {// 触发插件PreOpenForm事件，供插件确认是否允许打开界面
                plug.PreOpenForm(args);
            }
            if (args.Cancel == true)
            {// 插件不允许打开界面
                // 本案例不理会插件的诉求，继续....
            }
            // 返回
            return openParam;
        }
        #region 构造K3表单
        /// <summary>
        /// 构造K3表单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public IBillView CreateAddNewView(Context ctx, string formId, object id = null)
        {
            FormMetadata formMetadata = (FormMetadata)ServiceHelper.GetService<IMetaDataService>().Load(ctx, formId);
            Form form = formMetadata.BusinessInfo.GetForm();
            BillOpenParameter openParameter = new BillOpenParameter(form.Id, formMetadata.GetLayoutInfo().Id)
            {
                Context = ctx,
                PageId = Guid.NewGuid().ToString(),
                Status = id.IsNullOrEmptyOrWhiteSpace() ? OperationStatus.ADDNEW : OperationStatus.VIEW,
                CreateFrom = CreateFrom.Default,
                DefaultBillTypeId = null,  // 设置单据类型
                PkValue = id,
                FormMetaData = formMetadata,
                ServiceName = form.FormServiceName,
            };
            openParameter.SetCustomParameter(Kingdee.BOS.Core.FormConst.PlugIns, form.CreateFormPlugIns());

            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            IDynamicFormViewService dynamicFormViewService = (IDynamicFormViewService)Activator.CreateInstance(type);
            dynamicFormViewService.Initialize(openParameter, form.GetFormServiceProvider(true));
            IBillView billView = (IBillView)dynamicFormViewService;
            billView.CreateNewModelData();
            // 为单据类型字段赋值FFormId
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }
            //IImportView importView = dynamicFormViewService as IImportView;
            //importView.AddViewSession();//添加session
            // todo...
            //importView.RemoveViewSession();//移除session

            return billView;
        }

        /// <summary>
        /// 执行单据转换
        /// </summary>
        /// <param name="ctx">服务上下文</param>
        /// <param name="convertRuleKey">单据转换标识</param>
        /// <param name="entryEntityKey">源单明细ORM实体名</param>
        /// <param name="billObjs">单据集合</param>
        /// <param name="targetBillIds">目标单据ID集合</param>
        public void RunBillConvert(Context ctx, IOperationResult operationResult, string convertRuleKey, string entryEntityKey, DynamicObject[] billObjs, out List<string> targetBillIds)
        {
            targetBillIds = new List<string>();
            if (billObjs == null || billObjs.Count() == 0)
            {
                return;
            }

            ConvertRuleElement convertRuleElement = ConvertServiceHelper.GetConvertRule(ctx, convertRuleKey).Rule;// 单据转换
            List<ListSelectedRow> selectRows = new List<ListSelectedRow>();// 源单选中行
            foreach (DynamicObject billObj in billObjs)//e.DataEntitys
            {
                string fid = billObj["Id"].ToString();
                DynamicObjectCollection entryColl = billObj.Contains(entryEntityKey) ? billObj[entryEntityKey] as DynamicObjectCollection : null;
                if (entryColl == null || entryColl.Count == 0)
                {
                    ListSelectedRow listSelectedRow = new ListSelectedRow(fid, "0", 0, convertRuleElement.SourceFormId);
                    selectRows.Add(listSelectedRow);
                    continue;
                }
                foreach (DynamicObject rowObj in entryColl)
                {
                    string entryId = rowObj["Id"].ToString();
                    ListSelectedRow listSelectedRow = new ListSelectedRow(fid, entryId, 0, convertRuleElement.SourceFormId);
                    selectRows.Add(listSelectedRow);
                    //break;// 整单操作，只取第1行明细id即可。
                }
            }
            //  执行下推操作
            ConvertOperationResult convertResult = null;
            try
            {
                ConvertService convertService = new ConvertService();
                PushArgs pushArgs = new PushArgs(convertRuleElement, selectRows.ToArray());
                //pushArgs.CustomParams = new Dictionary<string, object>(); // 需要传递给单据转换插件的自定义参数；Key => Value
                convertResult = convertService.Push(ctx, pushArgs, CreateOperition(1)); // 执行下推操作，并接收下推操作结果
            }
            catch (Exception ex)
            {
                OperateResult result = new OperateResult()
                {
                    SuccessStatus = false,
                    Message = string.Format("下推时发生异常，请稍后重试！{0}", ex.Message),
                    HadWriteLog = false,
                    MessageType = MessageType.FatalError
                };
                operationResult.IsShowMessage = true;// 显示操作结果
                operationResult.IsSuccess = false;// 是否操作成功
                operationResult.OperateResult.Add(result);// 操作结果信息集
            }

            FormMetadata formMetadata = (FormMetadata)ServiceHelper.GetService<IMetaDataService>().Load(ctx, convertRuleElement.TargetFormId);
            this.SaveAndAutoSubmitAndAudit(ctx, operationResult, formMetadata.BusinessInfo, convertResult, out targetBillIds);
        }

        /// <summary>
        /// 保存并自动提交与审核
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="operationResult">当前操作的全局操作结果</param>
        /// <param name="businessInfo">目标单据BusinessInfo</param>
        /// <param name="convertResult">转换结果</param>
        /// <param name="successBillIds">执行成功的单据ID</param>
        /// <returns></returns>
        public bool SaveAndAutoSubmitAndAudit(Context ctx, IOperationResult operationResult, BusinessInfo businessInfo, ConvertOperationResult convertResult, out List<string> successBillIds)
        {
            successBillIds = new List<string>();
            if (convertResult == null)
            {
                Logger.Info("SaveAndAutoSubmitAndAudit", string.Format("转换结果为null，生成{0}失败！", businessInfo != null ? businessInfo.GetForm().Name : ""));
                return false;
            }

            try
            {
                // 如果下推失败，则在检查返回结果时直接抛出异常，不继续执行
                if (this.CheckOpResult(operationResult, convertResult))
                {
                    DynamicObject[] dataObjects = (from o in convertResult.TargetDataEntities select o.DataEntity).ToArray();
                    SaveService saveService = new SaveService();// 保存服务接口
                    IOperationResult saveResult = saveService.Save(ctx, businessInfo, dataObjects, CreateOperition(1), "save");
                    if (this.CheckOpResult(operationResult, saveResult))
                    {
                        // 将保存结果写入全局的操作结果
                        if (saveResult != null && saveResult.OperateResult != null && saveResult.OperateResult.Count > 0)
                        {
                            foreach (OperateResult item in saveResult.OperateResult)
                            {
                                operationResult.OperateResult.Add(item);
                            }
                        }
                        
                        SubmitService submitService = new SubmitService();//提交服务接口
                        string[] billIds = (from o in saveResult.SuccessDataEnity select o["Id"].ToString()).ToArray();
                        successBillIds = billIds.ToList();//保存成功的单据ID集合
                        IOperationResult submitResult = submitService.Submit(ctx, businessInfo, billIds, "submit", CreateOperition(1));
                        if (this.CheckOpResult(operationResult, submitResult))
                        {
                            AuditService auditService = new AuditService();//审核服务接口
                            string[] sumbitIds = (from o in submitResult.SuccessDataEnity select o["Id"].ToString()).ToArray();
                            IOperationResult auditResult = auditService.Audit(ctx, businessInfo, sumbitIds, CreateOperition(1));
                            if (this.CheckOpResult(operationResult, auditResult))
                            {
                                // 执行成功：审核完成
                                return true;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new KDBusinessException("SaveAndAutoSubmitAndAudit", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 判断操作结果是否成功，如果不成功，则直接抛错中断进程
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="operationResult">叠加操作结果</param>
        /// <param name="opResult">当前操作的操作结果</param>
        /// <returns></returns>
        public bool CheckOpResult(IOperationResult operationResult, IOperationResult opResult)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示

                    // 传出交互提示完整信息对象
                    operationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    operationResult.Sponsor = opResult.Sponsor;

                    // 抛出错误，终止本次操作
                    isSuccess = false;
                    string errMsg = "本次操作需要用户确认是否继续，暂时中断!";// + JsonUtil.Serialize((from m in opResult.InteractionContext.K3DisplayerModel.Messages select m.DataEntity).Distinct().ToArray(), false);
                    throw new KDBusinessException("CheckOpResult", errMsg);
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {
                        // 未知原因导致提交失败
                        isSuccess = false;
                        string errMsg = "未知原因导致自动提交、审核失败！" + JsonUtil.Serialize(opResult, false);
                        throw new KDBusinessException("CheckOpResult", errMsg);
                    }
                    else
                    {
                        isSuccess = false;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("自动提交、审核失败，失败原因：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("CheckOpResult", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }

        /// <summary>
        /// 创建操作选项
        /// </summary>
        /// <param name="level">0: 无任何提示、交互；1：有提示、无交互；其他：系统默认值（有提示、有交互）</param>
        /// <returns></returns>
        public static OperateOption CreateOperition(int level)
        {
            OperateOption option = OperateOption.Create();
            option.SetVariableValue(BOSConst.CST_AutoSubmitVlidatePermission, false);
            if (level == 0)
            {
                option.SetValidateFlag(false);
                option.SetIsThrowValidationInfo(false);
                option.SetIgnoreWarning(true);
                option.SetIgnoreInteractionFlag(true);
            }
            else if (level == 1)
            {
                //option.SetValidateFlag(false);
                //option.SetIsThrowValidationInfo(false);
                option.SetIgnoreWarning(true);
                //option.SetIgnoreInteractionFlag(true);
            }
            else
            {
                // 使用系统默认值
            }
            return option;
        }
        #endregion

        #region 仓库解锁库存
        public static string uid = "Administrator";
        public static string pwd = "hl28&2021";
        public static string surl = RequestUtils.GetLoginUrl();
        //public static K3CloudApiClient client = new K3CloudApiClient(RequestUtils.GetLoginUrl());

        public static long GetDynamicValue(DynamicObject obj)
        {
            if (obj != null)
            {
                if (obj.DynamicObjectType.Properties.ContainsKey(FormConst.MASTER_ID))
                {
                    return Convert.ToInt64(obj[FormConst.MASTER_ID]);
                }
                if (obj.DynamicObjectType.Properties.ContainsKey("Id"))
                {
                    return Convert.ToInt64(obj["Id"]);
                }
            }
            return 0L;
        }

        //获取缺货物料
        public static bool ShowRelationDynamicForm(Context context, string sql)
        {
            DynamicObjectCollection doc = DBUtils.ExecuteDynamicObject(context, sql);
            if (doc != null && doc.Count > 0)
            {
                return true;
            }
            return false;
        }

         #endregion


    }


   
}
