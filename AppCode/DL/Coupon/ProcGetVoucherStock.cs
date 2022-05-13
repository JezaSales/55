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
    public class ProcGetVoucherStock : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVoucherStock(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CoupanVoucher req = (CoupanVoucher)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            List<VStock> res = new List<VStock>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new VStock
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Amount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                            VoucherID = row["_VoucherID"] is DBNull ? 0 : Convert.ToInt32(row["_VoucherID"]),
                            VoucherType = row["_VoucherType"] is DBNull ? null : Convert.ToString(row["_VoucherType"]),
                            TotalCount = row["_TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["_TotalCount"]),
                            Remain = row["_remain"] is DBNull ? 0 : Convert.ToInt32(row["_remain"]),
                        };
                        res.Add(data);
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
                return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_VoucherStock";
    }
}
