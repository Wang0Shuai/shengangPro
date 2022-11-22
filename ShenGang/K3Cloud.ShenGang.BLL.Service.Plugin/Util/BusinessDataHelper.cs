using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceClient.BusinessData;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
  public  class BusinessDataHelper
    {
      public static IOperationResult save(Context context, BusinessInfo businessInfo, DynamicObject[] dataObject, OperateOption option = null, string operationNumber = "")
      {
         
          if (CommonHelper.DEBUG)
          {
              BusinessDataServiceProxy businessProxy = new BusinessDataServiceProxy();
              businessProxy.HostURL = CommonHelper.BASEURL;
              return  businessProxy.SaveData(businessInfo, dataObject[0]);
          }
          else {
              return BusinessDataServiceHelper.Save(context, businessInfo, dataObject, option, "Save");
          }
      }

      public static IOperationResult submit(Context context, BusinessInfo businessInfo, object[] Ids, string operationNumber, OperateOption option = null)
      {

          if (CommonHelper.DEBUG)
          {
              BusinessDataServiceProxy businessProxy = new BusinessDataServiceProxy();
              businessProxy.HostURL = CommonHelper.BASEURL;
              return businessProxy.Submit(businessInfo.GetForm().Id, operationNumber, Ids);
          }
          else
          {
              return  BusinessDataServiceHelper.Submit(context, businessInfo, Ids, "Submit", option);
          }
      }

      public static IOperationResult audit(Context context, BusinessInfo businessInfo, object[] Ids,OperateOption option = null)
      {

          if (CommonHelper.DEBUG)
          {
              BusinessDataServiceProxy businessProxy = new BusinessDataServiceProxy();
              businessProxy.HostURL = CommonHelper.BASEURL;
              return businessProxy.Audit(businessInfo.GetForm().Id, "Audit", Ids);
          }
          else
          {
              return BusinessDataServiceHelper.Audit(context, businessInfo, Ids,  option);
          }
      }


    }
}
