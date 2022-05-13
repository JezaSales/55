using System;
using System.Collections.Generic;
using System.Linq;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region Affilieation       

        [HttpPost]
        [Route("/edit-AffiliateVendor")]
        public IActionResult EditAffiliateVendor(int id)
        {
            IEnumerable<AffiliateVendors> res = new List<AffiliateVendors>();
            if (id > 0)
            {
                IAffiliationML ml = new AffiliationML(_accessor, _env);
                res = ml.GetAffiliatedVendors(id);
            }
            return PartialView("Affiliates/Partial/_AddAffiliateVendor", res?.FirstOrDefault() ?? new AffiliateVendors());
        }

        [HttpPost("/update-AffiliateVendor")]
        public IActionResult UpdateAffiliateVendor(string VendorName, int Id, bool IsActive, IFormFile vIcon, IFormFile vBanner, string VendorBanner)
        {
            if (string.IsNullOrEmpty(VendorBanner))
            {
                VendorBanner = $"{DateTime.Now.ToString("ddMMyyyyhhmmss")}.png";
            }
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.SaveAffiliateVendor(VendorName, VendorBanner, Id, IsActive);
            if (res.Statuscode == ErrorCodes.One)
            {
                var _ = AppUtility.O.UploadFile(new FileUploadModel
                {
                    file = vIcon,
                    FileName = $"{res.CommonInt}.png",
                    FilePath = DOCType.AffiliateVendorIconPath
                });

                AppUtility.O.UploadFile(new FileUploadModel
                {
                    file = vBanner,
                    FileName = VendorBanner,
                    FilePath = DOCType.AffiliateItemPath.Replace("{0}", res.CommonInt.ToString())
                });
            }
            return Json(res);
        }


        [HttpPost]
        [Route("/AffiliateCategory")]
        public IActionResult _AffiliateCategory()
        {
            return PartialView("Affiliates/Partial/_AffiliateCategory");
        }

        [HttpGet]
        [Route("/AffiliateVendor")]
        public IActionResult AffiliateVendor()
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            return View("Affiliates/AffiliateVendor");
        }

        [HttpPost]
        [Route("AffiliatedVendors")]
        public IActionResult AffiliatedVendors()
        {
            IAffiliationML opml = new AffiliationML(_accessor, _env);
            var list = opml.GetAffiliatedVendors(0);
            return PartialView("Affiliates/Partial/_AffiliateVendors", list);
        }

        [HttpPost]
        [Route("AffiliatedVendorCommission")]
        public IActionResult AffiliatedVendorCommission(int VendorId)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetAfCategoryCommission(VendorId);
            return PartialView("Affiliates/Partial/_AffiliatedVendorCommission", res);

        }

        [HttpPost]
        [Route("/SaveAfVendorCateComm")]
        public IActionResult SaveAfVendorCateComm(int OID, int VendorID, decimal comm, int AmtType)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.SaveAfVendorCateComm(OID, VendorID, comm, AmtType);
            return Json(res);
        }

        [HttpPost("AffiliatedItem")]
        public IActionResult _AffiliatedItem(int VendorID, int Id)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var operators = ml.GetOperatorsByService("AFS");
            IAffiliationML aml = new AffiliationML(_accessor, _env);
            var res = aml.GetAfItemsById(Id, VendorID);
            if (res.VendorID == 0)
            {
                res.VendorID = VendorID;
            }
            res.Operator = operators;
            return PartialView("Affiliates/Partial/_AffiliatedItem", res);
        }


        //[HttpPost]
        //[Route("uploadAfItem")]
        //public IActionResult uploadAfItem(IFormFile file, int VendorID)
        //{
        //    IResourceML _bannerML = new ResourceML(_accessor, _env);
        //    var _res = _bannerML.uploadAfItem(file, VendorID, _lr);
        //    return Json(_res);
        //}

        //[HttpPost]
        //[Route("/AddAffiliatedItems")]
        //public IActionResult AddAffiliatedItems(int VendorID, string Link, string ImgUrl, bool IsActive, int ID, int LinkType, int OID, bool IsDel, bool IsImageURL, string Tittle)
        //{
        //    IAffiliationML ml = new AffiliationML(_accessor, _env);
        //    var res = ml.AddAffiliatedItems(VendorID, Link, ImgUrl, IsActive, ID, LinkType, OID, IsDel, IsImageURL, Tittle);
        //    return Json(res);
        //}

        [HttpPost("/AddAffiliatedItems")]
        public IActionResult AddAffiliatedItems(AffiliatedItem affiliatedItem, IFormFile file)
        {
            IResponseStatus response = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
            };
            string imgFullPath = affiliatedItem.ImageURL;
            if (file != null)
            {
                string imgPath = DOCType.AffiliateItemPathSuffix.Replace("{0}", affiliatedItem.VendorID.ToString());
                string imgName = $"Img_{DateTime.Now.ToString("ddMMyyyhhmmss")}.png";
                imgFullPath = imgPath + imgName;
                response = AppUtility.O.UploadFile(new FileUploadModel
                {
                    file = file,
                    FilePath = imgPath,
                    FileName = imgName
                });
            }
            if (response.Statuscode == ErrorCodes.One)
            {
                affiliatedItem.ImageURL = imgFullPath;
                affiliatedItem.ImgUrl = imgFullPath;
                IAffiliationML ml = new AffiliationML(_accessor, _env);
                response = ml.AddAffiliatedItems(affiliatedItem);
            }
            return Json(response);
        }




        [HttpPost]
        [Route("GetAllAfItems")]
        public IActionResult _GetAllAfItems(int VendorID)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetAllAfItems(VendorID, _lr.UserID, _lr.LoginTypeID);
            return PartialView("Affiliates/Partial/_GetAllAfItems", res);
        }

        [HttpPost]
        [Route("Af-Slab-Detail")]
        public IActionResult _ASlabDetail(int SlabID, bool IsAdminDefined)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetASlabDetail(SlabID, IsAdminDefined);
            if (!IsAdminDefined)
            {
                IOperatorML oml = new OperatorML(_accessor, _env);
                res.Operators = oml.GetOperatorsByService("AFS");
                return PartialView("Affiliates/Partial/_AfSlabDetail", res);
            }

            else
            {
                return PartialView("Affiliates/Partial/_AfCategoryWithRole", res);
            }

        }

        [HttpPost]
        [Route("Af-Slab-DetailRole")]
        public IActionResult _ASlabDetailRole(int SlabID, int RoleID, int OID)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetASlabDetailRole(SlabID, RoleID, OID);
            return PartialView("Affiliates/Partial/_AfSlabDetailRole", res);
        }

        [HttpPost]
        [Route("/UpdateAfSlabComm")]
        public IActionResult UpdateAfSlabComm(ASlabDetail req)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.UpdateAfSlabComm(req);
            return Json(res);
        }
        #endregion          
    }
}
