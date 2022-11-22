using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.FileServer.Core;
using Kingdee.BOS.FileServer.Core.Object;
using Kingdee.BOS.FileServer.ProxyService;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K3Cloud.ShenGang.BLL.Service.Plugin.Utils
{
   public class FileHelper
    {
        /// <summary>
        /// 保存附件跟单据关系
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fileInfos"></param>
        /// <param name="formId"></param>
        public static string SaveAttachment(Context ctx, List<Dictionary<string, string>> fileInfos, string formId, int storageType)
        {
            // 获取附件表的元数据类
            var formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, FormIdConst.BOS_Attachment);
            List<DynamicObject> dynList = new List<DynamicObject>();
            StringBuilder sb = new StringBuilder();
            foreach (Dictionary<string, string> file in fileInfos)
            {

                /**
                * 此处几个关键属性解读：
                * 1. BillType 关联的模型的 FormId
                * 2. BillNo 关联的单据编号，用于确定此附件是属于哪张单据
                * 3. InterID 关联的单据/基础资料 ID，附件列表就是根据这个 ID 进行加载
                * 4. EntryInterID 关联的单据体 ID，这里我们只演示单据头，所以固定设置
                为-1
                * 5. AttachmentSize 系统统一按照 KB 单位进行显示，所以需要除以 1024
                * 6. FileStorage 文件存储类型，0 为数据库，1 为文件服务，2 为亚马逊云，3 为金蝶云
                */
                string fileName = file["FileName"];
                string filePath = file["FilePath"];
                string fBillNo = file["FBillNo"];
                string fid = file["FID"];
                var dyn = new DynamicObject(formMetadata.BusinessInfo.GetDynamicObjectType());
                var dataBuff = System.IO.File.ReadAllBytes(filePath);
                // 将数据上传至文件服务器，并返回上传结果
                var result = UploadAttachment(ctx, fileName, dataBuff);
                if (!result.Success)
                {
                    // 上传失败，收集失败信息
                    sb.AppendLine(string.Format("附件：{0}，上传失败：{1}", fileName, result.Message));
                    throw new Exception("附件上传失败");
                }

                // 通过这个 FileId 就可以从文件服务器下载到对应的附件
                dyn["FileId"] = result.FileId;
                dyn["AttachmentSize"] = Math.Round(dataBuff.Length / 1024.0, 2);
                // 此处我们不绑定到特定的单据，为了简化示例，只实现单纯的文件上传与下载

                dyn["BillType"] = formId; //FormId
                dyn["BillNo"] = Convert.ToString(fBillNo); // 单据编号
                dyn["InterID"] = Convert.ToString(fid); // 内码，这个 ID 将作为我们下载的识别标识
                // 上传文件服务器成功后才加入列表
                dyn["AttachmentName"] = fileName;

                dyn["EntryInterID"] = -1;// 参照属性解读
                dyn["CreateMen_Id"] = Convert.ToInt32(ctx.UserId);
                dyn["CreateMen"] = GetUser(ctx);
                dyn["ModifyTime"] = dyn["CreateTime"] = TimeServiceHelper.GetSystemDateTime(ctx);
                dyn["ExtName"] = ".pdf";
                dyn["FileStorage"] = storageType.ToString();
                dyn["EntryKey"] = "";
                dyn["F_PATC_CHKECONTRACT"] = true;
                dynList.Add(dyn);
            }
            if (dynList.Count > 0)
            {
                // 所有数据加载完成后再一次性保存全部
                BusinessDataServiceHelper.Save(ctx, dynList.ToArray());
                sb.AppendLine();
                sb.AppendLine(string.Join(",", dynList.Select(dyn => dyn["AttachmentName"].ToString()).ToArray()) + ",上传成功");

            }
            return sb.ToString();
        }

        /// <summary>
        /// 保存附件跟单据关系
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fileInfos"></param>
        /// <param name="formId"></param>
        public static string SaveAttachmentWithoutUpdate(Context ctx, DynamicObject[] fileInfos, string formId, string billNo, string fid)
        {
            // 获取附件表的元数据类
            var formMetadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, FormIdConst.BOS_Attachment);
            List<DynamicObject> dynList = new List<DynamicObject>();
            StringBuilder sb = new StringBuilder();

            foreach (var file in fileInfos)
            {
                var dyn = new DynamicObject(formMetadata.BusinessInfo.GetDynamicObjectType());
                dyn["FileId"] = file["FileId"];
                dyn["AttachmentSize"] = file["AttachmentSize"];

                dyn["BillType"] = formId; //FormId
                dyn["BillNo"] = billNo; // 单据编号
                dyn["InterID"] = fid; // 内码，这个 ID 将作为我们下载的识别标识
                // 上传文件服务器成功后才加入列表
                dyn["AttachmentName"] = file["AttachmentName"];

                dyn["EntryInterID"] = -1;// 参照属性解读
                dyn["CreateMen_Id"] = Convert.ToInt32(ctx.UserId);
                dyn["CreateMen"] = GetUser(ctx);
                dyn["ModifyTime"] = dyn["CreateTime"] = TimeServiceHelper.GetSystemDateTime(ctx);
                dyn["ExtName"] = ".pdf";
                dyn["FileStorage"] = file["FileStorage"];
                dyn["EntryKey"] = "";
                dynList.Add(dyn);
            }

            if (dynList.Count > 0)
            {
                // 所有数据加载完成后再一次性保存全部
                BusinessDataServiceHelper.Save(ctx, dynList.ToArray());
                sb.AppendLine();
                sb.AppendLine(string.Join(",", dynList.Select(dyn => dyn["AttachmentName"].ToString()).ToArray()) + ",上传成功");

            }
            return sb.ToString();
        }
        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dataBuff"></param>
        /// <returns></returns>
        public static FileUploadResult UploadAttachment(Context ctx, string fileName, byte[] dataBuff)
        {
            // 初始化上传下载服务，这个 Service 会根据 Cloud 配置自动上传到对应的文件服务器
            var service = new UpDownloadService();
            int len = 0, less = 0;
            string fileId = null;
            byte[] buff = null;
            while (len < dataBuff.Length)
            {
                // 文件服务器采用分段上传，每次上传 4096 字节, 最后一次如果不够则上传剩余长度
                less = (dataBuff.Length - len) >= 4096 ? 4096 : (dataBuff.Length - len);
                buff = new byte[less];
                Array.Copy(dataBuff, len, buff, 0, less);
                len += less;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buff))
                {
                    TFileInfo tFile = new TFileInfo()
                    {
                        FileId = fileId,
                        FileName = fileName,
                        CTX = ctx,
                        Last = len >= dataBuff.Length,//标记是否为文件的最后一个片段
                        Stream = ms
                    };
                    var result = service.UploadAttachment(tFile);
                    // 注意点：上传时 fileId 传入 null 为新文件开始,会返回一个文件的fileId，后续采用这个 fileId 标识均为同一文件的不同片段。
                    fileId = result.FileId;
                    if (!result.Success)
                    {
                        return result;
                    }
                }
            }
            return new FileUploadResult()
            {
                Success = true,
                FileId = fileId
            };
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private static DynamicObject GetUser(Context ctx)
        {
            string userID = ctx.UserId.ToString();
            OQLFilter filter = OQLFilter.CreateHeadEntityFilter(string.Format("FUSERID={0}", userID));
            return BusinessDataServiceHelper.Load(ctx, FormIdConst.SEC_User, null, filter).FirstOrDefault();
        }

        /// <summary>
        /// 获取单据附件
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fids"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public static DynamicObject[] GetFileData(Context ctx, List<string> fids, string formId)
        {
            string fidWhere = "";
            foreach (var fid in fids)
            {
                fidWhere = fidWhere + ",'" + fid + "'";
            }
            var filter = OQLFilter.CreateHeadEntityFilter(string.Format("FBillType = '{0}' AND FInterID in ({1})", formId, fidWhere.Substring(1)));
            var dyns = BusinessDataServiceHelper.Load(ctx, FormIdConst.BOS_Attachment, null, filter);
            return dyns;
        }
        public static byte[] GetFile(Context ctx, string fileId)
        {
            // 采用文件服务器存储
            var service = new UpDownloadService();
            TFileInfo tFile = new TFileInfo()
            {
                FileId = fileId,
                CTX = ctx
            };
            var fileBytes = service.GetFileData(tFile);
            return fileBytes;
        }
    }
}
