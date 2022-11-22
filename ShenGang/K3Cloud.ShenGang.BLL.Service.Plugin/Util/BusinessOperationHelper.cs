using Kingdee.BOS.Core.DynamicForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
    /// <summary>
    /// 业务操作类处理
    /// </summary>
    public   class BusinessOperationHelper
    {
        /// <summary>
        /// 处理操作结果 
        /// </summary>
        /// <param name="operationResult"></param>
        public static void operationResultProcess(IOperationResult operationResult)
        {
            if (!operationResult.IsSuccess)
            {
                StringBuilder message = new StringBuilder();
                List<Kingdee.BOS.Core.Validation.ValidationErrorInfo> list = operationResult.ValidationErrors;
                if (list != null && list.Count > 0)
                {
                   
                    for (int i = 0; i < list.Count; i++)
                    {
                        message.Append(list[i].Message);
                    }
                    
                }
                
                foreach(OperateResult r in operationResult.OperateResult)             
                {
                    message.Append(r.Message);
                }

                if (message.Length > 0) {
                    ExceptionHelper.throwKDBusinessException(message.ToString());
                }

               
            }

        }
    }
}
