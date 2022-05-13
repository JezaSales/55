using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetB2CMemberTypeByMember : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetB2CMemberTypeByMember(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            var res = new PackageDetailB2CWithMemberShip
            {
                membershipmasterB2Cs = new List<MembershipmasterB2C>(),
                //
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 1)
                {
                    DataTable dt = ds.Tables[0];
                    DataTable dt2 = ds.Tables[1];
                    if (dt.Rows.Count > 0)
                    {
                        if (!dt.Columns.Contains("Msg"))
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var slabid = row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"]);
                                var slabdetailServicew = new List<SlabdetailServicewise>();
                                foreach (DataRow rows in dt2.Rows)
                                {
                                    slabdetailServicew.Add(new SlabdetailServicewise
                                    {
                                        Name = rows["_Service"] is DBNull ? string.Empty : rows["_Service"].ToString(),
                                        OpType = rows["_OpType"] is DBNull ? string.Empty : rows["_OpType"].ToString(),
                                        Comm = rows["_Comm"] is DBNull ? 0 : Convert.ToDecimal(rows["_Comm"]),
                                        CommType = rows["_CommType"] is DBNull ? false : Convert.ToBoolean(rows["_CommType"]),
                                        AmtType = rows["_AmtType"] is DBNull ? false : Convert.ToBoolean(rows["_AmtType"]),
                                        SlabID = rows["_SlabID"] is DBNull ? 0 : Convert.ToInt32(rows["_SlabID"]),
                                    });
                                }
                                var lst = from x in slabdetailServicew where x.SlabID == slabid select x;
                                res.membershipmasterB2Cs.Add(new MembershipmasterB2C
                                {
                                    IsIDActive = row["_IsIDActive"] is DBNull ? false : Convert.ToBoolean(row["_IsIDActive"]),
                                    IsExpireSoon = row["_IsExpireSoon"] is DBNull ? false : Convert.ToBoolean(row["_IsExpireSoon"]),
                                    IsCouponAllowed = row["_IsCouponAllowed"] is DBNull ? false : Convert.ToBoolean(row["_IsCouponAllowed"]),
                                    ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                    MemberShipType = row["_MemberShipType"] is DBNull ? string.Empty : row["_MemberShipType"].ToString(),
                                    Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                    CouponCount = row["_CouponCount"] is DBNull ? 0 : Convert.ToInt32(row["_CouponCount"]),
                                    CouponValue = row["_CouponValue"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValue"]),
                                    CouponValidityDays = row["_CouponValidityDays"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValidityDays"]),
                                    Cost = row["_Cost"] is DBNull ? 0 : Convert.ToDecimal(row["_Cost"]),
                                    slabdetailServicewises = lst.ToList(),
                                });
                            }
                        }
                    }
                    // DataTable dt2 = ds.Tables[1];
                    //if (dt2.Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in dt.Rows)
                    //    {
                    //        res.slabdetailServicewises.Add(new SlabdetailServicewise
                    //        {
                    //            Name = row["_Service"] is DBNull ? string.Empty : row["_Service"].ToString(),
                    //            OpType = row["_OpType"] is DBNull ? string.Empty : row["_OpType"].ToString(),
                    //            Comm = row["_Comm"] is DBNull ? 0 : Convert.ToDecimal(row["_Comm"]),
                    //            CommType = row["_CommType"] is DBNull ? false : Convert.ToBoolean(row["_CommType"]),
                    //            AmtType = row["_AmtType"] is DBNull ? false : Convert.ToBoolean(row["_AmtType"]),
                    //        });
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBalance",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetB2CMemberTypeByMember";
    }
    public class ProcGetb2cTargetAchievedTillDate : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetb2cTargetAchievedTillDate(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@IsTotal",req.IsListType)
               
            };
            var res = new List<B2cTarget>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dt.Rows[0][0]) != ErrorCodes.Minus1)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            var data = new B2cTarget
                            {
                                MembershipType = item["_MembershipType"] is DBNull ? string.Empty : item["_MembershipType"].ToString(),
                                ServiceName = item["_Service"] is DBNull ? string.Empty : item["_Service"].ToString(),
                       
                                SlabID = item["_SlabID"] is DBNull ? 0 : Convert.ToInt32(item["_SlabID"]),
                                OID = item["_OID"] is DBNull ? 0 : Convert.ToInt32(item["_OID"]),
                                RoleID = item["_RoleID"] is DBNull ? 0 : Convert.ToInt32(item["_RoleID"]),
                                Target = item["_Target"] is DBNull ? 0 : Convert.ToInt32(item["_Target"]),
                                Achieved = item["_Achieved"] is DBNull ? 0 : Convert.ToInt32(item["_Achieved"]),
                                Pending = item["_Pending"] is DBNull ? 0 : Convert.ToInt32(item["_Pending"]),
                                IsGift = item["_IsGift"] is DBNull ? false : Convert.ToBoolean(item["_IsGift"]),
                                IsEarned = item["_IsEarned"] is DBNull ? false : Convert.ToBoolean(item["_IsGift"])
                            };
                            string[] ext = { ".png", ".jpg", ".jpeg" };
                            foreach (string s in ext)
                            {
                                string fileName = "Gift_" + data.RoleID + "_" + data.OID + "_" + data.SlabID + s;
                                string file = DOCType.GiftImgPath + fileName;
                                if (File.Exists(file))
                                {
                                    data.ImagePath = "/Image/GiftImage/" + fileName;
                                    break;
                                }
                            }
                            res.Add(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_Getb2cTargetAchievedTillDate";
    }
}
