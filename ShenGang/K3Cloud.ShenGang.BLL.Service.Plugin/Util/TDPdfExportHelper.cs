using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.ListFilter;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.Log;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Web.Core;
using Kingdee.BOS.Web.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin.Utils
{
    public class TDPdfExportHelper
    {
        public static String export(Context ctx, String formid, String billid, List<string> templateIds, string pdfName)
        {

            IDynamicFormView view = null;
            view = CreateView(ctx, formid);
            CommonSession.GetCurrent(ctx.UserToken).SessionManager.GetOrAdd(view.PageId, view.GetType().Name, view);

            //单据内码与套打模板标识一一对应
            List<String> billIds = new List<String>(); //单据内码
            foreach (var bill in templateIds)
            {
                billIds.Add(billid);
            }
            List<String> tempIds = templateIds;//套打模板标识
            PrintExportInfo pExInfo = new PrintExportInfo();
            pExInfo.PageId = view.PageId;
            pExInfo.FormId = view.BillBusinessInfo.GetForm().Id;
            pExInfo.BillIds = billIds;//单据内码
            pExInfo.TemplateIds = tempIds;//套打模板ID
            pExInfo.FileType = ExportFileType.PDF;//文件格式
            pExInfo.ExportType = ExportType.ByPage;//导出格式
            string fileName = pdfName + ".PDF";
            string filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, fileName);//文件存储路径
            // string filePath = Path.Combine(temppath, Guid.NewGuid().ToString() + ".PDF");
            pExInfo.FilePath = filePath;//文件输出路径
            Export(pExInfo, view);
            return fileName;
        }

        private static void Export(PrintExportInfo pExInfo, IDynamicFormView view)
        {
            if (view is IListViewService)
            {
                ListView list = (ListView)view;
                list.ExportNotePrint(pExInfo);
            }
        }


        public static IDynamicFormView CreateView(Context ctx, string formId)
        {
            FormMetadata _metadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, formId);
            var OpenParameter = CreateOpenParameter(OperationStatus.VIEW, ctx, formId, _metadata);
            var Provider = GetListServiceProvider(OpenParameter);

            string importViewClass = "Kingdee.BOS.Web.List.ListView,Kingdee.BOS.Web";
            Type type = Type.GetType(importViewClass);
            IListViewService View = (IListViewService)Activator.CreateInstance(type);

            ((IListViewService)View).Initialize(OpenParameter, Provider);
            ((IListViewService)View).LoadData();
            return (IDynamicFormView)View;
        }

        private static ListOpenParameter CreateOpenParameter(OperationStatus status, Context ctx, string formId, FormMetadata _metadata)
        {

            ListOpenParameter openPara = new ListOpenParameter(formId, _metadata.GetLayoutInfo().Id);
            Form form = _metadata.BusinessInfo.GetForm();
            openPara = new ListOpenParameter(formId, string.Empty);
            openPara.Context = ctx;
            openPara.ServiceName = form.FormServiceName;
            openPara.PageId = Guid.NewGuid().ToString();

            // 单据
            openPara.FormMetaData = _metadata;
            openPara.LayoutId = _metadata.GetLayoutInfo().Id;
            openPara.ListFormMetaData = (FormMetadata)FormMetaDataCache.GetCachedFormMetaData(ctx, FormIdConst.BOS_List);

            // 操作相关参数
            openPara.SetCustomParameter(FormConst.PlugIns, form.CreateListPlugIns());
            openPara.SetCustomParameter("filterschemeid", "");
            openPara.SetCustomParameter("listfilterparameter", new ListRegularFilterParameter());
            // 修改主业务组织无须用户确认
            openPara.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);

            openPara.SetCustomParameter("SessionManager", CommonSession.GetCurrent(ctx.UserToken).SessionManager);
            return openPara;
        }

        private static IResourceServiceProvider GetListServiceProvider(DynamicFormOpenParameter param)
        {
            FormServiceProvider provider = new FormServiceProvider();
            provider.Add(typeof(IDynamicFormView), CreateListView(param));
            provider.Add(typeof(DynamicFormViewPlugInProxy), new ListViewPlugInProxy());
            provider.Add(typeof(DynamicFormModelPlugInProxy), new ListModelPlugInProxy());
            provider.Add(typeof(IDynamicFormModelService), GetListModel(param));
            provider.Add(typeof(IListFilterModelService), GetListFilterModel());
            var type = TypesContainer.GetOrRegister("Kingdee.BOS.Business.DynamicForm.DefaultValue.DefaultValueCalculator,Kingdee.BOS.Business.DynamicForm");
            provider.Add(typeof(IDefaultValueCalculator), Activator.CreateInstance(type));
            // 注册IDBModelService
            type = TypesContainer.GetOrRegister("Kingdee.BOS.Business.DynamicForm.DBModel.DBModelService,Kingdee.BOS.Business.DynamicForm");
            provider.Add(typeof(IDBModelService), Activator.CreateInstance(type));
            return provider;
        }



        /// 获取视图
        private static IDynamicFormView CreateListView(DynamicFormOpenParameter param)
        {
            Form form = param.FormMetaData.BusinessInfo.GetForm();
            if (form.FormGroups != null && form.FormGroups.Count > 0)
            {
                return new TreeListView();
            }
            else
            {
                return new ListView();
            }
        }


        /// 获取视图模型
        private static IDynamicFormModelService GetListModel(DynamicFormOpenParameter param)
        {
            Form form = param.FormMetaData.BusinessInfo.GetForm();
            if (form.FormGroups != null && form.FormGroups.Count > 0)
            {
                var type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.List.TreeListModel,Kingdee.BOS.Model");
                return (IDynamicFormModelService)Activator.CreateInstance(type);
            }
            else
            {
                var type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.List.ListModel,Kingdee.BOS.Model");
                return (IDynamicFormModelService)Activator.CreateInstance(type);
            }
        }



        /// 创建过滤条件模型
        ///
        ///
        private static IListFilterModelService GetListFilterModel()
        {
            Type type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.ListFilter.ListFilterModel,Kingdee.BOS.Model");
            return (IListFilterModelService)Activator.CreateInstance(type);
        }
    }
}
