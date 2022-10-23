using Microsoft.AspNetCore.StaticFiles;
using Microsoft.JSInterop;

namespace File_Upload.services
{

    public interface IFileDownload
    {
        public Task<List<String>> GetUploadedFiles();
        public Task DownloadFile(string url);
    }
    public class FileDownload : IFileDownload
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IJSRuntime _js;

        public FileDownload(IWebHostEnvironment webHostEnvironment, IJSRuntime js)
        {
            _webHostEnvironment = webHostEnvironment;
            _js = js;
        }

        public async Task DownloadFile(string url)
        {
            await _js.InvokeVoidAsync("downloadFile", url);
        }

        public async Task<List<string>> GetUploadedFiles()
        {
            var base64Url = new List<string>();
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "upload");
            var files = Directory.GetFiles(uploadPath);

            if(files is not null && files.Length > 0)
            {
                foreach(var file in files)
                {
                    using(var fileInput = new FileStream(file,FileMode.Open, FileAccess.Read))
                    {
                        var memorystream = new MemoryStream();
                        await fileInput.CopyToAsync(memorystream);
                        var buffer = memorystream.ToArray();
                        var filetype = GetMimeTypeForFileExtension(file);
                        base64Url.Add($"data:{filetype}; base64, {Convert.ToBase64String(buffer)}");
                    }
                }
            }

            return base64Url;
        }
        private string GetMimeTypeForFileExtension(string filePath)
        {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string mimeType))
            {
                return DefaultContentType;
            }

            return mimeType;
        }
    }

    
}
