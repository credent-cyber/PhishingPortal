﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using IO = System.IO;
using PhishingPortal.Dto;

namespace PhishingPortal.Server.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<FileUploadController> logger;
        private readonly IConfiguration configuration;

        public FileUploadController(IWebHostEnvironment env, ILogger<FileUploadController> logger, IConfiguration configuration)
        {
            this.env = env;
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpPost("AppendFile/{fragment}")]
        public async Task<UploadResult> UploadFileChunk(int fragment, IFormFile file)
        {

            try
            {
                var trainingVideoPath = configuration.GetValue<string>("TrainingVideoPath");

                var fileLocation = Path.Combine(env.ContentRootPath, trainingVideoPath, file.FileName);

                if (fragment == 0 && IO.File.Exists(fileLocation))
                {
                    IO.File.Delete(fileLocation);
                }
                using (var fileStream = new FileStream(fileLocation, FileMode.Append, FileAccess.Write, FileShare.None))
                using (var bw = new BinaryWriter(fileStream))
                {
                    await file.CopyToAsync(fileStream);
                }

                var fileName = Path.GetFileName(fileLocation);
                return new UploadResult { IsUploaded = true, FileLocation = fileName };
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception: {0}", exception.Message);
            }
            return new UploadResult { IsUploaded = false, FileLocation = "" };
        }


    }
}
