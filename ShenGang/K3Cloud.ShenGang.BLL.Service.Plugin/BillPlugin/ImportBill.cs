using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [HotUpdate]
    [Description("进口单证")]
    public class ImportBill : AbstractDynamicFormPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            #region MyRegion


            if (e.BarItemKey.Equals("RCQV_Entering"))
            {
                DynamicObject obj_ImportBil = this.View.Model.DataObject;
                string FDocumentStatus = Convert.ToString(obj_ImportBil["DocumentStatus"]);
                if (FDocumentStatus != "C")
                {
                    throw new KDBusinessException("", "单据未审核，请先审核！");
                }

                string FBILLNO = obj_ImportBil["BillNo"] == null ? "" : Convert.ToString(obj_ImportBil["BillNo"]);
                int FID = Convert.ToInt32(obj_ImportBil["Id"]);
                decimal realTariff = Convert.ToDecimal(obj_ImportBil["F_PDHJ_Amount3"]);

                string sql = string.Format(@"select FSBILLID from T_STK_INSTOCKENTRY_LK a
where a.FRULEID='PUR_ReceiveBill-STK_InStock' and FSBILLID ={0}", FID);
                DynamicObjectCollection doc = DBUtils.ExecuteDynamicObject(this.Context, sql);
                if (doc != null && doc.Count > 0)
                {
                    throw new KDBusinessException("", "已存在下游单据【采购入库单】！");
                }

                ConvertHelper.PurchaseInstockFids = null;

                ConvertHelper.pushDownSaveSubmitAuditBillByRuleId(this.Context, "a1ff32276cd9469dad3bf2494366fa4f", FID.ToString(), "PUR_ReceiveBill-STK_InStock");

                object[] primaryKeys = ConvertHelper.PurchaseInstockFids;
                if (primaryKeys != null)
                {
                    //已经设置自动下推标准应付单 
                    //for (int i = 0; i < primaryKeys.Count(); i++)
                    //{
                    //    ConvertHelper.pushDownSaveSubmitAuditBillByRuleId(this.Context, "3c6f819d78ac4d5981891956c4595b20", Convert.ToString(primaryKeys[i]), "AP_InStockToPayableMap");
                    //}
                    if (primaryKeys.Count() > 1)
                    {
                        throw new KDBusinessException("", "进口单证与生成的采购入库单不是一对一关系，不能生成关税费用应付！");
                    }
                    else
                    {
                        sql = string.Format(@"select FBILLNO from T_STK_INSTOCK where FID ={0}", Convert.ToInt32(primaryKeys[0]));
                        doc = DBUtils.ExecuteDynamicObject(this.Context, sql);
                        string inStockBillNo = Convert.ToString(doc[0]["FBILLNO"]);
                        // 构建一个IBillView实例，通过此实例，可以方便的填写应收单各属性
                        IBillView billView = CommonHelper.CreateK3BillView(this.Context, "AP_Payable");
                        // 新建一个空白应收单
                        // billView.CreateNewModelData();
                        ((IBillViewService)billView).LoadData();
                        // 触发插件的OnLoad事件：
                        // 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
                        // 如果不触发OnLoad事件，会导致主业务组织赋值不成功
                        DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
                        eventProxy.FireOnLoad();

                        IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;
                        
                        dynamicFormView.SetItemValueByID("FBillTypeID", "3c6f819d78ac4d5981891956c4595b20", 0);
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FBillTypeID", 0);

                        dynamicFormView.UpdateValue("FBUSINESSTYPE", 0, "FY");
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FBUSINESSTYPE", 0);

                        dynamicFormView.SetItemValueByID("FSUPPLIERID", 100073, 0);
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FSUPPLIERID", 0);
                       
                        billView.Model.DeleteEntryData("FEntityDetail");

                        billView.Model.BatchCreateNewEntryRow("FEntityDetail", 1);
                        int iRowIndex = billView.Model.GetEntryCurrentRowIndex("FEntityDetail");

                        dynamicFormView.SetItemValueByID("FCOSTID", 20047, iRowIndex);
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FCOSTID", iRowIndex);

                        dynamicFormView.UpdateValue("FPriceQty", iRowIndex, 1);
                        dynamicFormView.UpdateValue("FTaxPrice", iRowIndex, realTariff);

                        DynamicObjectCollection obj_costPayableDe = billView.Model.DataObject["AP_PAYABLEENTRY"] as DynamicObjectCollection;
                        DynamicObject obj_costPayable = obj_costPayableDe[0];
                        var dyc = new DynamicObject((obj_costPayable["INSTOCKID"] as DynamicObjectCollection).DynamicCollectionItemPropertyType);
                        //给基础资料的Id赋值
                        dyc["INSTOCKID_Id"] = Convert.ToInt32(primaryKeys[0]);
                        //单个的账簿Id对应的账簿实体
                        //dyc["FINSTOCKID"] = item as DynamicObject;
                        (obj_costPayable["INSTOCKID"] as DynamicObjectCollection).Add(dyc);

                        //DynamicObjectCollection FOperation = obj_costPayable["INSTOCKID"] as DynamicObjectCollection;

                        //获取单据体操作工id集合

                        //DynamicObjectCollection entryOp = this.View.Model.GetValue("FINSTOCKID") as DynamicObjectCollection;

                        //if (FOperation.Count > 0)
                        //{
                        //    //多选基础资料主键集合

                        //    MulBaseDataField mulField = this.View.BusinessInfo.GetField("FINSTOCKID") as MulBaseDataField;

                        //    string[] pkValues = FOperation.Select(p => p[mulField.RefIDDynamicProperty.Name].ToString()).Distinct().ToArray();

                        //    ////给单据上面的多选基础资料字段B赋值

                        //    //this.View.Model.SetValue("FMulBaseB", pkValues, e.Row);

                        //    //获取单据体行数



                        //    billView.Model.SetValue("FINSTOCKID", pkValues, iRowIndex);

                        //    billView.Model.SetValue("FINSTOCKIDnumber", FOperation[0]["INSTOCKID_Id"], iRowIndex);

                        //}



                        IOperationResult opResult = new OperationResult();
                        // 保存物料
                        OperateOption saveOption = OperateOption.Create();
                        IOperationResult result = saveBill(billView, saveOption);

                        this.View.ShowMessage("入关操作执行完毕，操作成功！");
                    }
                }
                else
                {
                    throw new KDBusinessException("", "未能生成采购入库,检查系统是否已存在！");
                }



            }
            #endregion

        }

        private IOperationResult saveBill(IBillView billView, OperateOption saveOption)
        {
            // 设置FormId
            Form form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }

            // 调用保存操作
            IOperationResult saveResult = BusinessDataServiceHelper.Save(
            this.Context,
            billView.BillBusinessInfo,
            billView.Model.DataObject,

            saveOption,
            "Save");

            if (!saveResult.IsSuccess)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in saveResult.ValidationErrors)
                {
                    sb.AppendLine(item.Message);
                }
                throw new Exception(sb.ToString());
            }

          ////  IOperationResult checkResult = BusinessDataServiceHelper.Submit(
          //// this.Context,
          //// billView.BillBusinessInfo,
          ////new object[] { billView.Model.DataObject["Id"] },

          //// "Submit",
          //// saveOption);

          ////  if (!checkResult.IsSuccess)
          ////  {
          ////      StringBuilder sb = new StringBuilder();
          ////      foreach (var item in checkResult.ValidationErrors)
          ////      {
          ////          sb.AppendLine(item.Message);
          ////      }
          ////      throw new Exception(sb.ToString());
          ////  }

          ////  IOperationResult AuditResult = BusinessDataServiceHelper.Audit(
          //// this.Context,
          //// billView.BillBusinessInfo,
          ////new object[] { billView.Model.DataObject["Id"] },

          //// saveOption);

          ////  if (!AuditResult.IsSuccess)
          ////  {
          ////      StringBuilder sb = new StringBuilder();
          ////      foreach (var item in AuditResult.ValidationErrors)
          ////      {
          ////          sb.AppendLine(item.Message);
          ////      }
          ////      throw new Exception(sb.ToString());
          ////  }

            return saveResult;

        }
    }
}
