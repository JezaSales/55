using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class FileUploadModel
    {
        public IFormFile file { get; set; }
        public List<IFormFile> Files { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int Id { get; set; }
        public bool IsThumbnailRequired { get; set; }
    }
}
