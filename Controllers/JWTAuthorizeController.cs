using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiController]
    [Route("userauth/")]
    public class JWTAuthorizeController : Controller
    {

        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public JWTAuthorizeController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }

        /// <summary>
        /// This Service is used to generate a JWT Token.
        /// Note: All API requests must be authenticated with a JWT Token in the request.
        /// </summary>
        /// <param name="usersdata"></param>
        /// <returns>{"statusCode": 1,"msg": "Token Generated Successfully.","token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIiLCJ0b2tlbiI6ImRmOThmMjYxOTUxZmMzYzgyMTc2NTFlNjBjMTBjNTk2IiwibmJmIjoxNjQ3ODQ4NjYwLCJleHAiOjE2NDg0NTM0NjAsImlhdCI6MTY0Nzg0ODY2MH0.PIVO37RZCdn31_w_-VD-EBfdfcAhaSDtR3kXeSlw5KY" }</returns>
        [HttpPost]
        [Route("getToken")]
        public IActionResult Authenticate(JWTReqUsers usersdata)
        {
            var tokenResp = new JWTTokensResp
            {
                StatusCode = -1,
                Msg = ErrorCodes.AnError
            };
            var userMl = new UserML(_accessor, _env);
            var resp = userMl.CheckUserByUIDandToken(usersdata);
            if(resp.Statuscode == ErrorCodes.Minus1)
                return Unauthorized();

            var jWTTokenAuth_ML = new JWTTokenAuth_ML(_accessor, _env);
            var jWTToken = jWTTokenAuth_ML.GenerateJwtToken(usersdata);

            if (string.IsNullOrEmpty(jWTToken))
                return Unauthorized();
            else if (jWTToken.Contains("Wrong Key"))
                return Unauthorized();

            tokenResp.StatusCode = 1;
            tokenResp.Msg = "Token Generated Successfully.";
            tokenResp.Token = jWTToken;

            return Json(tokenResp);
        }


        ///// <summary>
        ///// This is Testing.
        ///// </summary>
        ///// <param name="usersdata"></param>
        ///// <returns>{"statusCode": 1,"msg": "Token Generated Successfully.","token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjIiLCJ0b2tlbiI6ImRmOThmMjYxOTUxZmMzYzgyMTc2NTFlNjBjMTBjNTk2IiwibmJmIjoxNjQ3ODQ4NjYwLCJleHAiOjE2NDg0NTM0NjAsImlhdCI6MTY0Nzg0ODY2MH0.PIVO37RZCdn31_w_-VD-EBfdfcAhaSDtR3kXeSlw5KY" }</returns>
        //[HttpPost]
        //[Route("getAuth")]
        //public JWTTokensResp AuthenticateService(JWTReqUsers usersdata)
        //{
        //    var tokenResp = new JWTTokensResp
        //    {
        //        StatusCode = -1,
        //        Msg = ErrorCodes.AnError
        //    };
        //    return tokenResp;
        //}

    }
}
