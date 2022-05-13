using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ICollect;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcICollectValidationEntry : IProcedure
    {
        private readonly IDAL _dal;
        public ProcICollectValidationEntry(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ICollectRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UTR",req.UTR??string.Empty),
                new SqlParameter("@Amount",req.Amount??"0"),
                new SqlParameter("@CorporateCode",req.CorporateCode??string.Empty),
                new SqlParameter("@PaymentMode",req.PaymentMode??string.Empty),
                new SqlParameter("@Account",req.Account??string.Empty),
                new SqlParameter("@IMPSAccount",req.IMPSAccount??string.Empty),
                new SqlParameter("@AccountName",req.AccountName??string.Empty),
                new SqlParameter("@IMPSAccountName",req.IMPSAccountName??string.Empty),
                new SqlParameter("@IFSC",req.IFSC??string.Empty)
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
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
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
                    UserId = 1
                });
            }
            return res;

        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_ICollectValidationEntry";
    }
}
