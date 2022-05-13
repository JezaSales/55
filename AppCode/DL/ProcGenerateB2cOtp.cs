using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGenerateB2cOtp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGenerateB2cOtp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@RefrenceID",req.CommonInt),
                new SqlParameter("@OTP",req.CommonStr),
                new SqlParameter("@UserID",req.CommonInt3)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0]["StatusCode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["StatusCode"]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["Msg"]);
                    res.CommonInt = dt.Rows[0]["RefrenceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["RefrenceID"]);
                   // res.CommonInt2 = dt.Rows[0]["Otp"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Otp"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId =0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GenerateB2cOtp";
    }
}
