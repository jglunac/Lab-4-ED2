using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using API.Models;
using LZWCompression;
using System.Text;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CompressionController : ControllerBase
    {
        private IWebHostEnvironment _env;
        public CompressionController(IWebHostEnvironment env)
        {
            _env = env;
        }
        
        [HttpPost]
        [Route("compress/{name}")]
        public async Task<ActionResult> Compress(string name, [FromForm] IFormFile file)
        {

            //try
            //{


            string originalName = file.FileName;
            string path = _env.ContentRootPath;
            double originalSize;
            
            using (var Memory = new MemoryStream())
            {
                if (file != null && name != null)
                {
                    await file.CopyToAsync(Memory);
                }
                else
                {
                    return StatusCode(500);
                }
                using (FileStream stream = System.IO.File.Create(path + @"\Uploads\" + originalName))
                {
                    stream.Write(Memory.ToArray());
                    stream.Close();
                }
                LZW compressor = new LZW(path + @"\Uploads\" + originalName);
                byte[] ByteArray = compressor.Compress();
                originalSize = Memory.Length;
                double compressedSize = ByteArray.Length;
                CustomFile result = new CustomFile();
                result.Content = ByteArray;
                result.ContentType = "text / plain";
                result.FileName = name;
                return File(result.Content, result.ContentType, result.FileName + ".lzw");
            }
        }
    }
}
