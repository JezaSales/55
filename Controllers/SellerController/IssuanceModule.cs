using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace RoundpayFinTech.Controllers
{
    public partial class SellerController
    {
        [HttpPost]
        [Route("CheckIssuance")]
        public IActionResult CheckIssuance(string MobileNo, int o)
        {
            IIssuanceML issuanceML = new IssuanceML(_accessor, _env);
            var res = issuanceML.GenerateOTP(new AppCode.Model.Issuance.IssuanceOTPRequest
            {
                OID = o,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                MobileNo = MobileNo
            });
            return Json(res);
        }
        [HttpPost]
        [Route("VerifyIssuance")]
        public IActionResult VerifyIssuance(string MobileNo, int o,string otp,string refid)
        {
            IIssuanceML issuanceML = new IssuanceML(_accessor, _env);
            var res = issuanceML.VerifyOTP(new AppCode.Model.Issuance.IssuanceOTPRequest
            {
                OID = o,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                MobileNo = MobileNo,
                OTP=otp,
                ReffrenceID=refid
            });
            return Json(res);
        }
    }
}
