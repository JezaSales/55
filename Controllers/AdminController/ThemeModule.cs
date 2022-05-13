using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        [HttpGet]
        [Route("Theme")]
        public IActionResult Theme()
        {
            //if (!ApplicationSetting.IsWhitelabel)
            //    return Ok();
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var res = new TemplatesAndThemes
            {
                Themes = ml.GetThemes(),
                SiteTemplates = GetSiteTemplates(),
                isOnlyForRP = _accessor.HttpContext.Request.Host.ToString().Contains("Roundpay.net")
            };
            return View("Theme/Theme", res);
        }

        [HttpPost]
        [Route("ChangeTheme")]
        public IActionResult ChangeTheme(int ThemeId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.ChangeTheme(ThemeId);
            return Json(response);
        }

        [HttpPost]
        [Route("WLAllowTheme")]
        public IActionResult WLAllowTheme(int ThemeId, bool IsWLAllowed)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.WLAllowTheme(ThemeId, IsWLAllowed);
            return Json(response);
        }

        public IActionResult ChangeColorSet(int ColorSetId, int ThemeId = 0)
        {
            var res = ColorSets.UpdateThemColor(ThemeId, ColorSetId, _lr.WID);
            var response = new ResponseStatus
            {
                Statuscode = res ? ErrorCodes.One : ErrorCodes.Minus1,
                Msg = res ? "Coler set applied successfully" : ErrorCodes.TempError
            };
            return Json(response);
        }

        [HttpPost]
        public IActionResult ChangeSiteTemplate(int TemplateId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.ChangeSiteTemplate(_lr.UserID, TemplateId, _lr.WID);
            return Json(response);
        }

        [HttpPost]
        public IActionResult WebsitecontentEditor(int contentId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            //var response = ml.ChangeSiteTemplate(_lr.UserID, TemplateId, _lr.WID);
            return PartialView("Theme/WebsitecontentEditor");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWebsiteContentAsync(string section, string content)
        {
            if (section.Equals("slider", StringComparison.OrdinalIgnoreCase))
            {
                content = content.Replace("src=\"Image", "src=\"/Image");
            }
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = await ml.UpdateWebsiteContentAsync(new CommonReq { LoginID = _lr.UserID, CommonInt = _lr.WID, CommonStr = section, CommonStr2 = content }).ConfigureAwait(true);
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetWebsiteContectAsync()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = await ml.GetWebsiteContentAsync(_lr.WID).ConfigureAwait(true);
            return Json(response);
        }

        private IEnumerable<SiteTemplate> GetSiteTemplates()
        {
            var res = new List<SiteTemplate>();
            try
            {
                var path = DOCType.SiteconfigJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    res = JsonConvert.DeserializeObject<List<SiteTemplate>>(jsonData);
                }
            }
            catch (Exception ex)
            {

            }
            return res ?? new List<SiteTemplate>();
        }

        [Route("/EditWebsiteTemplate/{id}")]
        public IActionResult EditWebsiteTemplate(int id = 0)
        {
            string templateId = id.ToString();
            var isAdmin = _lr.RoleID == Role.Admin || _lr.RoleID == Role.MasterWL;
            string path = DOCType.websiteTemplateContent.Replace("{templateId}", templateId, StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", _lr.WID.ToString()), StringComparison.OrdinalIgnoreCase);
            string jsonFile = string.Concat(path, "content.json");
            var template = new PublishTemplateModel();
            if (System.IO.File.Exists(jsonFile))
            {
                var json = System.IO.File.ReadAllText(jsonFile);
                template = JsonConvert.DeserializeObject<PublishTemplateModel>(json) ?? new PublishTemplateModel();
            }
            string content = isAdmin ? template.Template : template.Published;
            return View("~/views/home/template/1.cshtml", new PublishTemplateViewModel { IsAdmin = true, Content = content });
        }

        public IActionResult WebsiteTemplateContent(int templateId = 1, bool isEditable = false)
        {
            string path = DOCType.websiteTemplateContent.Replace("{templateId}", templateId.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", _lr.WID.ToString()), StringComparison.OrdinalIgnoreCase);
            string jsonFile = string.Concat(path, "content.json");
            var json = System.IO.File.ReadAllText(jsonFile);
            var template = JsonConvert.DeserializeObject<PublishTemplateModel>(json);
            return Json(new { template = template, isEditable = isEditable });
        }

        public IActionResult UploadtemplateImages(List<IFormFile> files, string section, int templateId)
        {
            List<string> imgSrc = new List<string>();
            int i = 0;
            string filePath = DOCType.websiteTemplateImages.Replace("{templateId}", templateId.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", _lr.WID.ToString()), StringComparison.OrdinalIgnoreCase);
            foreach (var item in files)
            {
                i++;
                string fileName = string.Concat(section, "_", i.ToString(), ".png");
                var fileres = AppUtility.O.UploadFile(new FileUploadModel
                {
                    FilePath = filePath,
                    file = item,
                    FileName = fileName
                });
                imgSrc.Add(fileName);
            }
            var res = new ResponseStatus { Statuscode = ErrorCodes.One, Msg = ErrorCodes.SUCCESS };
            return Json(new { response = res, images = imgSrc });//imgSrc
        }

        public IActionResult PublishTemplate(PublishTemplateModel publishTemplate, int templateId = 0)
        {
            var response = AppUtility.O.CreateJsonFile(new CreateJsonFileModel
            {
                Path = DOCType.websiteTemplateContent.Replace("{templateId}", templateId.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", _lr.WID.ToString()), StringComparison.OrdinalIgnoreCase),
                FileName = "content.json",
                Json = JsonConvert.SerializeObject(publishTemplate)
            });
            return Json(response);//imgSrc
        }
    }
}