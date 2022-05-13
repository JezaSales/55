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
    public class ProcGetPaymentGateway : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPaymentGateway(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@PGID", req.CommonInt)
            };
            var res = new PaymentGatewayEntity
            {
                PGateways = new List<PaymentGateway>(),
                MASTERPGateways = new List<MasterPaymentGateway>()
            };
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        if (!dt.Columns.Contains("Msg"))
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                res.PGateways.Add(new PaymentGateway
                                {
                                    ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                    EntryBy = row["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(row["_EntryBy"]),
                                    ModifyBy = row["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(row["_ModifyBy"]),
                                    PGID = row["_PGID"] is DBNull ? 0 : Convert.ToInt32(row["_PGID"]),
                                    Name = row["_Name"] is DBNull ? string.Empty : Convert.ToString(row["_Name"]),
                                    MerchantID = row["_MerchantID"] is DBNull ? string.Empty : Convert.ToString(row["_MerchantID"]),
                                    MerchantKey = row["_MerchantKey"] is DBNull ? string.Empty : Convert.ToString(row["_MerchantKey"]),
                                    EntryDate = row["_EntryDate"] is DBNull ? string.Empty : Convert.ToString(row["_EntryDate"]),
                                    ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : Convert.ToString(row["_ModifyDate"]),
                                    IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                });
                            }
                        }
                    }
                    var dt2 = ds.Tables[1];
                    if (dt2.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt2.Rows)
                        {
                            res.MASTERPGateways.Add(new MasterPaymentGateway
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Name = row["_Name"] is DBNull ? string.Empty : Convert.ToString(row["_Name"])
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetPaymentGateway";
    }
}
