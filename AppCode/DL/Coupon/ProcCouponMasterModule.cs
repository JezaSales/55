using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCouponMasterModule : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCouponMasterModule(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ID",req.CommonInt)
            };
            var res = new List<CoupanMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            res.Add(new CoupanMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                VoucherType = row["_VoucherType"] is DBNull ? "" : row["_VoucherType"].ToString(),
                                OID = row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                                OpName = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                LastModifyDate = Convert.ToDateTime(row["_ModifyDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                                Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                                Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                                IsActive = row["_IsActive"] is DBNull ? true : Convert.ToBoolean(row["_IsActive"]),
                            });
                        }
                    }
                    else
                    {
                        return new CoupanMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            VoucherType = dt.Rows[0]["_VoucherType"] is DBNull ? "" : dt.Rows[0]["_VoucherType"].ToString(),
                            OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]),
                            OpName = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
                            Max = dt.Rows[0]["_Max"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Max"]),
                            Min = dt.Rows[0]["_Min"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Min"]),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? true : Convert.ToBoolean(dt.Rows[0]["_IsActive"])
                        };
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
                return new CoupanMaster() { Min = 0, Max = 0 };
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetCouponMaster";
    }
}
