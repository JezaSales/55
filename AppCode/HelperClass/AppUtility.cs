using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.HelperClass
{
    public class AppUtility
    {
        public static AppUtility O => instance.Value;
        private static Lazy<AppUtility> instance = new Lazy<AppUtility>(() => new AppUtility());
        private AppUtility() { }

        public IResponseStatus UploadFile(FileUploadModel request)
        {
            var response = IsFileValid(request.file);
            if (response.Statuscode == ErrorCodes.One)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(request.FilePath);
                    if (!Directory.Exists(sb.ToString()))
                    {
                        Directory.CreateDirectory(sb.ToString());
                    }
                    var filename = ContentDispositionHeaderValue.Parse(request.file.ContentDisposition).FileName.Trim('"');
                    string originalExt = Path.GetExtension(filename).ToLower();
                    string[] Extensions = { ".png", ".jpeg", ".jpg" };
                    if (Extensions.Contains(originalExt))
                    {
                        //originalExt = ".jpg";
                    }
                    string originalFileName = Path.GetFileNameWithoutExtension(filename).ToLower() + originalExt;
                    
                    if (string.IsNullOrEmpty(Path.GetExtension(request.FileName)))
                    {
                        request.FileName = string.Concat(request.FileName, originalExt);
                    }
                    request.FileName = string.IsNullOrEmpty(request.FileName) ? originalFileName.Trim() : request.FileName;
                    sb.Append(request.FileName);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        request.file.CopyTo(fs);
                        fs.Flush();
                    }
                    response.Statuscode = ErrorCodes.One;
                    response.Msg = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    response.Msg = "Error in file uploading. Try after sometime...";
                    // response.Error = ex.Message;
                }
            }
            return response;
        }

        public IResponseStatus CreateJsonFile(CreateJsonFileModel createJsonFileModel)
        {
            try
            {
                var jsonFile = string.Concat(createJsonFileModel.Path, createJsonFileModel.FileName);
                if (!Directory.Exists(createJsonFileModel.Path))
                {
                    Directory.CreateDirectory(createJsonFileModel.Path);
                }
                File.WriteAllText(jsonFile, createJsonFileModel.Json);
                return new ResponseStatus
                {
                    Statuscode = ErrorCodes.One,
                    CommonBool = true,
                    Msg = ErrorCodes.SUCCESS
                };
            }
            catch (Exception ex)
            {
                return new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.TempError,
                    ErrorMsg = ex.Message
                };
            }
        }
        public IResponseStatus IsFileValid(IFormFile file)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Temperory Error"
            };
            if (file != null)
            {
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename).ToLower();

                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    res.Msg = "Invalid File Format!";
                    return res;
                }
                else if (!file.ContentType.Any())
                    res.Msg = "File not found!";
                else if (file.Length < 1)
                    res.Msg = "Empty file not allowed!";
                else if (file.Length / 1024 > 1024 /*&& !ext.ToLower().In(".zip", ".rar")*/)
                    res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                else
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "it is a valid file";
                }

            }
            else
            {
                res.Msg = "No File Found";
            }
            return res;
        }
    }
}
