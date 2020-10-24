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
            string path = _env.ContentRootPath;
            string OriginalName = file.FileName;
            string uploadPath = path + @"\Uploads\" + OriginalName;
            string downloadPath = path + @"\Compressions\" + name;
            byte[] FileBytes;

            double originalSize;

            try
            {
                if (file != null)
                {
                    using (FileStream fs = System.IO.File.Create(uploadPath))
                    {
                        await file.CopyToAsync(fs);
                        originalSize = fs.Length;
                    }

                LZW Compressor = new LZW(uploadPath);
                FileBytes = Compressor.Compress(uploadPath, OriginalName, 100);
                double compressedSize = FileBytes.Length;

                Compressor.UpdateCompressions(path, OriginalName, uploadPath, originalSize, compressedSize);
                return File(FileBytes, "text/plain", name + ".lzw");
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        [Route("decompress")]
        public async Task<ActionResult> Decompress([FromForm] IFormFile file)
        {
            string path = _env.ContentRootPath;
            string OriginalName = file.FileName;
            OriginalName = OriginalName.Substring(0, OriginalName.Length - 4);
            string downloadPath = path + @"\Compressions\" + OriginalName;
            byte[] FileBytes;
            try
            {
                if (file != null)
            {
                    using (FileStream fs = System.IO.File.Create(downloadPath))
                    {
                        await file.CopyToAsync(fs);
                    }
                    LZW Compressor = new LZW(downloadPath);
                    FileBytes = Compressor.Decompress(downloadPath, 100);
                    return File(FileBytes, "text/plain", Compressor.Name); ;
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult ReturnJSON()
        {
            string path = _env.ContentRootPath;
            using (StreamReader reader = new StreamReader(path + "/CompressedFiles.json"))
            {
                string json = reader.ReadToEnd();
                List<LZWCompression.LZWCompression> Compressions = CompressionDeserialize(json);
                JsonSerializer.Serialize(Compressions);
                return Ok(Compressions);
            }
        }

        public static List<LZWCompression.LZWCompression> CompressionDeserialize(string content)
        {
            return JsonSerializer.Deserialize<List<LZWCompression.LZWCompression>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
    
}
