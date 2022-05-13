using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBlogdata : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetBlogdata(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", req)
            };
            var _alist = new List<Blog>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow row in dt.Rows)
                {
                    _alist.Add(new Blog
                    {
                        ID = Convert.ToInt32(row["_ID"]),
                        Tittle = row["_Tittle"] == null ? "" : row["_Tittle"].ToString(),
                        ContentDetails = row["_Content"] == null ? "" : row["_Content"].ToString(),
                        CreateDate = row["_EntryDate"] == null ? "" : row["_EntryDate"].ToString()                       
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetBlogs";
    }
}


