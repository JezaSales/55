using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using paytm;
using Newtonsoft.Json;
using Fintech.AppCode.WebRequest;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using System.Text;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace RoundpayFinTech.AppCode.ThirdParty.Paytm
{
    public class DIKSHUPayML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly string _APICode;
        private readonly int _APIID;
        private readonly string _APIGroupCode;
        private readonly IDAL _dal;
        private readonly DiskshuPayAppSetting apiSetting;
        private IErrorCodeML errorCodeML;

        public DIKSHUPayML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode, int APIID, string APIGroupCode)
        {
            _APICode = APICode;
            _APIID = APIID;
            _accessor = accessor;
            _env = env;
            _APIGroupCode = APIGroupCode;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
            errorCodeML = new ErrorCodeML(_accessor, _env, false);
        }
        public DiskshuPayAppSetting AppSetting()
        {
            var setting = new DiskshuPayAppSetting();
            try
            {
                setting = new DiskshuPayAppSetting
                {
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    MobileNo = Configuration["DMR:" + _APICode + ":MobileNo"],
                    CustomerID = Configuration["DMR:" + _APICode + ":CustomerID"],
                    Token = Configuration["DMR:" + _APICode + ":Token"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DiskshuPayAppSetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }

        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = request.APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.SenderMobile = request.SenderMobile;
                    res.KYCStatus = SenderKYCStatus.ACTIVE;
                    res.SenderName = senderRequest.Name;
                    res.IsNotCheckLimit = senderRequest.IsNotCheckLimit;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {

            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = request.SenderMobile,
                    _Name = request.mBeneDetail.BeneName,
                    _AccountNumber = request.mBeneDetail.AccountNo,
                    _MobileNo = request.mBeneDetail.MobileNo,
                    _IFSC = request.mBeneDetail.IFSC,
                    _BankName = request.mBeneDetail.BankName,
                    _EntryBy = request.UserID,
                    _VerifyStatus = 1,
                    _APICode = request.APICode,
                    _BankID = request.mBeneDetail.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
                {
                    Name = request.FirstName + " " + request.LastName,
                    MobileNo = request.SenderMobile,
                    Pincode = request.Pincode.ToString(),
                    Address = request.Address,
                    City = request.City,
                    StateID = request.StateID,
                    AadharNo = request.AadharNo,
                    Dob = request.DOB,
                    UserID = request.UserID
                })) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.IsOTPGenerated = true;
                    res.OTP = dbres.OTP;
                    res.WID = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = "Internal",
                Response = JsonConvert.SerializeObject(res),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            return DMTAPIHelperML.GetBeneficiary(request, _dal, GetType().Name);
        }

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = request.SenderMobile,
                    CommonStr2 = request.OTP,
                    CommonInt = request.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var _res = (ResponseStatus)new ProcRemoveBeneficiaryNew(_dal).Call(new CommonReq
                {
                    LoginID = request.UserID,
                    CommonInt = Convert.ToInt32(request.mBeneDetail.BeneID),
                    CommonStr = request.SenderMobile
                });
                res.Statuscode = _res.Statuscode;
                res.Msg = _res.Msg;
                res.ErrorCode = _res.ErrorCode;
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };

            StringBuilder req = new StringBuilder("token={TOKEN}&MobileNo={MOBILE}&AccountNumber={ACCOUNT}&IFSCCode={IFSC}&transid={TID}&FirstName={FIRSTNAME}&LastName={LASTNAME}&Pincode={PINCODE}&BankName={BANK}");
            req.Replace("{TOKEN}", apiSetting.Token);
            req.Replace("{MOBILE}", apiSetting.MobileNo);
            req.Replace("{ACCOUNT}", request.mBeneDetail.AccountNo);
            req.Replace("{IFSC}", request.mBeneDetail.IFSC);
            req.Replace("{TID}", request.TID.ToString());
            req.Replace("{FIRSTNAME}", string.IsNullOrEmpty(request.mBeneDetail.BeneName) ? "Test" : request.FirstName);
            req.Replace("{LASTNAME}", string.IsNullOrEmpty(request.mBeneDetail.BeneName) ? "Test" : request.LastName);
            req.Replace("{PINCODE}", request.Pincode.ToString());
            req.Replace("{BANK}", request.mBeneDetail.BankName);

            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "V2/VerifyBeneficiary?" + req.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var apiResp = JsonConvert.DeserializeObject<DKPVerifyResp>(response);
                        if (apiResp != null)
                        {
                            if (apiResp.Status == "Success")
                            {
                                if (apiResp.Message.Contains("Successfully"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.BeneName = apiResp.Data.beneName;
                                    res.LiveID = apiResp.Data.referenceid;
                                    //res.VendorID = string.Empty;
                                }
                                else
                                {
                                    res.Statuscode = ErrorCodes.Minus1;
                                    res.Msg = nameof(ErrorCodes.FAILED);
                                    res.ErrorCode = ErrorCodes.Minus1;
                                }
                            }
                            else if (apiResp.Status == "Fail")
                            {
                                res.Statuscode = ErrorCodes.Minus1;
                                res.Msg = nameof(ErrorCodes.FAILED);
                                res.ErrorCode = ErrorCodes.Minus1;
                            }
                            else
                            {
                                errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, apiResp.Message);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", apiResp.Message);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    //res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL + "|" + JsonConvert.SerializeObject(req);
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            StringBuilder req = new StringBuilder("token={TOKEN}&MobileNo={MOBILE}&AccountNumber={ACCOUNT}&IFSCCode={IFSC}&Amount={AMOUNT}&Mode={MODE}&transid={TID}&FirstName={FIRSTNAME}&LastName={LASTNAME}&Pincode={PINCODE}&BeneficiaryName={BENEFICIARYNAME}&BankName={BANK}");

            req.Replace("{TOKEN}", apiSetting.Token);
            req.Replace("{MOBILE}", apiSetting.MobileNo);
            req.Replace("{MODE}", request.TransMode);
            req.Replace("{ACCOUNT}", request.mBeneDetail.AccountNo);
            req.Replace("{IFSC}", request.mBeneDetail.IFSC);
            req.Replace("{BENEIFSC}", request.mBeneDetail.IFSC);
            req.Replace("{TID}", request.TID.ToString());
            req.Replace("{AMOUNT}", request.Amount.ToString());
            req.Replace("{FIRSTNAME}", request.mBeneDetail.BeneName.Split(' ')[0].ToString());
            req.Replace("{LASTNAME}", request.mBeneDetail.BeneName.Split(' ')[1].ToString());
            req.Replace("{PINCODE}", request.Pincode.ToString());
            req.Replace("{BENEFICIARYNAME}", request.mBeneDetail.BeneName);
            req.Replace("{BANK}", request.mBeneDetail.BankName);
            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "V2/SendMoney?" + req.ToString();

            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DKPayAccTrfResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Data != null)
                        {
                            if (_apiRes.Status == "Success")//sucess
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.Message;
                                res.ErrorCode = 0;
                                res.LiveID = _apiRes.Data.optransid;
                                res.BeneName = request.mBeneDetail.BeneName;
                                res.VendorID = _apiRes.Data.referenceid;
                            }
                            else if (_apiRes.Status == "Fail")//fail
                            {
                                res.Statuscode = ErrorCodes.Minus1;
                                res.Msg = nameof(ErrorCodes.FAILED);
                                res.ErrorCode = ErrorCodes.Minus1;
                                res.VendorID = _apiRes.Data.referenceid;
                            }
                            else if (_apiRes.Status == "Pending")
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.VendorID = _apiRes.Data.referenceid;
                            }
                            else
                            {
                                errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.Message);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.Message);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    //res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.Message);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.Message);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                //res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL + "|" + JsonConvert.SerializeObject(req);
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }

        public DMRTransactionResponse GetTransactionStatus(int TID, int UserID, int RequestMode, string APIOpCode, string APIOutletID)
        {
            //string TransactionID,  int UserID, int APIID, string VendorID
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            StringBuilder req = new StringBuilder("token={TOKEN}&transid={TID}");

            req.Replace("{TOKEN}", apiSetting.Token);
            req.Replace("{TID}", TID.ToString());

            string response = string.Empty;
            var _URL = apiSetting.BaseURL + "PayoutStatus?" + req.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DKPayAccTrfResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.Data != null)
                        {
                            if (_apiRes.Status == "Success")
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.Message;
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.VendorID = _apiRes.Data.referenceid;
                                res.LiveID = _apiRes.Data.optransid;
                            }
                            else if (_apiRes.Status == "Fail")
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.Message;
                                res.ErrorCode = 0;
                                res.VendorID = _apiRes.Data.referenceid;
                                //res.LiveID = _apiRes.data[0].error;
                            }
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.Message);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.Message);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                //res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = " Exception:" + ex.Message + " | " + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = response;
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
