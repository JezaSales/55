using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.CashFree;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SmartCollectML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public SmartCollectML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        public SmartCollectViewModel GetSmartVirtualAccountDetail(int LoginID)
        {
            var res = new SmartCollectViewModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.SystemErrorDown
            };
            var collectTypes = GetSmartCollectType();
            if (collectTypes.Count() > 0)
            {
                var iciciType = collectTypes.Where(x => x.Id == SmartCollectType.ICICISmartCollect && x.IsActive).ToList();
                if (iciciType.Count() > 0)
                {
                    res.iciciCollectData = new SmartCollectDataModel
                    {
                        IsVirtualShow = iciciType[0].IsVirtual && ApplicationSetting.IsECollectEnable,
                        IsQRShow = iciciType[0].IsQR && ApplicationSetting.IsUPIQR,
                        IsVPAShow = iciciType[0].IsVPA && ApplicationSetting.IsUPI
                    };
                    IProcedure proc = new ProcGetICICIQRResp(_dal);
                    var procres = (ResponseStatus)proc.Call(new CommonReq
                    {
                        LoginTypeID = LoginType.ApplicationUser,
                        LoginID = LoginID
                    });
                    if (procres.Statuscode == ErrorCodes.One)
                    {
                        res.Statuscode = procres.Statuscode;
                        res.Msg = "Detail found";
                        var smartData = new SmartCollectData
                        {
                            QR = string.Empty,
                            Account = string.Empty,
                            AccountHolder = string.Empty,
                            BankName = string.Empty,
                            IFSC = string.Empty,
                            VPA = string.Empty
                        };
                        if (res.iciciCollectData.IsVirtualShow)
                        {
                            ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);
                            var appSetting = iCICIPayoutML.AppSetting();

                            smartData.BankName = "ICICI";
                            smartData.IFSC = appSetting.CollectIFSC;
                            smartData.Account = appSetting.CollectVirtualCode + procres.CommonStr3;
                            smartData.AccountHolder = appSetting.CollectBeneName;
                        }
                        res.iciciCollectData.data = smartData;
                    }
                }
                SmartCollectML smartCollect = new SmartCollectML(_accessor, _env);
                var razorType = collectTypes.Where(x => x.Id == SmartCollectType.RazorPaySmartCollect && x.IsActive).ToList();
                if (razorType.Count() > 0)
                {
                    res.razorpayCollectData = new SmartCollectDataModel
                    {
                        IsVirtualShow = razorType[0].IsVirtual && ApplicationSetting.IsECollectEnable,
                        IsQRShow = razorType[0].IsQR && ApplicationSetting.IsUPIQR,
                        IsVPAShow = razorType[0].IsVPA && ApplicationSetting.IsUPI
                    };
                    var smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                    bool IsCallupdate = true;
                    RazorpayHelper(smrtUsrDetail, res, IsCallupdate);
                    if (IsCallupdate)
                    {
                        var updateRes = smartCollect.UpdateSmartCollectDetailOfUser(LoginID, LoginID);
                        if (updateRes.Statuscode == ErrorCodes.One)
                        {
                            smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                            RazorpayHelper(smrtUsrDetail, res, IsCallupdate);
                        }
                    }
                }
                var cashfreeType = collectTypes.Where(x => x.Id == SmartCollectType.CashfreeSmartCollect && x.IsActive).ToList();
                if (cashfreeType.Count() > 0)
                {
                    res.cashfreeCollectData = new SmartCollectDataModel
                    {
                        IsVirtualShow = cashfreeType[0].IsVirtual && ApplicationSetting.IsECollectEnable,
                        IsQRShow = cashfreeType[0].IsQR && ApplicationSetting.IsUPIQR,
                        IsVPAShow = cashfreeType[0].IsVPA && ApplicationSetting.IsUPI
                    };
                    var smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                    bool IsCallupdate = true;
                    CashfreeHelper(smrtUsrDetail, res, IsCallupdate);
                    if (IsCallupdate)
                    {
                        var updateRes = smartCollect.UpdateSmartCollectDetailOfUser(LoginID, LoginID);
                        if (updateRes.Statuscode == ErrorCodes.One)
                        {
                            smrtUsrDetail = smartCollect.GetUserSmartDetails(LoginID, LoginID);
                            CashfreeHelper(smrtUsrDetail, res, IsCallupdate);
                        }
                    }
                }
            }
            return res;
        }
        
        public List<SmartCollect> GetSmartCollectType()
        {
            IProcedure _proc = new ProcGetSmartCollectType(_dal);
            var res = (List<SmartCollect>)_proc.Call();
            return res;
        }
        public ResponseStatus ChangeSmartCollectTypeStatus(SmartCollect smartCollect)
        {
            IProcedure _proc = new ProcChangeSmartCollectTypeStatus(_dal);
            return (ResponseStatus)_proc.Call(smartCollect);
        }
        public UserSmartDetailModel GetUserSmartDetails(int LoginID, int UserID)
        {
            IProcedure proc = new ProcGetUserSmartCollectDetail(_dal);
            return (UserSmartDetailModel)proc.Call(new CommonReq
            {
                LoginID = LoginID,
                UserID = UserID
            });
        }
        public ResponseStatus UpdateSmartCollectDetailOfUser(int LoginID, int UserID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcGetUserSmartCollectDetail(_dal);
            var smartDetailList = (UserSmartDetailModel)proc.Call(new CommonReq
            {
                LoginID = LoginID,
                UserID = UserID
            });
            if (smartDetailList != null)
            {
                if (smartDetailList.USDList.Count > 0)
                {
                    var userSmartDetail_Razorpay = new RoundpayFinTech.AppCode.Model.UserSmartDetail();
                    if (smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).Count() > 0)
                    {
                        userSmartDetail_Razorpay = smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).ToList()[0];
                        if (userSmartDetail_Razorpay.SmartCollectTypeID > 0)
                        {
                            RazorpaySmartCollectML RZRPayObj = new RazorpaySmartCollectML(_accessor, _env, _dal);
                            var createCustomerResp = RZRPayObj.CreateCustomer(new SmartCollectCreateCustomerRequest
                            {
                                Name = smartDetailList.Name,
                                EmailID = smartDetailList.EmailID,
                                Contact = smartDetailList.MobileNo,
                                GSTIN = smartDetailList.GSTIN,
                                NotesKey1 = "Customer Registration " + smartDetailList.MobileNo,
                                NotesKey2 = "Customer Registration " + smartDetailList.EmailID
                            });
                            if (createCustomerResp.Statuscode == ErrorCodes.One)
                            {
                                var virtualAcResp = RZRPayObj.CreateVirtualAccount(new SmartCollectCreateCustomerRequest
                                {
                                    CustomerID = createCustomerResp.CustomerID,
                                    Contact = smartDetailList.MobileNo,
                                    Name = smartDetailList.Name
                                });
                                IProcedure procUpdate = new ProcUpdateCustomerSmartAccountDetail(_dal);
                                res = (ResponseStatus)procUpdate.Call(new UpdateSmartCollectRequestModel
                                {
                                    CustomerID = createCustomerResp.CustomerID,
                                    LoginID = LoginID,
                                    UserID = UserID,
                                    SmartAccountNo = virtualAcResp.AccountNumber,
                                    SmartCollectTypeID = SmartCollectType.RazorPaySmartCollect,
                                    SmartQRShortURL = virtualAcResp.QRShortURL,
                                    SmartVPA = virtualAcResp.VPAAddress
                                });
                            }
                        }
                    }
                    if (smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.CashfreeSmartCollect).Count() > 0)
                    {
                        var userSmartDetail = smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.CashfreeSmartCollect).ToList()[0];
                        if (userSmartDetail.SmartCollectTypeID > 0)
                        {
                            if (string.IsNullOrEmpty(userSmartDetail.SmartAccountNo))
                            {
                                var cashfreeML = new CashfreeCollectML(_accessor, _env, _dal, string.Empty);
                                var cashfreeRes = cashfreeML.CreateVirtualAccount(new SmartCollectCreateCustomerRequest
                                {
                                    Contact = smartDetailList.MobileNo,
                                    EmailID = smartDetailList.EmailID,
                                    Name = smartDetailList.Name,
                                    UserID = UserID
                                });
                                if (cashfreeRes.Statuscode == ErrorCodes.One)
                                {
                                    if (cashfreeRes.collectAccountDetails != null)
                                    {
                                        if (cashfreeRes.collectAccountDetails.Count > 0)
                                        {
                                            var updateReq = new UpdateSmartCollectRequestModel
                                            {
                                                CustomerID = cashfreeRes.CustomerID,
                                                LoginID = LoginID,
                                                UserID = UserID,
                                                SmartAccountNo = String.Empty,
                                                SmartCollectTypeID = SmartCollectType.CashfreeSmartCollect,
                                                SmartQRShortURL = String.Empty,
                                                SmartVPA = String.Empty
                                            };
                                            updateReq.tp_SmartCollect = new System.Data.DataTable();
                                            updateReq.tp_SmartCollect.Columns.Add("_BankCode", typeof(string));
                                            updateReq.tp_SmartCollect.Columns.Add("_AccountNo", typeof(string));
                                            updateReq.tp_SmartCollect.Columns.Add("_IFSC", typeof(string));
                                            foreach (var item in cashfreeRes.collectAccountDetails)
                                            {
                                                updateReq.tp_SmartCollect.Rows.Add(new object[] { (item.BankCode ?? String.Empty), (item.AccountNumber ?? String.Empty), (item.IFSC ?? String.Empty) });
                                            }
                                            IProcedure procUpdate = new ProcUpdateCustomerSmartAccountDetail(_dal);
                                            res = (ResponseStatus)procUpdate.Call(updateReq);
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(userSmartDetail.SmartVPA))
                            {
                                var cashfreeML = new CashfreeCollectML(_accessor, _env, _dal, string.Empty);
                                var cashfreeRes = cashfreeML.CreateVPA(new SmartCollectCreateCustomerRequest
                                {
                                    Contact = smartDetailList.MobileNo,
                                    EmailID = smartDetailList.EmailID,
                                    Name = smartDetailList.Name,
                                    UserID = UserID
                                });
                                if (cashfreeRes.Statuscode == ErrorCodes.One)
                                {
                                    if (!string.IsNullOrEmpty(cashfreeRes.VPA))
                                    {
                                        var updateReq = new UpdateSmartCollectRequestModel
                                        {
                                            CustomerID = cashfreeRes.CustomerID,
                                            LoginID = LoginID,
                                            UserID = UserID,
                                            SmartAccountNo = String.Empty,
                                            SmartCollectTypeID = SmartCollectType.CashfreeSmartCollect,
                                            SmartQRShortURL = String.Empty,
                                            SmartVPA = cashfreeRes.VPA ?? string.Empty
                                        };

                                        IProcedure procUpdate = new ProcUpdateCustomerSmartAccountDetail(_dal);
                                        res = (ResponseStatus)procUpdate.Call(updateReq);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        #region PrivateMethods
        private void CashfreeHelper(UserSmartDetailModel userSmartDetail, SmartCollectViewModel smartCollectViewModel, bool IsCallupdate)
        {
            if (userSmartDetail.USDList != null)
            {
                if (userSmartDetail.USDList.Count > 0)
                {
                    if (userSmartDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.CashfreeSmartCollect).Count() > 0)
                    {
                        var userSDetail = userSmartDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.CashfreeSmartCollect).ToList()[0];
                        IsCallupdate = userSDetail == null || string.IsNullOrEmpty((userSDetail ?? new UserSmartDetail()).SmartAccountNo);
                        if (IsCallupdate == false)
                        {
                            var cashfreeList = userSmartDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.CashfreeSmartCollect).ToList();
                            var smartDataList = new List<SmartCollectData>();
                            foreach (var item in cashfreeList)
                            {
                                var smartData = new SmartCollectData
                                {
                                    QR = string.Empty,
                                    Account = string.Empty,
                                    AccountHolder = string.Empty,
                                    BankName = string.Empty,
                                    IFSC = string.Empty,
                                    VPA = string.Empty
                                };
                                if (smartCollectViewModel.cashfreeCollectData.IsVirtualShow)
                                {
                                    smartData.BankName = userSDetail.BankName;
                                    smartData.AccountHolder = "Cashfree";
                                    smartData.Account = userSDetail.SmartAccountNo;
                                    smartData.IFSC = userSDetail.IFSC;
                                }
                                IsCallupdate = string.IsNullOrEmpty((userSDetail ?? new UserSmartDetail()).SmartVPA);
                                if (smartCollectViewModel.cashfreeCollectData.IsVPAShow && IsCallupdate == false)
                                {
                                    smartData.VPA = userSDetail.SmartVPA;
                                }
                                if (smartCollectViewModel.cashfreeCollectData.IsQRShow)
                                {
                                    smartData.QR = userSDetail.SmartQRShortURL;
                                }
                                smartDataList.Add(smartData);
                            }
                            smartCollectViewModel.cashfreeCollectData.data = smartDataList;
                        }
                    }
                }
            }
        }
        private void RazorpayHelper(UserSmartDetailModel userSmartDetail, SmartCollectViewModel smartCollectViewModel, bool IsCallupdate)
        {
            if (userSmartDetail.USDList != null)
            {
                if (userSmartDetail.USDList.Count > 0)
                {
                    if (userSmartDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).Count() > 0)
                    {
                        var userSDetail = userSmartDetail.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).ToList()[0];
                        IsCallupdate = userSDetail == null || string.IsNullOrEmpty((userSDetail ?? new UserSmartDetail()).SmartAccountNo);
                        if (IsCallupdate == false)
                        {
                            var smartData = new SmartCollectData
                            {
                                QR = string.Empty,
                                Account = string.Empty,
                                AccountHolder = string.Empty,
                                BankName = string.Empty,
                                IFSC = string.Empty,
                                VPA = string.Empty
                            };
                            if (smartCollectViewModel.razorpayCollectData.IsVirtualShow)
                            {
                                smartData.BankName = userSDetail.BankName;
                                smartData.AccountHolder = "Razorpay";
                                smartData.Account = userSDetail.SmartAccountNo;
                                smartData.IFSC = userSDetail.IFSC;
                            }
                            if (smartCollectViewModel.razorpayCollectData.IsVPAShow)
                            {
                                smartData.VPA = userSDetail.SmartVPA;
                            }
                            if (smartCollectViewModel.razorpayCollectData.IsQRShow)
                            {
                                smartData.QR = userSDetail.SmartQRShortURL;
                            }
                            smartCollectViewModel.razorpayCollectData.data = smartData;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
