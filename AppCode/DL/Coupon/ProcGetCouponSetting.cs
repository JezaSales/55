using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCouponSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCouponSetting(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ID",req.CommonInt)
            };
            var res = new List<DenominationVoucher>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        res.Add(new DenominationVoucher
                        {
                            VoucherID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_VoucherID"]),
                            DenominationID = row["_DenomID"] is DBNull ? 0 : Convert.ToInt32(row["_DenomID"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"])
                        });
                    }
                }
                else
                {
                    return new DenominationVoucher
                    {
                        VoucherID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VoucherID"]),
                        DenominationID = dt.Rows[0]["_DenomID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DenomID"]),
                        IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"])
                    };
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
            if (req.CommonInt > 0)
                return res;
            else
                return new DenominationVoucher();
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetCouponSetting";
    }
}
