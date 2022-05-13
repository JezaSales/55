using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCouponVoucherList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCouponVoucherList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CoupanVoucher req = (CoupanVoucher)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@VoucherID",req.ID)
            };
            List<CoupanVoucher> res = new List<CoupanVoucher>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0 && req.CommonInt == -1)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        res.Add(new CoupanVoucher
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            VoucherID = row["_VoucherID"] is DBNull ? 0 : Convert.ToInt32(row["_VoucherID"]),
                            CouponCode = row["_CouponCode"] is DBNull ? "" : Convert.ToString(row["_CouponCode"]),
                            Amount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                            ApiName = row["_Name"] is DBNull ? "" : Convert.ToString(row["_Name"]),
                            APIID = row["_APIID"] is DBNull ? "0" : Convert.ToString(row["_APIID"]),
                            IsSale = row["_IsSale"] is DBNull ? false : Convert.ToBoolean(row["_IsSale"]),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : Convert.ToDateTime(row["_EntryDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : Convert.ToDateTime(row["_ModifyDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt")
                        });
                    }
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
            if (req.CommonInt == -1)
                return res;
            else
                return new CoupanVoucher();
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetCouponVoucherList";
    }
}
