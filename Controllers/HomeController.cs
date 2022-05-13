using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Classes;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly ILoginML loginML;
        private readonly WebsiteInfo _WInfo;
        public HomeController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _session = _accessor.HttpContext.Session;
            loginML = new LoginML(_accessor, _env);
            _WInfo = loginML.GetWebsiteInfo();
        }

        public IActionResult Index_old()
        {
            string url = string.Empty;
            url = _WInfo != null ? "~/views/home/Themes/" + _WInfo.ThemeId + "/index.cshtml" : "~/views/login/index.cshtml";
            ViewData["Theme"] = _WInfo.ThemeId;
            var req = new CommonReq
            {
                CommonInt = _WInfo.WID,
                CommonInt2 = _WInfo.ThemeId
            };
            IProcedure proc = new ProcGetDisplayHtml(_dal);
            var response = (HomeDisplay)proc.Call(req);
            return View(url, response);
            //if (_WInfo != null)
            //{
            //    {
            //        if (_WInfo.WID != 1)
            //        {
            //            return RedirectToAction("Index", "Login");
            //        }
            //    }
            //    return Ok();
            //}
        }

        public IActionResult Index()
        {
            if (_WInfo.WID != 1)
            {
                int templateId = _WInfo.SiteId;
                int WID = _WInfo.WID;
                string path = DOCType.websiteTemplateContent.Replace("{templateId}", templateId.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", WID.ToString()), StringComparison.OrdinalIgnoreCase);
                string jsonFile = string.Concat(path, "content.json");
                var template = new PublishTemplateModel();
                try
                {
                    if (System.IO.File.Exists(jsonFile))
                    {
                        var json = System.IO.File.ReadAllText(jsonFile);
                        template = JsonConvert.DeserializeObject<PublishTemplateModel>(json);
                    }
                }
                catch (Exception ex)
                {

                }
                string content = template?.Published ?? string.Empty;
                return View("~/views/home/template/1.cshtml", new PublishTemplateViewModel { IsAdmin = false, Content = content });
            }

            if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Corporate)
            {
                int templateId = _WInfo.SiteId;
                int WID = _WInfo.WID;
                string path = DOCType.websiteTemplateContent.Replace("{templateId}", templateId.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{wid}", string.Concat("WID_", WID.ToString()), StringComparison.OrdinalIgnoreCase);
                string jsonFile = string.Concat(path, "content.json");
                var template = new PublishTemplateModel();
                try
                {
                    if (System.IO.File.Exists(jsonFile))
                    {
                        var json = System.IO.File.ReadAllText(jsonFile);
                        template = JsonConvert.DeserializeObject<PublishTemplateModel>(json);
                    }
                }
                catch (Exception ex)
                {

                }
                string content = template?.Published ?? string.Empty;
                return View("~/views/home/template/1.cshtml", new PublishTemplateViewModel { IsAdmin = false, Content = content });
            }
            else if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Shopping)
            {
                return RedirectToAction("Index", "ShoppingWebsite");

            }
            else if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Travel)
            {
                return RedirectToAction("Index", "TravelWebiste");
            }

            else if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Disabled)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(new IndexViewModel { WID = _WInfo.WID, SiteId = _WInfo.SiteId, Assets = GetAssets(_WInfo.SiteId), Content = new SiteTemplateSection { } });
        }

        private IEnumerable<string> GetAssets(int siteId)
        {
            var assets = new List<string>();
            try
            {
                var path = DOCType.SiteconfigJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    var res = JsonConvert.DeserializeObject<List<SiteTemplate>>(jsonData);
                    assets = res.Where(x => x.TemplateId == siteId).FirstOrDefault()?.Assets.ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return assets ?? new List<string>();
        }

        [HttpGet("aboutus")]
        public IActionResult AboutUs()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("about-us");

        }
        [HttpGet("contactus")]
        public IActionResult ContactUs()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("contact-us");

        }
        [HttpGet("servicedetail")]
        public IActionResult ServiceDetail()
        {
            if (_WInfo != null)
            {
                if (_WInfo.WID != 1)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            return Ok();
            return View("service-detail");

        }
        public IActionResult Hit(string s, bool IsE)
        {
            if (IsE)
            {
                return Ok(HashEncryption.O.Encrypt(s));
            }
            else
            {
                return Ok(HashEncryption.O.Decrypt(s));
            }
            //return Ok(HashEncryption.O.AppEncrypt(s));
        }
        [HttpGet("privacy")]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        [HttpGet("t-c")]
        public IActionResult TermsAndCondition()
        {
            return View();
        }
        [HttpPost]
        [Route("/WebsitePopup")]
        public IActionResult WebsitePopUp()
        {
            var WebInfo = loginML.GetPopupInfo();
            if (WebInfo.ISWebSitePopup)
            {
                return PartialView("Partial/_websitePopup", WebInfo);
            }
            else
            {
                return Ok();
            }
        }
        [HttpPost]
        [Route("GetinTouch")]
        [Route("Home/GetinTouch")]
        public IActionResult UserSubscription([FromBody] GetIntouch LoginDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            ILoginML ml = new LoginML(_accessor, _env);
            var _resp = ml.UserSubscription(LoginDetail);
            _res.Statuscode = _resp.Statuscode;
            _res.Msg = _resp.Msg;
            return Json(_res);
        }

        [HttpGet]
        [Route("InviteApp/{id}")]
        public ActionResult InviteApp(string id)
        {
            bool _IsB2C = false;
            if (string.IsNullOrEmpty(id))
                id = "1";
            else if (id.Contains("B2B"))
                id = id.Replace("B2B", "");
            else if (id.Contains("B2C"))
            {
                id = id.Replace("B2C", "");
                _IsB2C = true;
            }
            ILoginML ml = new LoginML(_accessor, _env);
            WebsiteInfo data = ml.GetWebsitePackage(Convert.ToInt32(id), _IsB2C);
            if (data.AppPackageID == "")
            {
                return Redirect("http://" + data.WebsiteName + "/SignUp?rid=" + id);
            }
            return View(data);
        }

        public IActionResult IrctcCertificate()
        {

            return PartialView("~/views/report/PartialView/_IrctcCertficated.cshtml");
        }
        [HttpPost]
        [Route("GetWebsiteBankDetails")]
        public async Task<IActionResult> GetWebsiteBankDetails()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var Bank = await ml.GetWebsiteBankDetails();
            return Json(Bank);
        }


        [HttpPost(nameof(ReadTextFromImage))]
        public IActionResult ReadTextFromImage([FromForm] OcrModel request)
        {
            OCRHelper ocr = new OCRHelper();
            string result = ocr.ReadTextFromImage(request);
            return Json(result);
        }
    }
}
