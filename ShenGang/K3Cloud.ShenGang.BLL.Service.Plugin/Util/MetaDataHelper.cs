using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceFacade.KDServiceClient.Metadata;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
  public  class MetaDataHelper
    {
      public static FormMetadata getFormMetadataById(Context ctx, String formid)
      {
          FormMetadata metadata = null;
          if (CommonHelper.DEBUG)
          {
              MetadataServiceProxy metaServiceProxy = new MetadataServiceProxy();
              metaServiceProxy.HostURL = CommonHelper.BASEURL;
              metadata = metaServiceProxy.GetFormMetadata(formid);
          }
          else
          {
              metadata =
             MetaDataServiceHelper.Load(ctx, formid) as FormMetadata;
          }
          return metadata;
      }
    }
}
