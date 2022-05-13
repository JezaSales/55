using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSmartCollectType : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSmartCollectType(IDAL dal) => _dal = dal;

        public object Call()
        {
            List<SmartCollect> res = new List<SmartCollect> { };
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new SmartCollect
                        {
                            Id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            SmartCollectTypeName = row["_SmartCollectType"] is DBNull ? "" : row["_SmartCollectType"].ToString(),
                            Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            IsVirtual = row["_IsVirtual"] is DBNull ? false : Convert.ToBoolean(row["_IsVirtual"]),
                            IsVPA = row["_IsUPI"] is DBNull ? false : Convert.ToBoolean(row["_IsUPI"]),
                            IsQR = row["_IsQR"] is DBNull ? false : Convert.ToBoolean(row["_IsQR"]),

                        };
                        res.Add(data);
                    }
                }
            }
            catch (Exception rx)
            {
            }
            return res;
        }

        public object Call(object obj) => throw new NotImplementedException();

        public string GetName() => "select _ID,_SmartCollectType,_Remark,_IsActive,_IsVirtual,_IsUPI,_IsQR from MASTER_Smart_Collect_Type where _IsAllowed=1";
    }
}
