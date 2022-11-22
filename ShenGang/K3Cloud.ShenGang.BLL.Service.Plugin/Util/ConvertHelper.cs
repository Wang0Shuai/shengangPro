using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using Kingdee.BOS.ServiceFacade.KDServiceClient.Metadata;
using Kingdee.BOS.JSON;
using Newtonsoft.Json;
using Kingdee.BOS.WebApi.FormService;
using Kingdee.BOS.App.Data;
using System.Data;
namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    /// <summary>
    /// 单据转换工具类
    /// </summary>
    public class ConvertHelper
    {
        /// <summary>
        ///  下推生成审核状态的单据 ，按照默认规则生成
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billID">原单据ID</param>
        /// <param name="sourceFromId">原单的FromId</param>
        /// <param name="targetFromId">目标单的FromId</param>
        public static void pushDownSaveSubmitAuditBillByDefault(Context context, String targetBillTypeId, String billID, String sourceFromId, String targetFromId)
        {
            //List<ConvertRuleElement> rules = ConvertServiceHelper.GetConvertRules(context, sourceFromId, targetFromId);
            //ConvertRuleElement rule = rules.FirstOrDefault(u => u.IsDefault==true);

            List<ConvertRuleMetaData> rulemetas = getRulesByFormId(context, sourceFromId, targetFromId);
            ConvertRuleElement rule = rulemetas.FirstOrDefault(u => u.Rule.IsDefault == true).Rule;

            if (rule == null)
            {
                ExceptionHelper.throwKDException("没有找到默认的转换规则");
            }
            pushDownSave(context, targetBillTypeId, billID, rule, "submit", "audit");
        }

        /// <summary>
        /// 按照规则ID生成单据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billID"></param>
        /// <param name="ruleId"></param>
        public static void pushDownSaveSubmitAuditBillByRuleId(Context context, String targetBillTypeId, String billID, String ruleId)
        {
            pushDownSaveByRuleId(context,targetBillTypeId, billID, ruleId, "submit", "audit");
        }

        /// <summary>
        /// 按照转换ID生成单据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billID"></param>
        /// <param name="ruleId"></param>
        /// <param name="actions"></param>
        public static void pushDownSaveByRuleId(Context context, String targetBillTypeId, String billID, String ruleId, params String[] actions)
        {
            ConvertRuleMetaData ruleMetadata = getConvertRuleById(context, ruleId);
            ConvertRuleElement rule = ruleMetadata.Rule;
            if (rule == null)
            {
                ExceptionHelper.throwKDException("没有找到转换规则:" + ruleId);
            }
            pushDownSave(context, targetBillTypeId, billID, rule, actions);
        }

//        public static void pushDownForUpdateZZFPSL_1(Context context, String targetBillTypeId, String billID, String ruleId, List<DistributionPlanEntity> dpList)
//        {
//            ConvertRuleMetaData ruleMetadata = getConvertRuleById(context, ruleId);
//            ConvertRuleElement rule = ruleMetadata.Rule;
//            if (rule == null)
//            {
//                ExceptionHelper.throwKDException("没有找到转换规则:" + ruleId);
//            }
//            ListSelectedRow row = new ListSelectedRow(billID, string.Empty, 0, rule.SourceFormId);
//            ListSelectedRow[] selectedRows = new ListSelectedRow[] { row };
//            // 调用下推服务，生成下游单据数据包
//            ConvertOperationResult pushResult = null;
//            PushArgs pushArgs = null;
//            if (targetBillTypeId.IsNullOrEmptyOrWhiteSpace())
//            {
//                pushArgs = new PushArgs(rule, selectedRows);
//            }
//            else
//            {
//                pushArgs = new PushArgs(rule, selectedRows)
//                {
//                    TargetBillTypeId = targetBillTypeId
//                };
//            }

//            // 执行下推操作，并获取下推结果
//            pushResult = push(context, pushArgs, OperateOption.Create());

//            // 获取生成的目标单据数据包
//            DynamicObject[] targetDataEntities = pushResult.TargetDataEntities.Select(u => u.DataEntity).ToArray();

//            // 读取目标单据元数据

//            FormMetadata targetBillMeta = MetaDataHelper.getFormMetadataById(context, rule.TargetFormId);

//            // 忽略全部需要交互性质的提示
//            OperateOption option = OperateOption.Create();
//            option.SetIgnoreWarning(true);// 保存

//            IOperationResult saveResult = BusinessDataHelper.save(context, targetBillMeta.BusinessInfo, targetDataEntities, option);
//            BusinessOperationHelper.operationResultProcess(saveResult);

//            object[] primaryKeys = saveResult.SuccessDataEnity.Select(u => u.GetPrimaryKeyValue()).ToArray();
//            //SHKD_LZJHTZDEntry  F_SHKD_TZHFPSL  F_SHKD_CPBM
//            DynamicObject[] resultData = saveResult.SuccessDataEnity.ToArray();
//            DynamicObjectCollection resultEntryData = resultData[0]["SHKD_LZJHTZDEntry"] as DynamicObjectCollection;
//            List<int> entryIds = new List<int>();

//            entryIds = (from p in resultEntryData
//                        join b in dpList on Convert.ToString(((DynamicObject)p["F_SHKD_CPBM"])["Number"]) equals b.matNum
//                        select Convert.ToInt32(p["Id"])).Distinct().ToList();


//            bool res = saveLZTZD_1(context, primaryKeys[0], entryIds, dpList);

//            if (!res)
//            {
//                throw new KDBusinessException("同步分配数量失败", "单据内码【" + billID + "】");
//            }

//            //提交操作
//            IOperationResult submitResult = BusinessDataHelper.submit(context, targetBillMeta.BusinessInfo, primaryKeys, "Submit", option);
//            BusinessOperationHelper.operationResultProcess(submitResult);

//            // 审核
//            IOperationResult auditResult = BusinessDataHelper.audit(context, targetBillMeta.BusinessInfo, primaryKeys);
//            BusinessOperationHelper.operationResultProcess(auditResult);

//            string qj = dpList[0].period;
//            List<SqlParam> sqlParam = new List<SqlParam>();
//            sqlParam.Add((new SqlParam("@peroid", KDDbType.String, qj)));
//            DBUtils.ExecuteStoreProcedure(context, "SHKD_RECALCINV", sqlParam);

            
//            List<string> opFEntryIDList = new List<string>();
//            for (int i = 0; i < dpList.Count; i++)
//            {
//                opFEntryIDList.Add(dpList[i].opFEntryID);
//            }
//            string opFEntryIDs = string.Join(",", opFEntryIDList);//数组转成字符串
//            string excSql = string.Format(@"update PATC_t_OrderPlanEntry set F_PATC_IsTB ='1' where FEntryID in ({0})", opFEntryIDs);
//            DBUtils.Execute(context, excSql);

//        }
//        private static bool saveLZTZD_1(Context context, object primaryKey, List<int> entryIDs, List<DistributionPlanEntity> dpList)
//        {
//            string excSql =string.Format( @"select b.FEntryID,c.FNUMBER  from SHKD_LZJHTZDBill a 
//left join SHKD_LZJHTZDEntry b on a.fid = b.FID
//left join  T_BD_MATERIAL c on b.F_SHKD_CPBM = c.FMATERIALID 
//where a.FID={0}", primaryKey.ToString());
//            List<TZDEntity> tzdList = new List<TZDEntity>();
//            TZDEntity tzd = new TZDEntity();
//            using (DataTable reader = DBUtils.ExecuteDataSet(context, excSql).Tables[0])
//            {
//                for (int j = 0; j < reader.Rows.Count; j++)
//                {
//                    tzd = new TZDEntity();
//                    tzd.tzdFEntryID = Convert.ToString(reader.Rows[j]["FEntryID"]);
//                    tzd.matNum = Convert.ToString(reader.Rows[j]["FNUMBER"]);

//                    tzdList.Add(tzd);
//                }
//            }

//            JSONObject obj = new JSONObject();
//            JSONObject model = new JSONObject();
//            JSONArray jsonHead = new JSONArray();
//            JSONObject data = null;
//            JSONObject datason = null;
//            //JSONObject entryData = null;

//            JSONArray entry = new JSONArray();
//            jsonHead.Add("F_SHKD_Entity");
//            jsonHead.Add("F_SHKD_TZHFPSL");
//            jsonHead.Add("F_PATC_IsFromODP");
//            jsonHead.Add("F_PATC_IsFriomODPEntry");
//            jsonHead.Add("F_PATC_IsSynODP");
//            jsonHead.Add("F_PATC_LastODPQty");

//            obj.Add("NeedUpDateFields", jsonHead);

//            obj.Add("IsAutoSubmitAndAudit", "false");//自动提交及审核
//            obj.Add("Model", model);

//            model.Add("FID", Convert.ToInt32(primaryKey));
//            model.Add("F_PATC_IsFromODP", 1);

//            model.Add("F_SHKD_Entity", entry);
//            for (int i = 0; i < entryIDs.Count; i++)
//            {
//                List<decimal> opQUANTITYONEs = (from p in dpList
//                 join b in tzdList on p.matNum equals b.matNum 
//                 where b.tzdFEntryID == entryIDs[i].ToString()
//                 select Convert.ToDecimal(p.opQUANTITYONE)).Distinct().ToList();

//                data = new JSONObject();
//                data.Add("FEntryID", entryIDs[i]);
//                data.Add("F_SHKD_TZHFPSL", opQUANTITYONEs[0]);//调整后分配数量
//                data.Add("F_PATC_IsFriomODPEntry", 1);//是否来源订购需求计划
//                data.Add("F_PATC_IsSynODP", 1);//是否同步订购需求计划
//                data.Add("F_PATC_LastODPQty", opQUANTITYONEs[0]);//最新同步订购需求计划数量

//                entry.Add(data);

//            }
//            string fromId = "SHKD_LZJHTZD";//表单id
//            string content = JsonConvert.SerializeObject(obj);//将封装好的json转换为String类型
//            object saveResult = WebApiServiceCall.Save(context, fromId, content);//调用保存方法  } 
//            Dictionary<String, object> responseStatus = ((Dictionary<String, object>)((Dictionary<String, object>)saveResult)["Result"])["ResponseStatus"] as Dictionary<String, object>;
//            if (responseStatus["IsSuccess"].ToString() == "True")
//            {
//                return true;
//            }
//            return false;
//        }

        public static void pushDownForUpdateZZFPSL(Context context, String targetBillTypeId, String billID, String ruleId, decimal quatityOne, string matNum, string dgxqjhBillNo)
        {
            ConvertRuleMetaData ruleMetadata = getConvertRuleById(context, ruleId);
            ConvertRuleElement rule = ruleMetadata.Rule;
            if (rule == null)
            {
                ExceptionHelper.throwKDException("没有找到转换规则:" + ruleId);
            }
            ListSelectedRow row = new ListSelectedRow(billID, string.Empty, 0, rule.SourceFormId);
            ListSelectedRow[] selectedRows = new ListSelectedRow[] { row };
            // 调用下推服务，生成下游单据数据包
            ConvertOperationResult pushResult = null;
            PushArgs pushArgs = null;
            if (targetBillTypeId.IsNullOrEmptyOrWhiteSpace())
            {
                pushArgs = new PushArgs(rule, selectedRows);
            }
            else
            {
                pushArgs = new PushArgs(rule, selectedRows)
                {
                    TargetBillTypeId = targetBillTypeId
                };
            }

            // 执行下推操作，并获取下推结果
            pushResult = push(context, pushArgs, OperateOption.Create());

            // 获取生成的目标单据数据包
            DynamicObject[] targetDataEntities = pushResult.TargetDataEntities.Select(u => u.DataEntity).ToArray();

            // 读取目标单据元数据

            FormMetadata targetBillMeta = MetaDataHelper.getFormMetadataById(context, rule.TargetFormId);

            // 忽略全部需要交互性质的提示
            OperateOption option = OperateOption.Create();
            option.SetIgnoreWarning(true);// 保存

            IOperationResult saveResult = BusinessDataHelper.save(context, targetBillMeta.BusinessInfo, targetDataEntities, option);
            BusinessOperationHelper.operationResultProcess(saveResult);

            object[] primaryKeys = saveResult.SuccessDataEnity.Select(u => u.GetPrimaryKeyValue()).ToArray();
            //SHKD_LZJHTZDEntry  F_SHKD_TZHFPSL  F_SHKD_CPBM
            DynamicObject[] resultData = saveResult.SuccessDataEnity.ToArray();
            DynamicObjectCollection resultEntryData = resultData[0]["SHKD_LZJHTZDEntry"] as DynamicObjectCollection;
            List<int> entryIds = new List<int>();
            entryIds = (from p in resultEntryData
                        where Convert.ToString(((DynamicObject)p["F_SHKD_CPBM"])["Number"]) == matNum
                        select Convert.ToInt32(p["Id"])).Distinct().ToList();
            bool res = saveLZTZD(context, primaryKeys[0], entryIds, quatityOne);
            if (!res)
            {
                throw new KDBusinessException("同步分配数量失败", "单据编号【" + dgxqjhBillNo + "】 物料编码【" + matNum + "】");
            }
            //提交操作
            IOperationResult submitResult = BusinessDataHelper.submit(context, targetBillMeta.BusinessInfo, primaryKeys, "Submit", option);
            BusinessOperationHelper.operationResultProcess(submitResult);

            // 审核
            IOperationResult auditResult = BusinessDataHelper.audit(context, targetBillMeta.BusinessInfo, primaryKeys);
            BusinessOperationHelper.operationResultProcess(auditResult);


        }

        private static bool saveLZTZD(Context context, object primaryKey, List<int> entryIDs, decimal qty)
        {
            JSONObject obj = new JSONObject();
            JSONObject model = new JSONObject();
            JSONArray jsonHead = new JSONArray();
            JSONObject data = null;
            JSONObject datason = null;
            //JSONObject entryData = null;

            JSONArray entry = new JSONArray();
            jsonHead.Add("F_SHKD_Entity");
            jsonHead.Add("F_SHKD_TZHFPSL");
            jsonHead.Add("F_PATC_IsFromODP");
            jsonHead.Add("F_PATC_IsFriomODPEntry");
            jsonHead.Add("F_PATC_IsSynODP");
            jsonHead.Add("F_PATC_LastODPQty");

            obj.Add("NeedUpDateFields", jsonHead);

            obj.Add("IsAutoSubmitAndAudit", "false");//自动提交及审核
            obj.Add("Model", model);

            model.Add("FID", Convert.ToInt32(primaryKey));
            model.Add("F_PATC_IsFromODP", 1);

            model.Add("F_SHKD_Entity", entry);
            for (int i = 0; i < entryIDs.Count; i++)
            {
                data = new JSONObject();
                data.Add("FEntryID", entryIDs[i]);
                data.Add("F_SHKD_TZHFPSL", qty);//调整后分配数量
                data.Add("F_PATC_IsFriomODPEntry", 1);//是否来源订购需求计划
                data.Add("F_PATC_IsSynODP", 1);//是否同步订购需求计划
                data.Add("F_PATC_LastODPQty", qty);//最新同步订购需求计划数量

                entry.Add(data);

            }
            string fromId = "SHKD_LZJHTZD";//表单id
            string content = JsonConvert.SerializeObject(obj);//将封装好的json转换为String类型
            object saveResult = WebApiServiceCall.Save(context, fromId, content);//调用保存方法  } 
            Dictionary<String, object> responseStatus = ((Dictionary<String, object>)((Dictionary<String, object>)saveResult)["Result"])["ResponseStatus"] as Dictionary<String, object>;
            if (responseStatus["IsSuccess"].ToString() == "True")
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 按照默认规则生成保存状态的单据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billID"></param>
        /// <param name="sourceFromId"></param>
        /// <param name="targetFromId"></param>
        public static void pushDownSaveByDefault(Context context, String targetBillTypeId, String billID, String sourceFromId, String targetFromId)
        {
            List<ConvertRuleMetaData> rulemetas = getRulesByFormId(context, sourceFromId, targetFromId);
            ConvertRuleElement rule = rulemetas.FirstOrDefault(u => u.Rule.IsDefault == true).Rule;

            if (rule == null)
            {
                ExceptionHelper.throwKDException("没有找到默认的转换规则");
            }
            pushDownSave(context, targetBillTypeId, billID, rule);
        }

        public static object[] PurchaseInstockFids;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="billID">原单据ID</param>
        /// <param name="rule">规则</param>
        /// <param name="actions">可变参数 需要传递 字符串类型的 submit audit</param>
        public static void pushDownSave(Context context, String targetBillTypeId, String billID, ConvertRuleElement rule, params String[] actions)
        {
            ListSelectedRow row = new ListSelectedRow(billID, string.Empty, 0, rule.SourceFormId);
            ListSelectedRow[] selectedRows = new ListSelectedRow[] { row };
            // 调用下推服务，生成下游单据数据包
            ConvertOperationResult pushResult = null;
            PushArgs pushArgs = null;
            if (targetBillTypeId.IsNullOrEmptyOrWhiteSpace())
            {
                pushArgs = new PushArgs(rule, selectedRows);
            }
            else
            {
                pushArgs = new PushArgs(rule, selectedRows)
                {
                    TargetBillTypeId = targetBillTypeId
                };
            }


            // 执行下推操作，并获取下推结果
            pushResult = push(context, pushArgs, OperateOption.Create());

            // 获取生成的目标单据数据包
            DynamicObject[] targetDataEntities = pushResult.TargetDataEntities.Select(u => u.DataEntity).ToArray();

            // 读取目标单据元数据

            FormMetadata targetBillMeta = MetaDataHelper.getFormMetadataById(context, rule.TargetFormId);

            // 忽略全部需要交互性质的提示
            OperateOption option = OperateOption.Create();
            option.SetIgnoreWarning(true);// 保存

            IOperationResult saveResult = BusinessDataHelper.save(context, targetBillMeta.BusinessInfo, targetDataEntities, option);
            BusinessOperationHelper.operationResultProcess(saveResult);


            object[] primaryKeys = saveResult.SuccessDataEnity.Select(u => u.GetPrimaryKeyValue()).ToArray();
            PurchaseInstockFids = primaryKeys;
            if (actions != null)
            {
                for (int i = 0; i < actions.Count(); i++)
                {
                    if ("submit".Equals(actions[i]))
                    {
                        //提交操作
                        IOperationResult submitResult = BusinessDataHelper.submit(context, targetBillMeta.BusinessInfo, primaryKeys, "Submit", option);
                        BusinessOperationHelper.operationResultProcess(submitResult);
                    }
                    else if ("audit".Equals(actions[i]))
                    {
                        // 审核
                        IOperationResult auditResult = BusinessDataHelper.audit(context, targetBillMeta.BusinessInfo, primaryKeys);
                        BusinessOperationHelper.operationResultProcess(auditResult);
                    }
                }

            }
        }

        /// <summary>
        /// 根据规则ID获取规则
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public static ConvertRuleMetaData getConvertRuleById(Context ctx, String ruleId)
        {

            if (CommonHelper.DEBUG)
            {
                ConvertServiceProxy convertProxy = new ConvertServiceProxy();
                convertProxy.HostURL = CommonHelper.BASEURL;
                return convertProxy.GetConvertRule(ruleId);
            }
            else
            {
                return ConvertServiceHelper.GetConvertRule(ctx, ruleId);
            }

        }

        /// <summary>
        /// 根据FORM获取规则ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sourceFormId"></param>
        /// <param name="targetFormId"></param>
        /// <returns></returns>
        public static List<ConvertRuleMetaData> getRulesByFormId(Context ctx, string sourceFormId, string targetFormId)
        {

            if (CommonHelper.DEBUG)
            {
                ConvertServiceProxy convertProxy = new ConvertServiceProxy();
                convertProxy.HostURL = CommonHelper.BASEURL;
                return convertProxy.GetRulesByFormId(sourceFormId, targetFormId);
            }
            else
            {
                return ConvertServiceHelper.GetRulesByFormId(ctx, sourceFormId, targetFormId);
            }
        }




        public static ConvertOperationResult push(Context context, PushArgs pushArgs, OperateOption option = null)
        {

            if (CommonHelper.DEBUG)
            {
                ConvertServiceProxy convertProxy = new ConvertServiceProxy();
                convertProxy.HostURL = CommonHelper.BASEURL;
                return convertProxy.Push(pushArgs, option);
            }
            else
            {
                return ConvertServiceHelper.Push(context, pushArgs, option);

            }

        }



    }
}
