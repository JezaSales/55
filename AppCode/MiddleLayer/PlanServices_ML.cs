using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.DL;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PlanServices_ML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConnectionConfiguration _c;
        private readonly IDAL _dal;
        private readonly string _SPKey;
        private readonly string _Token;
        private readonly int _userId;
        private int _OID;
        private PlansAPIML _plansAPIML;

        public PlanServices_ML(IHttpContextAccessor accessor, IHostingEnvironment env, string spkey, string token, int userid)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _SPKey = spkey;
            _Token = token;
            _plansAPIML = new PlansAPIML(_accessor, _env);
            _userId = userid;
        }
        #region LogReqRespPlans
        public void LogPlansServicesReqResp(PlanLogReq req)
        {
            IProcedureAsync proc = new ProcLogPlansServicesReqResp(_dal);
            proc.Call(req);
        }
        #endregion
        private PSValidateResp ValidatePlanServiceOperator()
        {
            var _res = new PSValidateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            IProcedure proc = new ProcValueAddedService(_dal);
            _res = (PSValidateResp)proc.Call(new PSValidateReq { 
                LoginId = _userId,
                RequestMode = RequestMode.API,
                SPKey = _SPKey,
                Token = _Token
            });
            return _res;
        }
        public RNPRoffer PSRoffer(string m)
        {
            var resp = new RNPRoffer
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp = _plansAPIML.GetRNPRoffer(m, vOp.OID);
            return resp;
        }
        public PSRechPResp PSRechPlan(int c,int LT,int UserID)
        {
            var resp = new PSRechPResp
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp.Data = _plansAPIML.AppSimplePlan(vOp.OID, c, LT, UserID);
            if (resp.Data != null)
            {
                resp.StatusCode = ErrorCodes.One;
                resp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                resp.StatusCode = ErrorCodes.Minus1;
                resp.Msg = ErrorCodes.FAILED;
                resp.Data = null;
            }
            return resp;
        }
        public PSRechPResp PSDTHPlan()
        {
            var resp = new PSRechPResp
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp.Data = _plansAPIML.AppDTHPlan(vOp.OID);
            if (resp.Data != null)
            {
                resp.StatusCode = ErrorCodes.One;
                resp.Msg = ErrorCodes.SUCCESS;
            }
            else
            {
                resp.StatusCode = ErrorCodes.Minus1;
                resp.Msg = ErrorCodes.FAILED;
                resp.Data = null;
            }
            return resp;
        }
        public RNPDTHCustInfo PSDTHCustInfo(string m)
        {
            var resp = new RNPDTHCustInfo
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp = _plansAPIML.GetRNPDTHCustInfo(vOp.OID,m);
            return resp;
        }
        public RNPDTHHeavyRefresh PSDTHHeavyRefresh(string m)
        {
            var resp = new RNPDTHHeavyRefresh
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp = _plansAPIML.GetRNPDTHHeavyRefresh(vOp.OID, m);
            return resp;
        }
        public PSHLRResponse PSMobileLookup(string m)
        {
            var resp = new PSHLRResponse
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var vOp = ValidatePlanServiceOperator();
            if (vOp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = vOp.Msg;
                return resp;
            }
            resp = _plansAPIML.CheckNumberSeriesExist(m, _SPKey,_userId);
            return resp;
        }

        public List<PSCircleCode> PSGetCircleCode()
        {
            return _plansAPIML.PSGetCircleCode();
        }
        public List<PSOperatorCode> PSGetOperatorCode()
        {
            return _plansAPIML.PSGetOperatorCode();
        }
    }
}
