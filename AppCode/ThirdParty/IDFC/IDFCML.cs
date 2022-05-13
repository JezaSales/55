using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Issuance;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.IDFC
{
    public class IDFCML : IIssuanceAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly IDFCAppSetting appSetting;
        private readonly IDAL _dal;
        private string _JWTToken = string.Empty;

        public IDFCML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
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
        }
        private IDFCAppSetting AppSetting()
        {
            try
            {
                return new IDFCAppSetting
                {
                    EnityID = Configuration["ISSUE:IDFC:EnityID"],
                    BasicAuth = Configuration["ISSUE:IDFC:BasicAuth"],
                    SecretToken = Configuration["ISSUE:IDFC:SecretToken"],
                    GetOTPURL = Configuration["ISSUE:IDFC:GetOTPURL"],
                    VerifyOTPURL = Configuration["ISSUE:IDFC:VerifyOTPURL"],
                    CustomerOnboardURL = Configuration["ISSUE:IDFC:CustomerOnboardURL"],
                    CustomerUpdateURL = Configuration["ISSUE:IDFC:CustomerUpdateURL"],
                    TagIssuanceURL = Configuration["ISSUE:IDFC:TagIssuanceURL"],
                    ManageVehicleURL = Configuration["ISSUE:IDFC:ManageVehicleURL"],
                    VehicleDetailURL = Configuration["ISSUE:IDFC:VehicleDetailURL"],
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IDFCAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return new IDFCAppSetting();
        }

        public IssuanceOTPResponse GenerateOTP(IssuanceOTPRequest oTPRequest, Func<string, string, int> SaveAPILog)
        {
            var apiReq = new IDFCOTPRequest
            {
                custId = oTPRequest.CustomerID,
                entityId = appSetting.EnityID,
                mobNo = oTPRequest.MobileNo,
                requestId = oTPRequest.TransactionID,
                vrn = oTPRequest.VRN,
                txnTime = DateTime.Now.ToString("ddMMyyyyHHmmss")
            };
            var ChsumStr = new StringBuilder();
            ChsumStr.Append(appSetting.SecretToken);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.requestId);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.entityId);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.custId??string.Empty);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.vrn??string.Empty);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.mobNo);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.txnTime);
            var checkSum = HashEncryption.O.SHA256_ComputeHash(ChsumStr.ToString(), appSetting.SecretToken).ToUpper();
            apiReq.chkSm = checkSum;

            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new IssuanceOTPResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.GetOTPURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.GetOTPURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCOTPResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.resCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                            res.RefferencNo = apiResp.txnNo;
                        }
                        else
                        {
                            res.Msg = apiResp.resMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = oTPRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }

        public IssuanceMatchOTPResponse VerifyOTP(IssuanceOTPRequest oTPRequest, Func<string, string, int> SaveAPILog)
        {
            var apiReq = new IDFCMatchOTPRequest
            {
                entityId = appSetting.EnityID,
                otp = oTPRequest.MobileNo,
                requestId = oTPRequest.TransactionID,
                txnNo = oTPRequest.ReffrenceID,
                txnTime = DateTime.Now.ToString("ddMMyyyyHHmmss")
            };
            var ChsumStr = new StringBuilder();
            ChsumStr.Append(appSetting.SecretToken);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.requestId);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.entityId);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.otp);
            //ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.txnTime);
            var checkSum = HashEncryption.O.SHA256_ComputeHash(ChsumStr.ToString(), appSetting.SecretToken).ToUpper();
            apiReq.chkSm = checkSum;

            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new IssuanceMatchOTPResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.VerifyOTPURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.VerifyOTPURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCMatchOTPRepsonse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.resCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = "OTP Verified Successfully";
                            res.IsKYCDone = apiResp.customerType == "KYC";
                            res.IsCustomerFound = Validate.O.IsNumeric(apiResp.uId ?? "-") && !string.IsNullOrEmpty(apiResp.firstName);
                            res.CustomerName = res.IsCustomerFound ? apiResp.firstName + " " + (apiResp.middleName ?? string.Empty) + " " + (apiResp.lastName ?? string.Empty) : String.Empty;
                        }
                        else
                        {
                            res.Msg = apiResp.resMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = oTPRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }

        public IssuanceCustomerOnboardingResponse CustomerOnBoarding(IssuanceCustomerOnboardingRequest onboardingRequest, Func<string, string, int> SaveAPILog)
        {
            var apiReq = new IDFCOnboardRequest
            {
                entityId = appSetting.EnityID,
                requestId = onboardingRequest.TransactionID,
                customerId = onboardingRequest.CustomerID,
                action = onboardingRequest.IsUpdate ? "UPDATE" : "ADD",
                customerType = onboardingRequest.IsKYC ? "KYC" : "NKYC",
                gstNo = onboardingRequest.GSTIN,
                uId = onboardingRequest.UID,
                firstName = onboardingRequest.FirstName,
                middleName = onboardingRequest.MiddleName,
                lastName = onboardingRequest.LastName,
                gender = onboardingRequest.Gender,
                dob = onboardingRequest.DOB,
                mobileNo = onboardingRequest.MobileNo,
                emailId = onboardingRequest.EmailId,
                phoneNo = onboardingRequest.Phone,
                usrConsent = "Y",
                addressDetail = new IDFCAddressDetail
                {
                    address1 = onboardingRequest.Address1,
                    address2 = onboardingRequest.Address2,
                    address3 = onboardingRequest.Address3,
                    district = onboardingRequest.District,
                    city = onboardingRequest.City,
                    country = onboardingRequest.Country,
                    pincode = onboardingRequest.Pincode,
                    state = onboardingRequest.State
                },
                docdtls = new List<IDFCDoc>(),
                bankDetail = null,
                txnTime = DateTime.Now.ToString("ddMMyyyyHHmmss")
            };
            if (onboardingRequest.DocDetails != null)
            {
                if (onboardingRequest.DocDetails.Count > 0)
                {
                    foreach (var item in onboardingRequest.DocDetails)
                    {
                        apiReq.docdtls.Add(new IDFCDoc
                        {
                            docName = item.DocName,
                            docNo = item.DocNo,
                            docType = item.DocType,
                            uploadFile = item.UploadFile
                        });
                    }
                }
            }
            var ChsumStr = new StringBuilder();
            ChsumStr.Append(appSetting.SecretToken);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.requestId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.entityId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.customerId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.firstName);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.gender);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.customerType);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.dob);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.mobileNo);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.addressDetail.pincode);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.addressDetail.address1);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.gstNo);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.uId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.usrConsent);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.txnTime);
            var checkSum = HashEncryption.O.SHA256_ComputeHash(ChsumStr.ToString(), appSetting.SecretToken);
            apiReq.chkSm = checkSum;

            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new IssuanceCustomerOnboardingResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.CustomerOnboardURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.CustomerOnboardURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCOnboardResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.resCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = apiResp.resMsg;
                        }
                        else
                        {
                            res.Msg = apiResp.resMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = onboardingRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }


        public IssuanceCommonResponse UpdateKYC(IssuanceUpdateKYCRequest updateKYCRequest, Func<string, string, int> SaveAPILog)
        {
            var apiReq = new IDFCUpdateKYCRequest
            {
                entityId = appSetting.EnityID,
                requestId = updateKYCRequest.TransactionID,
                makerId = "admin.1",
                kycUpdate = "KYC",
                docdtls = new IDFCDoc
                {
                    docType = "UPDATE",
                    docName = updateKYCRequest.docdtls.DocName,
                    docNo = updateKYCRequest.docdtls.DocNo,
                    uploadFile = updateKYCRequest.docdtls.UploadFile,
                },
                dateTime = DateTime.Now.ToString("ddMMyyyyHHmmss")
            };

            var ChsumStr = new StringBuilder();
            ChsumStr.Append(appSetting.SecretToken);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.requestId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.entityId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.custId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.makerId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.kycUpdate);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.dateTime);
            var checkSum = HashEncryption.O.SHA256_ComputeHash(ChsumStr.ToString(), appSetting.SecretToken);
            apiReq.chkSm = checkSum;

            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new IssuanceCommonResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.CustomerUpdateURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.CustomerUpdateURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCIssuanceTagResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.respCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = apiResp.respMessage;
                        }
                        else
                        {
                            res.Msg = apiResp.respMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateKYC",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = updateKYCRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }
        public IssuanceCommonResponse TagIssuance(IssuanceTagRequest tagRequest, Func<string, string, int> SaveAPILog)
        {
            //TagIssuanceURL
            var apiReq = new IDFCIssuanceTagRequest
            {
                entityId = appSetting.EnityID,
                requestId = tagRequest.TransactionID,
                custId = tagRequest.CustomerID,
                action = tagRequest.IsUpdate ? "UPDATE" : "ISSUE",
                mobileNo = tagRequest.MobileNo,
                product = tagRequest.Product,
                tvc = tagRequest.TVC,
                cch = tagRequest.CCH,
                tagId = tagRequest.TagID,
                chasisNo = tagRequest.ChasisNumber,
                barcode = tagRequest.BarCode,
                engineNo = tagRequest.EngineNumber,
                vehicleMakeModel = tagRequest.VehicleMakeModel,
                vehicleColor = tagRequest.VehicleColor,
                vehicleRegAvlbl = tagRequest.VehicleNumberAvailability,
                vrn = tagRequest.VRN,
                regDate = Convert.ToDateTime(tagRequest.RegistrationDate).ToString("dd/MM/yyyy"),
                issFee = tagRequest.IssuanceFees.ToString(),
                security = tagRequest.SecurityFees.ToString(),
                initial = tagRequest.InitialAmount.ToString(),
                minBal = tagRequest.MinBalance.ToString(),
                exempted = "Y",
                exemptedCatg = string.Empty,
                commVeh = tagRequest.IsCommercialVehicle ? "Y" : "N",
                vehImg = string.Empty,
                docdtls = new IDFCDoc
                {
                    docType = "IDN",
                    docName = "Vehicle Registration Certificate",
                    docNo = tagRequest.VehicleRegistraionNo,
                    uploadFile = tagRequest.VehicleRegistraionFile
                },
                txnTime = DateTime.Now.ToString("ddMMyyyyHHmmss")
            };

            var ChsumStr = new StringBuilder();
            ChsumStr.Append(appSetting.SecretToken);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.requestId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.entityId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.custId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.tagId);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.product);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.tvc);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.chasisNo);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.vrn);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.cch);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.initial);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.barcode);
            ChsumStr.Append(" ");
            ChsumStr.Append(apiReq.mobileNo);
            var checkSum = HashEncryption.O.SHA256_ComputeHash(ChsumStr.ToString(), appSetting.SecretToken);
            apiReq.chkSm = checkSum;

            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new IssuanceCommonResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.TagIssuanceURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.TagIssuanceURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCIssuanceTagResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.respCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = apiResp.respMessage;
                        }
                        else
                        {
                            res.Msg = apiResp.respMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "TagIssuance",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = tagRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }

        public ManageVRNResponse ManageVRN(ManageVRNRequest vRNRequest, Func<string, string, int> SaveAPILog)
        {
            //TagIssuanceURL
            var apiReq = new IDFCManageVRNRequest
            {
                entityId = appSetting.EnityID,
                action = vRNRequest.IsRemove? "Remove" : "Add",
                vrn = vRNRequest.VRN
            };
            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new ManageVRNResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.ManageVehicleURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.ManageVehicleURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCVRNResponse>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.resCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = apiResp.resMsg;
                            res.SuccessVRN = apiResp.successVrn;
                        }
                        else
                        {
                            res.Msg = apiResp.resMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ManageVRN",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = vRNRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }

        public VehicleDetailResponse VehicleDetailEnquiry(VehicleDetailRequest vRNRequest, Func<string, string, int> SaveAPILog)
        {
            //TagIssuanceURL
            var apiReq = new
            {
                entityId = appSetting.EnityID,
                Tag = vRNRequest.TagID,
                vrn = vRNRequest.VRN
            };
            var header = new Dictionary<string, string>
            {
                { "Authorization","Basic "+appSetting.BasicAuth},
                //{ ContentType.Self,ContentType.application_json}
            };
            var res = new VehicleDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var response = string.Empty;
            var request = string.Empty;
            try
            {
                request = (appSetting.ManageVehicleURL ?? string.Empty) + JsonConvert.SerializeObject(apiReq) + JsonConvert.SerializeObject(header);
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(appSetting.ManageVehicleURL, apiReq, header).Result;
                if (response != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IDFCVehicleDetail>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.resCode == "700")
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = apiResp.resMsg;
                            res.VRN = apiResp.vrn;
                            res.TagID = apiResp.tagId;
                            res.Status = apiResp.status;
                            res.VehicleClass = apiResp.vehClass;
                            res.VehicleDescription = apiResp.vehDesc;
                            res.AvailableBalance = apiResp.availableBal;
                            res.Name = apiResp.name;    
                        }
                        else
                        {
                            res.Msg = apiResp.resMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception[" + ex.Message + "]" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VehicleDetailEnquiry",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = vRNRequest.UserID
                });
            }
            SaveAPILog(request, response);
            return res;
        }
    }
}
