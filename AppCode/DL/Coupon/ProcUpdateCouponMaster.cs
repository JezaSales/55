using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateCouponMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCouponMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CoupanReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.coupanMaster.OID),
                new SqlParameter("@VoucherType",req.coupanMaster.VoucherType),
                new SqlParameter("@Remark",req.coupanMaster.Remark),
                new SqlParameter("@ID",req.coupanMaster.ID),
                new SqlParameter("@MinAmount",req.coupanMaster.Min),
                new SqlParameter("@MaxAmount",req.coupanMaster.Max),
                new SqlParameter("@IsActive",req.coupanMaster.IsActive)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateCouponMaster";
    }
}
