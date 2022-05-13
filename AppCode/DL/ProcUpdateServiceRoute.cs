using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateServiceRoute : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateServiceRoute(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@TransactAPIType", _req.CommonInt),
                new SqlParameter("@TransactAPIPriority", _req.CommonInt2)
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateServiceRoute";
    }


    public class ProcGetUserServiceRoute : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserServiceRoute(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID)
            };
            var _res = new Serviceroute
            {
               TransactAPIType=0,
               TransactAPIPriority=0,
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    _res.TransactAPIType = dt.Rows[0]["_TransactAPIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_TransactAPIType"]);
                    _res.TransactAPIPriority = dt.Rows[0]["_TransactAPIPriority"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_TransactAPIPriority"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserServiceRoute";
    }
}
