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
    public class ProcGetDenominationVoucher : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationVoucher(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CoupanVoucher req = (CoupanVoucher)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            List<DenominationVoucher> res = new List<DenominationVoucher>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new DenominationVoucher
                        {
                            DenominationID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            DenminationAmount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
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

        public string GetName() => "proc_GetDenominationVoucher";
    }
}
