using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace Fintech.AppCode.DL
{
    public class ProcValueAddedService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValueAddedService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (PSValidateReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginId),
                new SqlParameter("@RequestMode", _req.RequestMode),
                new SqlParameter("@Token", _req.Token),
                new SqlParameter("@SPKey", _req.SPKey)
            };

            var _res = new PSValidateResp
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
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Msg = dt.Rows[0]["Msg"].ToString();
                        _res.OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        //_res.WhatsappNo = dt.Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : dt.Rows[0]["_WhatsappNo"].ToString();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.LoginId
                });
            }

            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ValueAddedService";
    }
}
