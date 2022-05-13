using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Lookup;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.FintechVASAPI
{
    public class FintechVASAPI
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly FNTHVASAPISetting fNTHVASAPISetting;
        private readonly IDAL _dal;
        private readonly string _APICode;
        private static string _JWTTOKEN = string.Empty;

        public FintechVASAPI(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _APICode = APICode;
            fNTHVASAPISetting = AppSetting();
        }

        private FNTHVASAPISetting AppSetting()
        {
            var setting = new FNTHVASAPISetting();
            try
            {
                setting = new FNTHVASAPISetting
                {
                    UserID = Configuration[$"VAS:{_APICode}:UserID"],
                    Token = Configuration[$"VAS:{_APICode}:Token"],
                    BaseURL = Configuration[$"VAS:{_APICode}:BaseURL"],
                    PlanURL = Configuration[$"VAS:{_APICode}:PlanURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FNTHVASAPISetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        private void GenerateJWTToken()
        {
            var req = new
            {
                UserID = fNTHVASAPISetting.UserID,
                UserToken = fNTHVASAPISetting.Token
            };
            string response = string.Empty;
            var URL = fNTHVASAPISetting.BaseURL + "userauth/getToken";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWR(URL, req);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<VASAPITokenResp>(response);
                    if (_apiRes.statusCode == ErrorCodes.One)
                    {
                        _JWTTOKEN = _apiRes.token;
                    }
                }
            }
            catch (Exception ex)
            {
                response = string.Concat(ex.Message, " | ", response);
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateJWTToken",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcLogPlansAPIReqResp(_dal).Call(new CommonReq
            {
                CommonStr = "GenerateJWTToken",
                CommonStr2 = URL,
                CommonStr3 = response
            });
        }
        private string HitVASApi(string MethodName,string _URL,int ctr=0 )
        {
            string _response = string.Empty;
            if (string.IsNullOrEmpty(_JWTTOKEN))
                GenerateJWTToken();
            var header = new Dictionary<string, string> {
                { "Authorization" , "Bearer " + _JWTTOKEN }
            };
            try
            {
                _response = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, header).Result;
                if (_response.Contains("Unauthorized") && ctr < 2)
                {
                    ctr++;
                    GenerateJWTToken();
                    HitVASApi(MethodName, _URL, ctr);
                }
            }
            catch (Exception ex)
            {
                _response = string.Concat(ex.Message, " | ", _response);
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = MethodName,
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
                return string.Empty;
            }
            new ProcLogPlansAPIReqResp(_dal).Call(new CommonReq
            {
                CommonStr = MethodName,
                CommonStr2 = _URL,
                CommonStr3 = _response
            });
            return _response;
        }
        public RNPRoffer VASBestOffer(string accountNo,string spKey)
        {
            var resp = new RNPRoffer
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };                  
            var URL = fNTHVASAPISetting.PlanURL + $"BestOffer?accountNo={accountNo}&spkey={spKey}";
            try
            {
                var response = HitVASApi("VASBestOffer",URL,0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<RNPRoffer>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        resp = _apiRes.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASBestOffer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }
        public RNPDTHCustInfo VASDTHCustomerInfo(string accountNo, string spKey)
        {
            var resp = new RNPDTHCustInfo
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var URL = fNTHVASAPISetting.PlanURL + $"DTHCustomerInfo?accountNo={accountNo}&spkey={spKey}";
            try
            {
                var response = HitVASApi("VASDTHCustomerInfo", URL, 0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<RNPDTHCustInfo>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        resp = _apiRes.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASDTHCustomerInfo",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }
        public RNPDTHHeavyRefresh VASDTHHeavyRefresh(string accountNo, string spKey)
        {
            var resp = new RNPDTHHeavyRefresh
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var URL = fNTHVASAPISetting.PlanURL + $"DTHHeavyRefresh?accountNo={accountNo}&spkey={spKey}";
            try
            {
                var response = HitVASApi("VASDTHHeavyRefresh", URL, 0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<RNPDTHHeavyRefresh>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        resp = _apiRes.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASDTHHeavyRefresh",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }
        public PSRechPResp VASRechargePlan(string circleId, string spKey)
        {
            var resp = new PSRechPResp
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var URL = fNTHVASAPISetting.PlanURL + $"RechargePlan?circleId={circleId}&spkey={spKey}";
            try
            {
                var response = HitVASApi("VASRechargePlan", URL, 0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<PSRechPResp>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        resp = _apiRes.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASRechargePlan",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }
        public PSRechPResp VASDTHPlan(string spKey)
        {
            var resp = new PSRechPResp
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var URL = fNTHVASAPISetting.PlanURL + $"DTHPlan?spkey={spKey}";
            try
            {
                var response = HitVASApi("VASDTHPlan", URL, 0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<PSRechPResp>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        resp = _apiRes.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASDTHPlan",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }
        public HLRResponseStatus VASMobileLookup(string accountNo, string spKey, int ApiId, int UserId)
        {
            var resp = new HLRResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var URL = fNTHVASAPISetting.PlanURL + $"MobileLookup?accountNo={accountNo}&spkey={spKey}";
            try
            {
                var response = HitVASApi("VASMobileLookup", URL, 0);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<PlansRespModel<PSHLRResponse>>(response);
                    if (_apiRes.StatusCode == ErrorCodes.One)
                    {
                        string MobileNo = "";
                        string CurrentCircle = "";
                        string CurrentOperator = "";
                        if (_apiRes.Data != null)
                        {
                            if (_apiRes.Data.StatusCode == ErrorCodes.One)
                            {
                                MobileNo = accountNo; 
                                CurrentCircle = _apiRes.Data.Circle;
                                CurrentOperator = _apiRes.Data.Operator;
                                resp.Statuscode = ErrorCodes.One;
                                resp.Msg = ErrorCodes.SUCCESS;
                            }
                        }
                        IProcedure proc = new ProcLookUpAPIReqResp(_dal);
                        resp = (HLRResponseStatus)proc.Call(new LookUpDBLogReq
                        {
                            APIID = ApiId,
                            APIType = LookupAPIType.VASFintechAPI,
                            LoginID = UserId,
                            Mobile = accountNo,
                            Request = URL,
                            CurrentCircle = CurrentCircle,
                            CurrentOperator = CurrentOperator,
                            Response = response
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VASMobileLookup",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return resp;
        }

    }
}
