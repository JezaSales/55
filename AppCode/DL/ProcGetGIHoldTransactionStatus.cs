using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetGIHoldTransactionStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetGIHoldTransactionStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@TransactionID",req)
            };
            var res = new GIUpdateRequestModel { };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.HoldID = dt.Rows[0]["_ID"] is null ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.APICode = dt.Rows[0]["_APICode"] is null ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                    res.APIOpCode = dt.Rows[0]["_APIOpCode"] is null ? string.Empty : dt.Rows[0]["_APIOpCode"].ToString();
                    res.TransactionID = dt.Rows[0]["_TransactionID"] is null ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                    res.APIOutletID = dt.Rows[0]["_APIOutletID"] is null ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                    res.RechType = dt.Rows[0]["_RechType"] is null ? string.Empty : dt.Rows[0]["_RechType"].ToString();
                    res.VendorID = dt.Rows[0]["_VendorID"] is null ? string.Empty : dt.Rows[0]["_VendorID"].ToString();
                    res.ActualAmount = dt.Rows[0]["_ActualAmount"] is null ? 0M :Convert.ToDecimal(dt.Rows[0]["_ActualAmount"]);
                    res.AccountNo = dt.Rows[0]["_AccountNo"] is null ? string.Empty :dt.Rows[0]["_AccountNo"].ToString();
                    res.OutletID = dt.Rows[0]["_OutletID"] is null ? 0 :Convert.ToInt32(dt.Rows[0]["_OutletID"]);
                }
            }
            catch (System.Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    UserId = 0
                });
            }
            return res;
        }

        public object Call()
        {
            throw new System.NotImplementedException();
        }

        public string GetName() => "proc_GetGIHoldTransactionStatus";
    }
}
