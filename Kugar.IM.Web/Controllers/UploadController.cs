using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.Web;
using Kugar.Core.Web.JsonTemplate.Helpers;
using Kugar.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Kugar.IM.Web.Controllers
{
    /// <summary>
    /// 媒体文件
    /// </summary>
    public class FileIOController:IMBaseController
    {
        /// <summary>
        /// 上传媒体文件,使用标准form post方式, 包含contentType 和 file 文件流字段 图片/音频不能超过2m
        /// </summary>
        /// <param name="storge"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultReturn<string>> Upload([FromServices]IStorage storge)
        {
            var contentType = Request.GetInt("ContentType");

            var file = Request.GetFile();

            var path = $"/uploads/im/media/{contentType}/{DateTime.Now:yyyyMMdd}/{file.GetRandomName()}";

            using (var stream=file.OpenReadStream())
            {
                await storge.StorageFileAsync(path, stream, true);

                return new SuccessResultReturn<string>($"{(Request.IsHttps?"https":"http")}://{Request.Host.Host}:{Request.Host.Port}{path}");
            }
        }
    }
}
