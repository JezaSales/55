using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Issuance;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.IDFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class IssuanceML: IIssuanceML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IRequestInfo _info;
        public IssuanceML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor != null ? _accessor.HttpContext.Session : null;
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }
        public IssuanceOTPResponse GenerateOTP(IssuanceOTPRequest issuanceOTPRequest)
        {
            var res = new IssuanceOTPResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            var CheckOutLetStatusResp = (ValidateAPIOutletResp)_proc.Call(new CommonReq
            {
                LoginID = issuanceOTPRequest.UserID,
                CommonInt = issuanceOTPRequest.OutletID,
                CommonInt2 = issuanceOTPRequest.OID,
                CommonStr = issuanceOTPRequest.SPKey
            });
            if (CheckOutLetStatusResp.Statuscode == ErrorCodes.Minus1)
            {
                res.Msg = CheckOutLetStatusResp.Msg;
                return res;
            }
            if (string.IsNullOrEmpty(CheckOutLetStatusResp.APICode))
            {
                res.Msg = ErrorCodes.Down;
                return res;
            }
            issuanceOTPRequest.TransactionID = CheckOutLetStatusResp.RequestID;
            if (CheckOutLetStatusResp.APICode == APICode.IDFC)
            {
                IIssuanceAPIML issuanceAPIML = new IDFCML(_accessor, _env, _dal);
                res = issuanceAPIML.GenerateOTP(issuanceOTPRequest, SaveAPILog);
            }
            return res;
        }
        public IssuanceMatchOTPResponse VerifyOTP(IssuanceOTPRequest issuanceOTPRequest)
        {
            var res = new IssuanceMatchOTPResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            var CheckOutLetStatusResp = (ValidateAPIOutletResp)_proc.Call(new CommonReq
            {
                LoginID = issuanceOTPRequest.UserID,
                CommonInt = issuanceOTPRequest.OutletID,
                CommonInt2 = issuanceOTPRequest.OID,
                CommonStr = issuanceOTPRequest.SPKey
            });
            if (CheckOutLetStatusResp.Statuscode == ErrorCodes.Minus1)
            {
                res.Msg = CheckOutLetStatusResp.Msg;
                return res;
            }
            if (string.IsNullOrEmpty(CheckOutLetStatusResp.APICode))
            {
                res.Msg = ErrorCodes.Down;
                return res;
            }
            if (CheckOutLetStatusResp.APICode == APICode.IDFC)
            {
                IIssuanceAPIML issuanceAPIML = new IDFCML(_accessor, _env, _dal);
                res = issuanceAPIML.VerifyOTP(new IssuanceOTPRequest
                {
                    APIOutletID = issuanceOTPRequest.APIOutletID,
                    CustomerID = issuanceOTPRequest.CustomerID,
                    MobileNo = issuanceOTPRequest.MobileNo,
                    OTP = issuanceOTPRequest.OTP,
                    OutletID = issuanceOTPRequest.OutletID,
                    ReffrenceID = issuanceOTPRequest.ReffrenceID,
                    TransactionID = CheckOutLetStatusResp.RequestID,
                    VRN = issuanceOTPRequest.VRN,
                    UserID = issuanceOTPRequest.UserID
                }, SaveAPILog);
            }
            return res;
        }

        public IssuanceCustomerOnboardingResponse CustomerOnboarding(IssuanceCustomerOnboardingRequest onboardingRequest)
        {
            var res = new IssuanceCustomerOnboardingResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            var CheckOutLetStatusResp = (ValidateAPIOutletResp)_proc.Call(new CommonReq
            {
                LoginID = onboardingRequest.UserID,
                CommonInt = onboardingRequest.OutletID,
                CommonInt2 = onboardingRequest.OID,
                CommonStr = onboardingRequest.SPKey
            });
            if (CheckOutLetStatusResp.Statuscode == ErrorCodes.Minus1)
            {
                res.Msg = CheckOutLetStatusResp.Msg;
                return res;
            }
            if (string.IsNullOrEmpty(CheckOutLetStatusResp.APICode))
            {
                res.Msg = ErrorCodes.Down;
                return res;
            }
            if (CheckOutLetStatusResp.APICode == APICode.IDFC)
            {
                IIssuanceAPIML issuanceAPIML = new IDFCML(_accessor, _env, _dal);
                res = issuanceAPIML.CustomerOnBoarding(onboardingRequest, SaveAPILog);
            }
            return res;
        }

        

        public int SaveAPILog(string request, string response)
        {
            IProcedure proc = new Proc_LogIssuanceAPIReqResp(_dal);
            proc.Call(new CommonReq
            {
                CommonStr = request,
                CommonStr1 = response
            });
            return 0;
        }
    }
}
