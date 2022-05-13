using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{


    public class ProcSaveICICIStatement : IProcedureAsync
    {
        private IDAL _dal;

        public ProcSaveICICIStatement(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var req = (PostStatetmentRequest)obj;
            var Statement = req?.data.Select(x => new
            {
                AccountNo = req.AccountNo,
                TransactionId = x.TransactionId,
                ValueDate = x.ValueDate,
                TransactionDate = x.TransactionDate,
                TransactionPostedDate = x.TransactionPostedDate,
                ChequeNoRefNo = x.ChequeRefNo,
                TransactionRemarks = x.TransactionRemarks,
                TransactionAmount = x.TransactionAmount,
                TransactionType = x.TransactionType,
                AvailableBalance = x.AvailableBalance,
                UTR = x.UTR,
                UserName = x.UserName
            }).ToList();
            DataTable dataTable = Statement.ToDataTable();        
            SqlParameter[] param = {
                new SqlParameter("@AccountNo", req.AccountNo),
                new SqlParameter("@Record", dataTable)
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_SaveICICIStatement";

    }
}

