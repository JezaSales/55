using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAddAffiliatedItems : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAddAffiliatedItems(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (AffiliatedItem)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@LoginID",req.LoginId),
                new SqlParameter("@vendorID",req.VendorID),
                new SqlParameter("@Link",req.Link??""),
                new SqlParameter("@ImgUrl",req.ImgUrl??""),
                new SqlParameter("@Tittle",req.Tittle??""),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@IsImageURL",req.IsImageURL),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@LinkType",req.LinkType),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@Description",req.Description)
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
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginId
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AddAffiliatedItems";
    }
}
