using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.MiddleLayer;
using System.Text;
using Validators;
using Newtonsoft.Json;
using System.Net;
using Fintech.AppCode.Configuration;
using Microsoft.Extensions.Primitives;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;

namespace RoundpayFinTech.Controllers
{
    public partial class PGCallbackController : Controller
    {
        [Route("PGCallback/AXISCDMCALLBACK")]
        public async Task<IActionResult> AXISBank()
        {
            StringBuilder resp = new StringBuilder();
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
            };
            try
            {
                if (request.Method == "POST")
                {
                    if (request.HasFormContentType)
                    {
                        if (request.Form.Keys.Count > 0)
                        {
                            foreach (var item in request.Form.Keys)
                            {
                                request.Form.TryGetValue(item, out StringValues strVal);
                                if (resp.Length == 0)
                                {
                                    resp.AppendFormat("{0}={1}", item, strVal);
                                }
                                else
                                {
                                    resp.AppendFormat("&{0}={1}", item, strVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        resp = new StringBuilder(await request.GetRawBodyStringAsync().ConfigureAwait(false));

                    }
                }
                else
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
                if (resp.Length == 0)
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
            }
            catch (Exception ex)
            {
                resp = new StringBuilder(ex.Message);
            }
            callbackAPIReq.Content = WebUtility.UrlDecode(resp.ToString());
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);
            var res = new ICICIModelResp
            {
                CODE = "06",
                SuccessANDRejected = "Rejected"
            };
            if ((callbackAPIReq.Content ?? string.Empty).Contains("UTR") && Is)
            {
                if (Validate.O.ValidateJSON(callbackAPIReq.Content))
                {
                    try
                    {
                        var req = JsonConvert.DeserializeObject<AxisBankResp>(callbackAPIReq.Content);
                        IciciML iciciML = new IciciML(_accessor, _env, false);
                        res = iciciML.ValidateAxisData(req);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return Json(res);
        }

        [Route("PGCallback/AxisIcollectValidation")]
        public async Task<IActionResult> AxisBankICOLLECTValidation()
        {
            StringBuilder resp = new StringBuilder();
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
            };
            try
            {
                if (request.Method == "POST")
                {
                    if (request.HasFormContentType)
                    {
                        if (request.Form.Keys.Count > 0)
                        {
                            foreach (var item in request.Form.Keys)
                            {
                                request.Form.TryGetValue(item, out StringValues strVal);
                                if (resp.Length == 0)
                                {
                                    resp.AppendFormat("{0}={1}", item, strVal);
                                }
                                else
                                {
                                    resp.AppendFormat("&{0}={1}", item, strVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        resp = new StringBuilder(await request.GetRawBodyStringAsync().ConfigureAwait(false));

                    }
                }
                else
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
                if (resp.Length == 0)
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
            }
            catch (Exception ex)
            {
                resp = new StringBuilder(ex.Message);
            }
            callbackAPIReq.Content = WebUtility.UrlDecode(resp.ToString());
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);
            var respToBank = new AxixBankICollectResp
            {
                Message = "Authentication failed",
                Err_cd = "003",
                Stts_flg = "F"
            };
            if (callbackAPIReq.Content.Contains("Bene_acc_no"))
            {
                var valAPIResp = JsonConvert.DeserializeObject<AxisBankICollectReq>(callbackAPIReq.Content);
                if (valAPIResp != null)
                {
                    IciciML iciciML = new IciciML(_accessor, _env, false);
                    var valResp = iciciML.ValidateAxisBankRequest(valAPIResp);
                    if (valResp.Statuscode == ErrorCodes.One)
                    {
                        respToBank.Message = "Success";
                        respToBank.Stts_flg = "S";
                        respToBank.Err_cd = "000";
                    }
                }
            }
            return Json(respToBank);
        }


        [Route("PGCallback/AxisIcollectNotification")]
        public async Task<IActionResult> AxisBankICOLLECTNotification()
        {
            StringBuilder resp = new StringBuilder();
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path,
                InActiveMode = false
            };
            try
            {
                if (request.Method == "POST")
                {
                    if (request.HasFormContentType)
                    {
                        if (request.Form.Keys.Count > 0)
                        {
                            foreach (var item in request.Form.Keys)
                            {
                                request.Form.TryGetValue(item, out StringValues strVal);
                                if (resp.Length == 0)
                                {
                                    resp.AppendFormat("{0}={1}", item, strVal);
                                }
                                else
                                {
                                    resp.AppendFormat("&{0}={1}", item, strVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        resp = new StringBuilder(await request.GetRawBodyStringAsync().ConfigureAwait(false));

                    }
                }
                else
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
                if (resp.Length == 0)
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
            }
            catch (Exception ex)
            {
                resp = new StringBuilder(ex.Message);
            }
            callbackAPIReq.Content = WebUtility.UrlDecode(resp.ToString());
            var ml = new CallbackML(_accessor, _env);
            var Is = await ml.LogCallBackRequestBool(callbackAPIReq).ConfigureAwait(false);

            var respToBank = new AxixBankICollectResp
            {
                Message = "Authentication failed",
                Err_cd = "003",
                Stts_flg = "F"
            };
            if (callbackAPIReq.Content.Contains("Bene_acc_no"))
            {
                var valAPIResp = JsonConvert.DeserializeObject<AxisBankICollectReq>(callbackAPIReq.Content);
                if (valAPIResp != null)
                {
                    IciciML iciciML = new IciciML(_accessor, _env, false);
                    var valResp = iciciML.NotifyAxisBankICollect(valAPIResp);
                    if (valResp.Statuscode == ErrorCodes.One)
                    {
                        respToBank.Message = "Success";
                        respToBank.Stts_flg = "S";
                        respToBank.Err_cd = "000";
                    }
                }
            }
            return Json(respToBank);
        }

        [Route("PGCallback/Hash")]
        public IActionResult CheckHash(string ch)
        {
            return Ok(Fintech.AppCode.HelperClass.HashEncryption.O.MD5Hash(ch));
        }
    }
}
