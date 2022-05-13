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
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ICollect;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.ThirdParty.CashFree
{
    public class CashfreeCollectML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly CashFreeAppSetting appSetting;
        private readonly IDAL _dal;
        private string _Token;
        private string _TokenCollect;
        private string _APIGroupCode;
        private string _CFBeneIDInCaseofThanos;
        private IErrorCodeML errorCodeML;

        public CashfreeCollectML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APIGroupCode)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            appSetting = AppSetting();
            _APIGroupCode = APIGroupCode;
            errorCodeML = new ErrorCodeML(_accessor, _env, false);
        }
        private CashFreeAppSetting AppSetting()
        {
            var setting = new CashFreeAppSetting();
            try
            {
                setting = new CashFreeAppSetting
                {
                    CollectHost = Configuration["SMARTCOLLECT:CASHFREE:CollectHost"],
                    VirtualAccountURL = Configuration["SMARTCOLLECT:CASHFREE:VirtualAccountURL"],
                    CollectClientID = Configuration["SMARTCOLLECT:CASHFREE:CollectClientID"],
                    CollectSecretKey = Configuration["SMARTCOLLECT:CASHFREE:CollectSecretKey"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CashFreeAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private void CACAuthorization()
        {
            string _request = string.Empty, _response = string.Empty;

            var headers = new Dictionary<string, string>
            {
                { "X-Client-Id", appSetting.CollectClientID },
                { "X-Client-Secret", appSetting.CollectSecretKey }
            };
            _request = appSetting.CollectHost + "cac/v1/authorize";
            var apiResp = AppWebRequest.O.HWRPost(_request, "", headers);
            _response = apiResp;
            if (!string.IsNullOrEmpty(apiResp))
            {
                try
                {
                    var res = JsonConvert.DeserializeObject<CFTokenGen>(apiResp);
                    if (res != null)
                    {
                        if (res.data != null)
                            _TokenCollect = res.data.token;
                    }
                }
                catch (Exception ex)
                {
                    _response = "Exception:" + ex.Message + "||" + _response;
                }
            }
            if (!string.IsNullOrEmpty(_request) || !string.IsNullOrEmpty(_response))
            {
                IProcedure proc = new ProcLogAPITokenGeneration(_dal);
                proc.Call(new CommonReq
                {
                    str = APICode.CASHFREE,
                    CommonStr = _request,
                    CommonStr2 = _response
                });
            }
        }
        public ICollectStatusResponse StatusCheckICollect(string RefID)
        {
            ICollectStatusResponse collectStatusResponse = new ICollectStatusResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "No Response"
            };
            CACAuthorization();
            StringBuilder _URL = new StringBuilder(appSetting.CollectHost);
            _URL.Append("cac/v1/fetchPaymentById/");
            _URL.Append(RefID ?? string.Empty);
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _TokenCollect}
            };

            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL.ToString(), headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CashfreeCollectStatuscheckresp>(resp);
                    if (apiResp.status.Equals("SUCCESS"))
                    {
                        collectStatusResponse.Statuscode = ErrorCodes.One;
                        collectStatusResponse.Msg = apiResp.message;
                        if (apiResp.data != null)
                        {
                            if (apiResp.data.payment != null)
                            {
                                collectStatusResponse.Amount = apiResp.data.payment.amount;
                                collectStatusResponse.UTR = apiResp.data.payment.utr;
                            }
                        }
                    }
                    else
                    {
                        collectStatusResponse.Msg = apiResp.message;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckICollect",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = 0,
                Method = "StatusCheckICollect",
                RequestModeID = 0,
                Request = _URL.ToString() + JsonConvert.SerializeObject(headers),
                Response = resp ?? string.Empty
            });
            return collectStatusResponse;
        }
        public SmartCollectCreateCustomerResponse CreateVirtualAccount(SmartCollectCreateCustomerRequest createCustomerRequest)
        {
            var resp = new SmartCollectCreateCustomerResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Customer can not be created",
                collectAccountDetails = new List<SmartCollectAccountDetail>()
            };
            CACAuthorization();

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _TokenCollect}
            };
            var request = new CashfreeCreateVAAccountRequest
            {
                vAccountId = createCustomerRequest.Contact,
                name = createCustomerRequest.Name,
                email = createCustomerRequest.EmailID,
                phone = createCustomerRequest.Contact,
                notifGroup = "DEFAULT",
                createMultiple = 0
            };
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.VirtualAccountURL, request, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CashfreeCreateAccountResp>(response);
                    if (apiResp.status.ToUpper().Equals("SUCCESS"))
                    {
                        resp.Statuscode = ErrorCodes.One;
                        resp.Msg = apiResp.message;
                        if (apiResp.data != null)
                        {
                            if (apiResp.data.YESB != null)
                            {
                                resp.collectAccountDetails.Add(new SmartCollectAccountDetail
                                {
                                    BankCode = nameof(apiResp.data.YESB),
                                    AccountNumber = apiResp.data.YESB.accountNumber,
                                    IFSC = apiResp.data.YESB.ifsc
                                });
                            }

                            if (apiResp.data.ICIC != null)
                            {
                                resp.collectAccountDetails.Add(new SmartCollectAccountDetail
                                {
                                    BankCode = nameof(apiResp.data.ICIC),
                                    AccountNumber = apiResp.data.ICIC.accountNumber,
                                    IFSC = apiResp.data.ICIC.ifsc
                                });
                            }

                            if (apiResp.data.IDFC != null)
                            {
                                resp.collectAccountDetails.Add(new SmartCollectAccountDetail
                                {
                                    BankCode = nameof(apiResp.data.IDFC),
                                    AccountNumber = apiResp.data.IDFC.accountNumber,
                                    IFSC = apiResp.data.IDFC.ifsc
                                });
                            }
                        }
                    }
                    else
                    {
                        if (apiResp.subCode == "409")
                        {
                            resp = RecoverVirtualAccountOrVPA(createCustomerRequest);
                        }
                        else
                        {
                            resp.Msg = apiResp.message;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateVirtualAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = 0,
                Method = "CreateVirtualAccount",
                RequestModeID = 0,
                Request = appSetting.VirtualAccountURL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(request),
                Response = response ?? string.Empty
            });
            return resp;
        }
        public SmartCollectCreateCustomerResponse CreateVPA(SmartCollectCreateCustomerRequest createCustomerRequest)
        {
            var resp = new SmartCollectCreateCustomerResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Customer can not be created",
                collectAccountDetails = new List<SmartCollectAccountDetail>()
            };
            CACAuthorization();

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _TokenCollect}
            };
            var request = new CashfreeCreateVA_VPARequest
            {
                virtualVpaId = "VPA"+createCustomerRequest.UserID.ToString(),
                name = createCustomerRequest.Name,
                email = createCustomerRequest.EmailID,
                phone = createCustomerRequest.Contact,
                notifGroup = "DEFAULT"
            };
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.VirtualAccountURL, request, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CashfreeCreateAccountResp>(response);
                    if (apiResp.status.ToUpper().Equals("SUCCESS"))
                    {
                        resp.Statuscode = ErrorCodes.One;
                        resp.Msg = apiResp.message;
                        if (apiResp.data != null)
                        {
                            resp.VPA = apiResp.data.vpa;
                        }
                    }
                    else
                    {
                        if (apiResp.subCode == "409")
                        {
                            createCustomerRequest.IsVPA = true;
                            resp = RecoverVirtualAccountOrVPA(createCustomerRequest);
                        }
                        else
                        {
                            resp.Msg = apiResp.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateVirtualAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = 0,
                Method = "CreateVirtualAccount",
                RequestModeID = 0,
                Request = appSetting.VirtualAccountURL + JsonConvert.SerializeObject(headers) + JsonConvert.SerializeObject(request),
                Response = response ?? string.Empty
            });
            return resp;
        }

        public SmartCollectCreateCustomerResponse RecoverVirtualAccountOrVPA(SmartCollectCreateCustomerRequest createCustomerRequest)
        {
            var resp = new SmartCollectCreateCustomerResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Customer can not be created",
                collectAccountDetails = new List<SmartCollectAccountDetail>()
            };
            CACAuthorization();

            var headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + _TokenCollect}
            };

            string response = string.Empty;
            string _URL = appSetting.CollectHost + "cac/v1/va/" + (createCustomerRequest.IsVPA == false ? createCustomerRequest.Contact : createCustomerRequest.UserID.ToString());
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GETAsync(_URL, headers).Result;
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<CashfreeCreateAccountResp>(response);
                    if (apiResp.status.ToUpper().Equals("SUCCESS"))
                    {
                        resp.Statuscode = ErrorCodes.One;
                        resp.Msg = apiResp.message;
                        if (apiResp.data != null)
                        {
                            if (createCustomerRequest.IsVPA == false)
                            {
                                resp.collectAccountDetails.Add(new SmartCollectAccountDetail
                                {
                                    BankCode = apiResp.data.ifsc.Substring(0, 4).ToUpper(),
                                    AccountNumber = apiResp.data.virtualAccountNumber,
                                    IFSC = apiResp.data.ifsc
                                });
                            }
                            else
                            {
                                resp.VPA = apiResp.data.virtualVPA;
                            }
                        }
                    }
                    else
                    {
                        resp.Msg = apiResp.message;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RecoverVirtualAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = 0,
                Method = "RecoverVirtualAccount",
                RequestModeID = 0,
                Request = _URL + "" + JsonConvert.SerializeObject(headers),
                Response = response ?? string.Empty
            });
            return resp;
        }
    }
}
