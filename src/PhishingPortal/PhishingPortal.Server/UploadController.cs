using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Hosting;
using PhishingPortal.Common;
using Microsoft.Extensions.Configuration;

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
		   
		[HttpPost]
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
			}
		}
	
}
