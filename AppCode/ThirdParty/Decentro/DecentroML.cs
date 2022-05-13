using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class DecentroML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly DecentroAppSetting appSetting;
        private readonly IDAL _dal;
        public DecentroML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string apiCode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting(apiCode);
        }
        private DecentroAppSetting AppSetting(string apiCode)
        {
            var setting = new DecentroAppSetting();
            try
            {
                setting = new DecentroAppSetting
                {
                    BaseURL = Configuration["DMR:" + apiCode + ":BaseURL"],
                    client_id = Configuration["DMR:" + apiCode + ":client_id"],
                    client_secret = Configuration["DMR:" + apiCode + ":client_secret"],
                    module_secret = Configuration["DMR:" + apiCode + ":module_secret"],
                    provider_secret = Configuration["DMR:" + apiCode + ":provider_secret"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DecentroAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public DMRTransactionResponse DoUPIVerification(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            string resp = string.Empty;
            string URL = appSetting.BaseURL + "v2/payments/vpa/validate";
            var headers = new Dictionary<string, string>
            {
               { "client_id", appSetting.client_id },
               { "client_secret", appSetting.client_secret },
               { "module_secret", appSetting.module_secret },
               { "provider_secret", appSetting.provider_secret }
            };
            var dataRequest = new
            {
                reference_id = request.TID.ToString(),
                upi_id = request.mBeneDetail.AccountNo,
                type = request.APIOpCode
            };
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRTLS(URL, dataRequest, headers).Result;
                if (!string.IsNullOrEmpty(resp))
                {
                    if (resp.Contains("FAILURE"))
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = ErrorCodes.FAILED;
                        res.ErrorCode = ErrorCodes.Transaction_Failed_Replace;
                    }
                    else
                    {
                        var apiResp = JsonConvert.DeserializeObject<DecentroVPAResp>(resp);
                        if (apiResp.status.Equals("SUCCESS"))
                        {
                            res.AccountHolder = apiResp.data.name ?? string.Empty;
                            res.LiveID = apiResp.decentroTxnId;
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            res.Request = URL + "?" + JsonConvert.SerializeObject(headers) + "|" + JsonConvert.SerializeObject(dataRequest);
            res.Response = resp;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "DoUPIVerification",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
    }
}