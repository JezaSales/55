using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Fintech.AppCode.DB;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGenrateAfLink : IProcedureAsync
    {
        private IDAL _dal;

        public ProcGenrateAfLink(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var req = (CommonReq)obj;
                SqlParameter[] param ={
                    new SqlParameter("@LoginId",req.LoginID),
                    new SqlParameter("@Id",req.CommonInt)
                };
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt != null && dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
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
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => @"Proc_GenrateAfLink";
    }
}
