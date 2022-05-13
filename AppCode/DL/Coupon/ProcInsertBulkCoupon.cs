using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcInsertBulkCoupon : IProcedureAsync
    {
        private IDAL _dal;

        public ProcInsertBulkCoupon(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var _req = (CoupanVoucherEXlReq)obj;
            DataTable dataTable = _req.Couponvocher.ToDataTable();
            SqlParameter[] param = {
                new SqlParameter("@dataTable", dataTable),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@VocherID", _req.VocherID),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_InsertCouponVocher";
    }
}
