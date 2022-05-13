using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Reflection;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.DB;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveAPIResponse : IProcedureAsync
    {
        private IDAL _dal;

        public ProcSaveAPIResponse(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var req = (APIResponseModel)obj;
                SqlParameter[] param ={
                    new SqlParameter("@Response",req.Response.ToList().ToDataTable()),
                    new SqlParameter("@APIID",req.APIID)
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

        public string GetName() => @"proc_SaveAPIResponse";
    }
}
