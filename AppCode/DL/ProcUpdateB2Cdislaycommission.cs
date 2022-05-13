using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateB2Cdislaycommission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateB2Cdislaycommission(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (SlabdetailServicewise)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@SlabID", req.SlabID),
                new SqlParameter("@ServiceID", req.ServiceID),
                new SqlParameter("@OpTypeID", req.OpTypeID),
                new SqlParameter("@Comm", req.Comm),
                new SqlParameter("@CommType", req.CommType),
                new SqlParameter("@AmtType", req.AmtType),
                new SqlParameter("@EntryBy", req.EntryBy),
                new SqlParameter("@ModifyBy", req.ModifyBy)
                           
            };
            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateB2Cdislaycommission";

    }
}


