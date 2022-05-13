using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{

    public partial class IMWalletDMT : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly string _APICode;
        private readonly string _APIGroupCode;
        private string _FinodinToken;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly IMWAppSetting _appSetting;
        private IErrorCodeML errorCodeML;
        private readonly WebsiteInfo _WInfo;


        public IMWalletDMT(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, string APICode, int APIID, string APIGroupCode)
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
            errorCodeML = new ErrorCodeML(_accessor, _env, false);
            _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
            _appSetting = AppSetting();
        }
        public IMWAppSetting AppSetting()
        {
            var setting = new IMWAppSetting();
            try
            {
                setting = new IMWAppSetting
                {
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    webToken = Configuration["DMR:" + _APICode + ":webToken"],
                    userCode = Configuration["DMR:" + _APICode + ":userCode"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IMWalletAppSetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "get_senderRegistration_OTP.jsp";
            try
            {
                //"{\"otp_token\":\"1082f718-3e6a-48a2-b34f-64e113ef9ab7\",\"requestID\":\"ZGSQ4T2XLEZ8VD\",\"status\":\"success\"}";
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
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
                                UserID = request.UserID,
                            })) as SenderInfo;
                            if (dbres.Statuscode == ErrorCodes.Minus1)
                            {
                                res.Msg = dbres.Msg;
                                return res;
                            }
                            if (dbres.Statuscode == ErrorCodes.One)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                                res.IsOTPGenerated = true;
                                res.ReferenceID = _apiRes.otp_token;
                                //res.IsOTPResendAvailble = true;
                                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(new CommonReq
                                {
                                    CommonStr = request.SenderMobile,
                                    CommonStr2 = dbres.OTP,
                                    CommonInt = request.UserID,
                                });
                            }
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = nameof(DMTErrorCodes.Transaction_Failed).Replace("_", " ");
                        res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + _postReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "checkSender.jsp";
            try
            {
                //"{\"msg\":\"Mobile Number Not Found\",\"errorCode\":1032,\"status\":\"success\"}";
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.errorCode == (int)IMWErrCode.MobileNumberNotFound && _apiRes.msg.Contains("Mobile Number Not Found"))
                        {
                            res.Msg = _apiRes.msg;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                            res.IsSenderNotExists = true;
                            res.Statuscode = ErrorCodes.One;
                        }
                        else if (_apiRes.errorCode == (int)IMWErrCode.success && _apiRes.status.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            res.SenderName = _apiRes.firstName + " " + _apiRes.lastName;
                            res.RemainingLimit = Convert.ToDecimal(_apiRes.remainingLimit);
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(ErrorCodes.FAILED);
                            res.ErrorCode = ErrorCodes.Minus1;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = string.Concat(_URL + "|" + _postReq),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException(); ;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {

            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            //var req = new
            //{
            //    token = _FinodinToken,
            //    apiSource = string.Empty,
            //    subAgentID = string.Empty,
            //    mobile = string.Empty,
            //    remName = string.Empty,
            //    address = string.Empty,
            //    city = string.Empty,
            //    state = string.Empty,
            //    pinCode = string.Empty
            //};
            //string response = string.Empty;
            //var _URL = apiSetting.BaseURL + "RemitterRegister";
            //try
            //{
            //    response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
            //    if (!string.IsNullOrEmpty(response))
            //    {
            //        var _apiRes = JsonConvert.DeserializeObject<MMWFintechObjResp>(response);
            //        if (_apiRes != null)
            //        {
            //            if (_apiRes.Message.Equals(MMWFintechCodes.MsgOtpSent) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
            //            {
            //                senderCreateResp.Statuscode = ErrorCodes.One;
            //                senderCreateResp.Msg = _apiRes.Message;
            //                senderCreateResp.IsOTPGenerated = true;
            //                senderCreateResp.IsOTPResendAvailble = true;
            //                senderCreateResp.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;

            //            }
            //            else
            //            {
            //                senderCreateResp.Statuscode = ErrorCodes.Minus1;
            //                senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
            //                senderCreateResp.ErrorCode = ErrorCodes.Minus1;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    response = ex.Message + "|" + response;
            //    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
            //    {
            //        ClassName = GetType().Name,
            //        FuncName = "SenderResendOTP",
            //        Error = ex.Message,
            //        LoginTypeID = LoginType.ApplicationUser,
            //        UserId = request.UserID
            //    });
            //}
            //new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            //{
            //    APIID = request.APIID,
            //    Method = "SenderResendOTP",
            //    RequestModeID = request.RequestMode,
            //    Request = _URL + JsonConvert.SerializeObject(req),
            //    Response = response,
            //    SenderNo = request.SenderMobile,
            //    UserID = request.UserID,
            //    TID = request.TransactionID
            //});
            return senderCreateResp;
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile,
                    firstName = senderRequest.Name.Contains(" ") ? senderRequest.Name.Split(" ")[0].ToString() : senderRequest.Name,
                    lastName = senderRequest.Name.Contains(" ") ? senderRequest.Name.Split(" ")[1].ToString() : senderRequest.Name,
                    pin = senderRequest.Pincode,
                    otp = request.OTP,
                    address = senderRequest.Address,
                    city = senderRequest.City,
                    state = senderRequest.Statename.Replace(" ", "").ToString(),
                    otpToken = request.ReferenceID
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "senderRegistration.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.msg.Equals(MMWFintechCodes.MsgSAS) && _apiRes.errorCode == MMWFintechCodes.Msg200)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully);
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
                        }
                        else if (_apiRes.msg.Equals(MMWFintechCodes.MsgOtpExp) && _apiRes.errorCode == MMWFintechCodes.Msg200)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.OTP_Expired) + "! Please Resend it!";
                            res.ErrorCode = DMTErrorCodes.OTP_Expired;
                        }
                        else if (_apiRes.msg.Contains("Otp and State either both should be empty or both should be non empty") || _apiRes.errorCode == (int)IMWErrCode.OTPNotMatchedError)
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.msg);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.msg);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                //res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + _postReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile,
                    bankname = request.mBeneDetail.BankName,
                    ifsc = request.mBeneDetail.IFSC,
                    account = request.mBeneDetail.AccountNo,
                    benename = request.mBeneDetail.BeneName
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "add_beneficiary.jsp";

            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.errorCode == 0 && _apiRes.status.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully);
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
                        }
                        else if (_apiRes.status.Equals("Failed"))
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = _apiRes.msg;
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
                        }
                        else
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            res.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "CreateBeneficiary",
                    Error = response,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + _postReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "get_BeneList.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWGetBeneResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.errorCode == 0 && _apiRes.status.Equals("success"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful);
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (_apiRes.data != null)
                            {
                                if (_apiRes.data != null)
                                {
                                    var Beneficiaries = new List<MBeneDetail>();
                                    foreach (var item in _apiRes.data)
                                    {
                                        Beneficiaries.Add(new MBeneDetail
                                        {
                                            AccountNo = item.account,
                                            BankName = item.bankName,
                                            IFSC = item.ifsc,
                                            BeneName = item.accountHolderName,
                                            BeneID = item.beneID,
                                            IsVerified = true
                                        });
                                    }
                                    res.Beneficiaries = Beneficiaries;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                            }
                        }
                        else //if(_apiRes.statusCode == (int)FinodinErrCode.Error)
                        {
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_not_found;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        private IEnumerable<MBeneDetail> List<T>(T recipientList)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            var resDB = (new ProcGetBenificiaryByBeneID(_dal).Call(new DMTReq { SenderNO = request.SenderMobile, BeneAPIID = Convert.ToInt32(request.mBeneDetail.BeneID) })) as BenificiaryDetail;
            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                benID = string.Empty,
                otp = string.Empty
            };
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "BeneficiaryAddValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes.Message.Equals(MMWFintechCodes.MsgVerifyBen) && _apiRes.Status.Equals(MMWFintechCodes.Msg200) && _apiRes.MessageStatus == true)
                    {
                        if (_apiRes.JsonResult != null)
                        {
                            if (_apiRes.JsonResult.ISValid.Equals("true"))
                            {
                                senderCreateResp.Statuscode = -2;
                                senderCreateResp.Msg = _apiRes.Message;
                                senderCreateResp.ErrorCode = ErrorCodes.One;
                            }
                        }
                    }
                    else
                    {
                        senderCreateResp.Statuscode = ErrorCodes.Minus1;
                        senderCreateResp.Msg = nameof(ErrorCodes.FAILED);
                        senderCreateResp.ErrorCode = ErrorCodes.Minus1;
                    }

                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GenerateOTP",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                token = _FinodinToken,
                apiSource = string.Empty,
                subAgentID = string.Empty,
                mobile = string.Empty,
                benID = string.Empty,
                otp = string.Empty
            };
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "BeneficiaryAddValidate";
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<MMWFintechResp>(response);
                    if (_apiRes != null)
                    {
                        //mSenderLoginResponse.Statuscode = _apiRes.Statuscode;
                        //mSenderLoginResponse.Msg = _apiRes.Message;
                        //mSenderLoginResponse.ErrorCode = _apiRes.ErrorCode;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "ValidateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(req),
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "delete_bene_OTP.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status.Equals("success"))
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.One;
                            senderLoginResponse.IsOTPGenerated = true;
                            senderLoginResponse.ReferenceID = _apiRes.otp_token;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully);
                            senderLoginResponse.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                        }
                        else
                        {
                            senderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            senderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            senderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + _postReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            var mSenderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile,
                    otp = request.OTP,
                    otpToken = request.ReferenceID,
                    beneID = request.mBeneDetail.BeneID
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "delete_bene.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status.Equals("success"))
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.One;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Beneficiary_Deactivated_or_Deleted);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Beneficiary_Deactivated_or_Deleted;
                        }
                        else
                        {
                            mSenderLoginResponse.Statuscode = ErrorCodes.Minus1;
                            mSenderLoginResponse.Msg = nameof(DMTErrorCodes.Transaction_Failed);
                            mSenderLoginResponse.ErrorCode = DMTErrorCodes.Transaction_Failed;
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
                    FuncName = "RemoveBeneficiaryValidate",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiaryValidate",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + _postReq,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return mSenderLoginResponse;
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
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile,
                    bankname = request.mBeneDetail.BankName,
                    ifsc = request.mBeneDetail.IFSC,
                    account = request.mBeneDetail.AccountNo,
                    callBack = string.Concat(_WInfo.WebsiteName, "/Callback/", _APIID.ToString()),
                    orderid = request.TID.ToString()
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "account_validate.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response != null)
                    {
                        var _apiResp = JsonConvert.DeserializeObject<IMWResp>(response);
                        if (_apiResp != null)
                        {
                            if (_apiResp.errorCode == 0 && _apiResp.status.Equals("success"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.BeneName = _apiResp.beneName;
                                res.LiveID = _apiResp.utr;
                            }
                            else
                            {
                                errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiResp.msg);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiResp.msg);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
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
            res.Request = string.Concat(_URL, "|", _postReq);
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
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    senderMobile = request.SenderMobile,
                    orderid = request.TID.ToString(),
                    amount = request.Amount.ToString(),
                    beneID = request.mBeneDetail.BeneID,
                    mode = request.TransMode,
                    callBack = string.Concat(_WInfo.WebsiteName, "/Callback/", _APIID.ToString()),
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "sendMoney.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWAccTrResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status.ToUpper().In("SUCCESS", "INPROGRESS"))
                        {
                            if (_apiRes.data != null)
                            {
                                if (_apiRes.data[0].status.ToUpper().Equals("SUCCESS") )
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = _apiRes.data[0].status;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.data[0].imwsubID;
                                    res.LiveID = _apiRes.data[0].utr;
                                    res.BeneName = request.mBeneDetail.BeneName;
                                }
                                else if (_apiRes.data[0].status.Equals("Pending"))
                                {
                                    res.Statuscode = RechargeRespType.PENDING;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.data[0].imwsubID;
                                }
                                //else if ()//for failed
                                //{
                                //    res.Statuscode = RechargeRespType.FAILED;
                                //    res.Msg = _apiRes.transaction[0].error;
                                //    res.ErrorCode = 0;
                                //    res.VendorID = _apiRes.transaction[0].tranID;
                                //    res.LiveID = _apiRes.transaction[0].error;
                                //    res.BeneName = request.mBeneDetail.BeneName;
                                //}
                            }
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.msg);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.msg);
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
            res.Request = _URL + "|" + _postReq;
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
        //private DMRTransactionResponse LoopStatusCheck(int TID, int RequestMode, int UserID, int APIID)
        //{
        //    var res = new DMRTransactionResponse
        //    {
        //        Statuscode = RechargeRespType.PENDING,
        //        Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Request_Accpeted
        //    };
        //    int i = 0, LoopCount = 1;
        //    while (i < LoopCount)
        //    {
        //        i++;
        //        if (res.Statuscode == RechargeRespType.PENDING)
        //        {
        //            res = GetTransactionStatus(TID, RequestMode, UserID, APIID);
        //            if (res.Statuscode != RechargeRespType.PENDING)
        //            {
        //                i = LoopCount;
        //            }
        //        }
        //        else
        //        {
        //            i = LoopCount;
        //        }
        //    }
        //    return res;
        //}
        public DMRTransactionResponse GetTransactionStatus(int TID, int UserID, int RequestMode, string APIOpCode, string APIOutletID)
        {
            //string TransactionID,  int UserID, int APIID, string VendorID
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var req = new
            {
                webToken = _appSetting.webToken,
                userCode = _appSetting.userCode,
                parameters = new
                {
                    agentid = TID.ToString()
                }
            };
            string _postReq = "data=" + JsonConvert.SerializeObject(req);
            string response = string.Empty;
            var _URL = _appSetting.BaseURL + "checkStatus.jsp";
            try
            {
                response = AppWebRequest.O.PostDataWTP(_URL, _postReq);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<IMWAccTrResp>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.status.ToUpper().In("SUCCESS", "INPROGRESS"))
                        {
                            if (_apiRes.data != null)
                            {
                                if (_apiRes.data[0].status.ToUpper().Equals("SUCCESS"))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = _apiRes.data[0].status;
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.VendorID = _apiRes.data[0].imwsubID;
                                    res.LiveID = _apiRes.data[0].utr;
                                    res.BeneName = _apiRes.beneName;
                                }
                                else if (_apiRes.data[0].status.ToUpper().Equals("FAILED"))
                                {
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = _apiRes.data[0].status;
                                    res.ErrorCode = 0;
                                    res.VendorID = _apiRes.data[0].imwsubID;
                                    //res.LiveID = _apiRes.data[0].error;
                                    res.BeneName = _apiRes.beneName;
                                }
                                //else if ()//for failed
                                //{
                                //    res.Statuscode = RechargeRespType.FAILED;
                                //    res.Msg = _apiRes.transaction[0].error;
                                //    res.ErrorCode = 0;
                                //    res.VendorID = _apiRes.transaction[0].tranID;
                                //    res.LiveID = _apiRes.transaction[0].error;
                                //    res.BeneName = request.mBeneDetail.BeneName;
                                //}
                            }
                        }
                        else
                        {
                            errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(_APIGroupCode, _apiRes.msg);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.msg);
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
                Request = _URL + "|" + _postReq,
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
        //public ResponseStatus RefundOTP(string VendorID, int RequestMode, int UserID, string SenderNo, int TID, int APIID)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Unknown_Error
        //    };
        //    if (!ChannelPartnerLogin())
        //    {
        //        res.Msg = "instituteId expired";
        //        return res;
        //    }
        //    var req = new refundotpreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessionToken = RBLSession
        //        },
        //        RBLtransactionid = VendorID
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.DMTServiceURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
        //    string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
        //    var headers = new Dictionary<string, string>
        //    {
        //        { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
        //    };
        //    try
        //    {
        //        string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
        //        response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
        //        if (!string.IsNullOrEmpty(response))
        //        {
        //            var errorCheck = GetErrorIfExists(response);
        //            if (errorCheck.Statuscode == ErrorCodes.One)
        //            {
        //                var _apiRes = new refundotpres();
        //                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
        //                if ((_apiRes.status ?? 0) == 1)
        //                {
        //                    res.Statuscode = ErrorCodes.One;
        //                    res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
        //                }
        //                else
        //                {

        //                    res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
        //                }
        //            }
        //            else
        //            {

        //                res.Msg = errorCheck.Msg;
        //                res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = " Exception:" + ex.Message + " | " + response;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "ResendRefundOTP",
        //            Error = ex.Message,
        //            LoginTypeID = 1,
        //            UserId = UserID
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    var dMTReq = new DMTReqRes
        //    {
        //        APIID = APIID,
        //        Method = "RefundOTP",
        //        RequestModeID = RequestMode,
        //        Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
        //        Response = response,
        //        SenderNo = SenderNo,
        //        UserID = UserID,
        //        TID = TID.ToString()
        //    };
        //    new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        //    return res;
        //}
        //public ResponseStatus Refund(int TID, string VendorID, int RequestMode, int UserID, string SenderNo, string OTP, int APIID)
        //{
        //    var res = new ResponseStatus
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
        //        ErrorCode = ErrorCodes.Unknown_Error
        //    };
        //    if (!ChannelPartnerLogin())
        //    {
        //        res.Msg = "instituteId expired";
        //        return res;
        //    }
        //    var req = new refundreq
        //    {
        //        header = new RBLHeader
        //        {
        //            sessionToken = RBLSession
        //        },
        //        bcagent = appSetting.BCAGENT,
        //        channelpartnerrefno = TIDPrefix + TID,
        //        verficationcode = OTP,
        //        flag = 1
        //    };
        //    var xmlReq = XMLHelper.O.SerializeToXml(req, null);
        //    string response = string.Empty;
        //    var _URL = appSetting.DMTServiceURL + string.Format("?client_id={0}&client_secret={1}", appSetting.CLIENTID, appSetting.CLIENTSECRET);
        //    string authString = string.Format("{0}:{1}", appSetting.LDAP, appSetting.LDAPPASSWORD);
        //    var headers = new Dictionary<string, string>
        //    {
        //        { "Authorization", "Basic "+ Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) }
        //    };
        //    try
        //    {
        //        string KeyPath = Path.Combine(Directory.GetCurrentDirectory(), RBLPFXFile);
        //        response = AppWebRequest.O.PostJsonDataUsingHWRTLSWithCertificate(_URL, xmlReq, headers, KeyPath, appSetting.PFXPD).Result;
        //        if (!string.IsNullOrEmpty(response))
        //        {
        //            var errorCheck = GetErrorIfExists(response);
        //            if (errorCheck.Statuscode == ErrorCodes.One)
        //            {
        //                var _apiRes = new refundres();
        //                _apiRes = XMLHelper.O.DesrializeToObject(_apiRes, response, null);
        //                if ((_apiRes.status ?? 0) == 1)
        //                {
        //                    res.Statuscode = ErrorCodes.One;
        //                    res.Msg = nameof(DMTErrorCodes.Transaction_Status_Updated_Refund).Replace("_", " ");
        //                    res.ErrorCode = DMTErrorCodes.Transaction_Status_Updated_Refund;
        //                }
        //                else
        //                {
        //                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
        //                    res.ErrorCode = ErrorCodes.Invalid_OTP;
        //                }
        //            }
        //            else
        //            {
        //                res.Msg = errorCheck.Msg;
        //                res.ErrorCode = ErrorCodes.Unknown_Error;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response = " Exception:" + ex.Message + " | " + response;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "Refund",
        //            Error = ex.Message,
        //            LoginTypeID = 1,
        //            UserId = UserID
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    var dMTReq = new DMTReqRes
        //    {
        //        APIID = APIID,
        //        Method = "Refund",
        //        RequestModeID = RequestMode,
        //        Request = _URL + xmlReq + JsonConvert.SerializeObject(headers),
        //        Response = response,
        //        SenderNo = SenderNo,
        //        UserID = UserID,
        //        TID = TID.ToString()
        //    };
        //    new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
        //    res.CommonStr = dMTReq.Request;
        //    res.CommonStr2 = dMTReq.Response;
        //    return res;
        //}
    }
}