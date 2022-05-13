using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSlabdetailServicewise : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetSlabdetailServicewise(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            int SlabId = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@SlabId",SlabId),
                new SqlParameter("@B2CCommDisplayType",ApplicationSetting.B2CDisplayCommType)
            };
            var _alist = new List<SlabdetailServicewise>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow row in dt.Rows)
                {
                    _alist.Add(new SlabdetailServicewise
                    {
                        ServiceID = row["_ServiceID"] is DBNull ? 0 : Convert.ToInt32(row["_ServiceID"]),
                        Name = row["_Name"] == null ? "" : row["_Name"].ToString(),
                        OpTypeID = row["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(row["_OpTypeID"]),
                        Comm = row["_Comm"] is DBNull ? 0 : Convert.ToDecimal(row["_Comm"]),
                        CommType = row["_CommType"] is DBNull ? false : Convert.ToBoolean(row["_CommType"]),
                        AmtType = row["_AmtType"] is DBNull ? false : Convert.ToBoolean(row["_AmtType"]),
                        SlabID = row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"])

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

        public string GetName() => "proc_GetSlabdetailServicewise";
    }
}


