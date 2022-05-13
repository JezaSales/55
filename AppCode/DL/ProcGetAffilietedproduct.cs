using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System.Globalization;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAffilietedproduct : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAffilietedproduct(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new List<AffiliatedItem>();
            var response = new AffiliateItemModal();
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@vendorID",req.CommonInt),
                new SqlParameter("@id",req.CommonInt2),
                new SqlParameter("@OID",req.CommonInt3),
                new SqlParameter("@IsForVendor",req.CommonBool),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!req.CommonBool)
                    {
                        if (Convert.ToInt32(dt.Rows[0][0]) == 1)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                var data = new AffiliatedItem
                                {
                                    ID = Convert.ToInt32(dr["_ID"]),
                                    VendorID = Convert.ToInt32(dr["_VendorID"]),
                                    Link = Convert.ToString(dr["_Link"]),
                                    ImgUrl = Convert.ToString(dr["_ImgUrl"]),
                                    ImageURL = Convert.ToString(dr["_ImgUrl"]),
                                    IsActive = Convert.ToBoolean(dr["_ISActive"]),
                                    LinkType = Convert.ToInt32(dr["_LinkType"]),
                                    Name = Convert.ToString(dr["_Name"]),
                                    OID =Convert.ToInt32(dr["_OID"]),
                                    IsImageURL = Convert.ToBoolean(dr["_IsImageURL"])
                                };
                                res.Add(data);
                            }
                        }                        
                    }
                    else
                    {
                        if (Convert.ToInt32(dt.Rows[0][0]) == 1)
                        {
                            response.ID = Convert.ToInt32(dt.Rows[0]["_ID"], CultureInfo.InvariantCulture);
                            response.VendorID = Convert.ToInt32(dt.Rows[0]["_VendorID"], CultureInfo.InvariantCulture);
                            response.Link = Convert.ToString(dt.Rows[0]["_Link"], CultureInfo.InvariantCulture);
                            response.ImgUrl = Convert.ToString(dt.Rows[0]["_ImgUrl"], CultureInfo.InvariantCulture);
                            response.ImageURL = Convert.ToString(dt.Rows[0]["_ImgUrl"], CultureInfo.InvariantCulture);
                            response.IsActive = Convert.ToBoolean(dt.Rows[0]["_ISActive"], CultureInfo.InvariantCulture);
                            response.LinkType = Convert.ToInt32(dt.Rows[0]["_LinkType"], CultureInfo.InvariantCulture);
                            response.OID = Convert.ToInt32(dt.Rows[0]["_OID"], CultureInfo.InvariantCulture);
                        }
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
            if (!req.CommonBool)
                return res;
            else
                return response != null ? response : new AffiliateItemModal();
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAllproduct";
    }
}
