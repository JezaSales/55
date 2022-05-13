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
    public class ProcGetAffiliatedVendors : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetAffiliatedVendors(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj) {
            int id = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@id",id)
            };
            List<AffiliateVendors> res = new List<AffiliateVendors>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var vendor = new AffiliateVendors
                        {
                            Id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            VendorName = row["_VendorName"] is DBNull ? "" : row["_VendorName"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            LastUpdatedOn = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),

                        //    VendorIcon = row["_VendorIcon"] is DBNull ? "" : row["_VendorIcon"].ToString(),

                            VendorBanner = row["_VendorBanner"] is DBNull ? "" : row["_VendorBanner"].ToString(),
                        };
                        res.Add(vendor);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetAffiliatedVendors";
    }
}