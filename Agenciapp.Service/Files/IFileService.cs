using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.Files
{
    public interface IFileService
    {
        Task<Result<string>> CreateImageAsync(IFormFile file, string type);
        Result DeleteImage(string fileName, string type);
        Task<Result<string>> CreateDocumentAsync(IFormFile file, string type);
        Result DeleteDocument(string fileName, string type);
    }

    public class FileService : IFileService
    {
        private readonly IHostingEnvironment _webHostEnvironment;

        public FileService(IHostingEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result<string>> CreateDocumentAsync(IFormFile file, string type)
        {
            var fileNameAux = file.FileName.Split('.')[0];

            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{date}.{fileNameAux[fileNameAux.Length - 1]}";
            string filePath = $"{_webHostEnvironment.WebRootPath}" +
                              $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                              $"{type}";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Result.Success<string>(fileName);
        }

        public async Task<Result<string>> CreateImageAsync(IFormFile file, string type)
        {
            var fileNameAux = file.FileName.Split('.');

            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{date}.{fileNameAux[fileNameAux.Length - 1]}";
            string filePath = $"{_webHostEnvironment.WebRootPath}" +
                              $"{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}" +
                              $"{type}";
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Result.Success<string>(fileName);

        }

        public Result DeleteDocument(string fileName, string type)
        {
            string filePath = $"{_webHostEnvironment.WebRootPath}" +
                              $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                              $"{type}";
            filePath = Path.Combine(filePath, fileName);
            if (File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return Result.Success();
        }

        public Result DeleteImage(string fileName, string type)
        {
            string filePath = $"{_webHostEnvironment.WebRootPath}" +
                              $"{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}" +
                              $"{type}";
            filePath = Path.Combine(filePath, fileName);
            if (File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return Result.Success();


        }

    }
}
