using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCouponStock : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetCouponStock(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            CoupanVoucherReq req = (CoupanVoucherReq)obj ?? new CoupanVoucherReq();
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginId),
                new SqlParameter("@MasterVId",req.MasterVId)
            };
            List<CouponstockDetail> res = new List<CouponstockDetail>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param).ConfigureAwait(true);
                foreach (DataRow row in dt.Rows)
                {
                    res.Add(new CouponstockDetail
                    {
                        CouponAmount = row["_amount"] is DBNull ? 0 : Convert.ToDecimal(row["_amount"]),
                        Stock = row["_inStock"] is DBNull ? 0 : Convert.ToInt32(row["_inStock"])
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeId,
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res ?? new List<CouponstockDetail>();
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => @"select  _Amount into #temp from tbl_CouponVoucher(nolock) where _VoucherID=@MasterVId  group by _Amount
                                     select _Amount,count(1) _inStock into #temp2 from tbl_CouponVoucher(nolock) where _VoucherID=@MasterVId and ISNULL(_IsSale,0)=0  group by _Amount 
                                     select t1._Amount,ISNULL(t2._inStock,0) _inStock from #temp t1 Left join #temp2 t2 on t1._Amount = t2._Amount";
    }
}
