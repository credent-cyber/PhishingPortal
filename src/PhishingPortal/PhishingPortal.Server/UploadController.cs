using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Hosting;
using PhishingPortal.Common;
using Microsoft.Extensions.Configuration;
using PhishingPortal.UI.Blazor.Shared.Components;
using PhishingPortal.Dto.Auth;
using Microsoft.EntityFrameworkCore;

namespace PhishingPortal.Server
{

    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {

        //private readonly IWebHostEnvironment env;

        //public UploadController(IWebHostEnvironment env)
        //{
        //	this.env = env;

        //}
        IConfiguration _config;
        public UploadController(IConfiguration Configuration)
        {
            _config = Configuration;

        }

        /*	[HttpPost]
                public async Task Post([FromBody] ImageFile[] files)
                {
                var loc = _config.GetValue<string>("TemplateImgPath");
                    foreach (var file in files)
                    {
                        var buf = Convert.FromBase64String(file.base64data);
                        //await System.IO.File.WriteAllBytesAsync(env.ContentRootPath + System.IO.Path.DirectorySeparatorChar + file.fileName, buf);
                        #if DEBUG
                            await System.IO.File.WriteAllBytesAsync("./../PhishingPortal.UI.Blazor/wwwroot/timg" + System.IO.Path.DirectorySeparatorChar + file.fileName, buf);

                        #else
                            await System.IO.File.WriteAllBytesAsync(loc + System.IO.Path.DirectorySeparatorChar + file.fileName, buf);

                        #endif   

                    }
                }*/

        [HttpPost("upload")]
        public async Task<ProfileImageFile> UploadFile(ProfileImageFile model)
        {
                string fileName = string.Empty;
                var user = User.Identity.Name;

                // Convert Base64 string to byte array
                byte[] fileBytes = Convert.FromBase64String(model.base64data);
            if (model.IsProfilePic)
            {
                 fileName = $"{user}-pp.{model.fileName.Split('.')[1]}";
            }
            else
            {
                 fileName = $"{user}-Bg.{model.fileName.Split('.')[1]}";
            }
                string uploadPath = _config.GetValue<string>("ProfileImgPath");
                Directory.CreateDirectory(uploadPath); // Create directory if it doesn't exist
                string filePath = Path.Combine(uploadPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
                model.fileName = fileName;
                return model;
            
        }

    }

}
