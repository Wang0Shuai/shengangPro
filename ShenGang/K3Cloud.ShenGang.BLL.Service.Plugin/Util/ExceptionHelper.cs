using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin
{
    [Kingdee.BOS.Util.HotUpdate]
   /// <summary>
   /// 异常帮助类
   /// </summary>
   public  class ExceptionHelper
    {
        /// <exception cref="KDException"></exception>
       public static void throwKDException(String message)  {
           throw new KDException("00", message);
       }

       /// <exception cref="KDBusinessException"></exception>
       public static void throwKDBusinessException(String message)
       {
           throw new KDBusinessException(string.Empty, message);
       }
         
         
    }
}
