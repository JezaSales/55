using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [JWTAuthorize]
    [ApiController]
    [Route("PlanServices/v1/")]
    public class PlanServicesController : Controller
    {

        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly JwtTokenSession jwtTokenSession;
        public PlanServicesController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            jwtTokenSession = (JwtTokenSession)_accessor.HttpContext.Items["User"];
        }
        /// <summary>
        /// R-Offer Service 
        /// Required Fields --> accountNo[Mobile No] and spkey[OPID of Operators]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="spkey"></param>
        /// <returns>
        /// 
        /// </returns>
        [HttpGet]
        [Route("BestOffer")]
        public PlansRespModel Roffer(string accountNo,string spkey)
        {
            var pResp = new PlansRespModel<RNPRoffer>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSRoffer(accountNo);
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = accountNo,
                SPKey = spkey,
                CircleId = string.Empty,
                Method = "Roffer",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// Recharge Plans of Mobile Operator
        /// Required Fields --> spkey[OPID of Operators] and circleId[circle code]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <param name="spkey"></param>
        /// <param name="circleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RechargePlan")]
        public PlansRespModel RechargePlan(string spkey, int circleId)
        {
            var pResp = new PlansRespModel<PSRechPResp>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSRechPlan(circleId,1, jwtTokenSession.UserID);
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = string.Empty,
                SPKey = spkey,
                CircleId = circleId.ToString(),
                Method = "RechargePlan",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// DTH Plans of DTH Operator
        /// Required Fields --> spkey[OPID of Operators]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <param name="spkey"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DTHPlan")]
        public PlansRespModel DTHPlan(string spkey)
        {
            
            var pResp = new PlansRespModel<PSRechPResp>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSDTHPlan();
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = string.Empty,
                SPKey = spkey,
                CircleId = string.Empty,
                Method = "DTHPlan",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// Customer Information of DTH Operator
        /// Required Fields --> accountNo[Mobile No] and spkey[OPID of Operators]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="spkey"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DTHCustomerInfo")]
        public PlansRespModel DTHCustomerInfo(string accountNo, string spkey)
        {
            var pResp = new PlansRespModel<RNPDTHCustInfo>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSDTHCustInfo(accountNo);
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = accountNo,
                SPKey = spkey,
                CircleId = string.Empty,
                Method = "DTHCustomerInfo",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// Heavy Refresh Service of DTH Operator
        /// Required Fields --> accountNo[Mobile No] and spkey[OPID of Operators]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization. 
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="spkey"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DTHHeavyRefresh")]
        public PlansRespModel DTHHeavyRefresh(string accountNo, string spkey)
        {
            
            var pResp = new PlansRespModel<RNPDTHHeavyRefresh>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSDTHHeavyRefresh(accountNo);
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = accountNo,
                SPKey = spkey,
                CircleId = string.Empty,
                Method = "DTHHeavyRefresh",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// HLR Service of Prepaid Operator
        /// Required Fields --> accountNo[Mobile No] and spkey[OPID of Operators]
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization. 
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="spkey"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MobileLookup")]
        public PlansRespModel MobileLookup(string accountNo, string spkey)
        {
            
            var pResp = new PlansRespModel<PSHLRResponse>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, spkey, jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSMobileLookup(accountNo);
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            #region Log
            plans.LogPlansServicesReqResp(new PlanLogReq
            {
                UserID = jwtTokenSession.UserID,
                UserToken = jwtTokenSession.UserToken,
                JWTToken = jwtTokenSession.UserJWTToken,
                AccountNo = accountNo,
                SPKey = spkey,
                CircleId = string.Empty,
                Method = "MobileLookup",
                Response = JsonConvert.SerializeObject(pResp),
                O1 = string.Empty,
                O2 = string.Empty
            });
            #endregion
            return pResp;
        }
        /// <summary>
        /// Get Circle Codes 
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCircleCodes")]
        public PlansRespModel GetCircleCodes()
        {
            var pResp = new PlansRespModel<List<PSCircleCode>>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, "", jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSGetCircleCode();
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            return pResp;
        }

        /// <summary>
        /// Get Operator Codes 
        /// Note: JWT Bearer Token is mandatory user need to send it under HTTP Header under Authorization.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetOperatorCodes")]
        public PlansRespModel GetOperatorCodes()
        {
            var pResp = new PlansRespModel<List<PSOperatorCode>>
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var plans = new PlanServices_ML(_accessor, _env, "", jwtTokenSession.UserToken, jwtTokenSession.UserID);
            pResp.Data = plans.PSGetOperatorCode();
            pResp.StatusCode = ErrorCodes.One;
            pResp.Msg = ErrorCodes.SUCCESS;
            return pResp;
        }
    }
}
