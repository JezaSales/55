using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AffiliatedItemViewModel = RoundpayFinTech.AppCode.Model.AffiliatedItemViewModel;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class AffiliationML : IAffiliationML
    {
        #region Gloabl Variables

        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        #endregion

        public AffiliationML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
        }

        public ResponseStatus SaveAffiliateVendor(string VendorName, string VendorBanner, int Id, bool IsActive)
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = VendorName,
                CommonStr2 = VendorBanner,
                CommonInt = Id,
                CommonBool = IsActive
            };
            IProcedure proc = new ProcSaveAffiliateVendor(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public AfVendorCommission GetAfCategoryCommission(int VendorID)
        {
            IProcedure proc = new ProcAfVendorCommission(_dal);
            var response = (IEnumerable<AffiliateVendorCommission>)proc.Call(VendorID);
            var res = new AfVendorCommission
            {
                Commissions = response,
                VendorID = VendorID
            };
            return res;
        }

        public IEnumerable<AffiliateVendors> GetAffiliatedVendors(int id)
        {
            IProcedureAsync proc = new ProcGetAffiliatedVendors(_dal);
            var res = (List<AffiliateVendors>)proc.Call(id).Result;
            return res;
        }

        public IEnumerable<OperatorDetail> GetAfCategories()
        {
            IProcedure proc = new ProcGetOperatorByService(_dal);
            var res = (IEnumerable<OperatorDetail>)proc.Call(new CommonReq { CommonStr = "AFS" });
            return res;
        }

        public ResponseStatus SaveAfVendorCateComm(int OID, int VendorID, decimal comm, int AmtType)
        {
            var req = new AffiliateVendorCommission
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                AmtType = AmtType,
                Commission = comm,
                OID = OID,
                CommonInt = VendorID
            };
            IProcedure proc = new ProcSaveAfVendorCateComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        //public ResponseStatus AddAffiliatedItems(int VendorID, string Link, string ImgUrl, bool IsActive, int ID, int LinkType, int OID, bool IsDel, bool IsImageURL, string Tittle)
        //{
        //    var req = new CommonReq
        //    {
        //        LoginID = _lr.UserID,
        //        LoginTypeID = _lr.LoginTypeID,
        //        CommonInt = VendorID,
        //        CommonStr = Link,
        //        CommonStr2 = ImgUrl,
        //        CommonStr3 = Tittle,
        //        CommonInt2 = ID,
        //        CommonBool = IsActive,
        //        CommonBool1 = IsImageURL,
        //        CommonInt3 = LinkType,
        //        CommonInt4 = OID,
        //        CommonBool2 = IsDel
        //    };
        //    IProcedure proc = new ProcAddAffiliatedItems(_dal);
        //    ResponseStatus response = (ResponseStatus)proc.Call(req);
        //    return response;
        //}

        public ResponseStatus AddAffiliatedItems(AffiliatedItem affiliatedItem, bool isDel = false)
        {
            affiliatedItem.LoginId = _lr.UserID;
            affiliatedItem.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcAddAffiliatedItems(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(affiliatedItem);
            return response;
        }

        public IEnumerable<AffiliatedItem> GetAllAfItems(int VendorID, int UserID, int LoginTypeID, int categoryId = 0)
        {
            var req = new CommonReq
            {
                LoginID = UserID,
                LoginTypeID = LoginTypeID,
                CommonInt = VendorID,
                CommonBool = false,
                CommonInt3 = categoryId
            };
            IProcedure proc = new ProcGetAllAfItems(_dal);
            var response = (IEnumerable<AffiliatedItem>)proc.Call(req);
            return response;
        }

        public async Task<AffiliatedItemViewModel> GetAllAffiliatedItems(int userId, int loginTypeId)
        {
            IProcedureAsync procVendor = new ProcGetAffiliatedVendors(_dal);
            var vendors = (IEnumerable<AffiliateVendors>)await procVendor.Call(0);
            IProcedure procOperator = new ProcGetOperator(_dal);
            var operators = (List<OperatorDetail>)procOperator.Call(new CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 43,
                LoginID = userId,
                LoginTypeID = loginTypeId
            });
            AffiliatedItemViewModel response = new AffiliatedItemViewModel
            {
                affiliatedOperator = operators.Select(x => new AffiliatedOperator { OID = x.OID, Name = x.Name }).ToList(),
                affiliatedVendor = vendors.Select(x => new AffiliatedVendor { VID = x.Id, Name = x.VendorName }).ToList()
            };
            return response;
        }

        public IEnumerable<AffiliatedItem> GetAffilietedproduct(int OID)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 1,
                CommonInt3 = OID,
                CommonBool = false
            };
            IProcedure proc = new ProcGetAffilietedproduct(_dal);
            var response = (IEnumerable<AffiliatedItem>)proc.Call(req);
            return response;
        }

        public IEnumerable<AffiliatedItem> GetAffilietedVproduct(int VID)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 1,
                CommonInt3 = VID,
                CommonBool = false
            };
            IProcedure proc = new ProcGetAffilietedVproduct(_dal);
            var response = (IEnumerable<AffiliatedItem>)proc.Call(req);
            return response;
        }




        public IEnumerable<AffiliatedItem> GetAfItems(int ID, int UserID, int LoginTypeID)
        {
            var req = new CommonReq
            {
                LoginID = UserID,
                LoginTypeID = LoginTypeID,
                CommonInt2 = ID,
                CommonBool = false
            };
            IProcedure proc = new ProcGetAfItemsLinks(_dal);
            var response = (IEnumerable<AffiliatedItem>)proc.Call(req);
            return response;
        }


        //===================================================================
        public AFItemsForDisplay GetAfItemsForDisplay()
        {
            var Afitems = GetAllAfItems(0, 0, 0);
            var Categories = GetAfCategories();
            var Vendors = GetAffiliatedVendors(0);
            var AfVendorWithCategories = new List<AfVendorWithCategories>();

            foreach (var v in Vendors)
            {
                var AfCategoriesWithItems = new List<AfCategoriesWithItems>();
                foreach (var cat in Categories)
                {
                    AfCategoriesWithItems.Add(new AfCategoriesWithItems
                    {
                        CategoryName = cat.Name,
                        CategoryID = cat.OID,
                        Items = from pro in Afitems.Where(x => x.OID == cat.OID && x.VendorID == v.Id).ToList() select new AfProducts { ID = pro.ID, VendorID = pro.VendorID, ImgUrl = pro.ImgUrl, LinkType = pro.LinkType }
                    });
                }

                AfVendorWithCategories.Add(new AfVendorWithCategories
                {
                    VendorId = v.Id,
                    VendorName = v.VendorName,
                    CategoryWithItems = AfCategoriesWithItems
                });
            }

            var res = new AFItemsForDisplay()
            {
                data = AfVendorWithCategories
            };

            return res;
        }

        public List<AfCategoriesWithItems> GetAfCategoriesWithProduct(int userId, int loginTypeId, int categoryId = 0)
        {
            var Afitems = GetAllAfItems(0, userId, loginTypeId, categoryId);
            //var Categories = GetAfCategories();
            var Categories = Afitems.Select(x => new { CategoryName = x.Name, CategoryId = x.OID }).Distinct();
            var AfCategoriesWithItems = new List<AfCategoriesWithItems>();
            foreach (var cat in Categories)
            {
                AfCategoriesWithItems.Add(new AfCategoriesWithItems
                {
                    CategoryID = cat.CategoryId,
                    CategoryName = cat.CategoryName,
                    Icon = $"{DOCType.OperatorIconSuffix}{cat.CategoryId}.png",
                    Items = Afitems.Where(x => x.OID == cat.CategoryId).ToList().Select(x => new AfProducts { ID = x.ID, ImgUrl = x.ImgUrl, IsImageURL = x.IsImageURL, LinkType = x.LinkType,Description=x.Description,Tittle=x.Tittle })
                });
            }
            return AfCategoriesWithItems;
        }

        public IEnumerable<AfVenodrsWithItems> GetAfVendorsWithProduct(int userId, int loginTypeId, int vendorId = 0)
        {
            var Afitems = GetAllAfItems(vendorId, userId, loginTypeId, 0);
            var vendors = Afitems.Select(x => new { VendorID = x.VendorID, VendorName = x.VendorName }).Distinct();
            var afVenodrsWithItems = new List<AfVenodrsWithItems>();
            foreach (var v in vendors)
            {
                afVenodrsWithItems.Add(new AfVenodrsWithItems
                {
                    VendorId = v.VendorID,
                    VendorName = v.VendorName,
                    Icon = $"{ DOCType.AffiliateVendorIconSuffix}{v.VendorID}.png",
                    Items = Afitems.Where(x => x.VendorID == v.VendorID).ToList().Select(x => new AfProducts { ID = x.ID, ImgUrl = x.ImgUrl, IsImageURL = x.IsImageURL, LinkType = x.LinkType, Description = x.Description, Tittle = x.Tittle })
                });
            }
            return afVenodrsWithItems;
        }
        //=========================================================================



        public AffiliateItemModal GetAfItemsById(int id, int VendorID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = VendorID,
                CommonInt2 = id,
                CommonBool = true
            };
            IProcedure proc = new ProcGetAllAfItems(_dal);
            var response = (AffiliateItemModal)proc.Call(req);
            return response;
        }

        public ASlabDetailModel GetASlabDetail(int SLabID, bool IsAdminDefined)
        {
            var response = new ASlabDetailModel();
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = SLabID,
                CommonInt2 = _lr.RoleID,
            };
            if (!IsAdminDefined)
            {
                IProcedure proc = new ProcASlabDetail(_dal);
                response = (ASlabDetailModel)proc.Call(req);
            }
            else
            {
                var param = new CommonReq
                {
                    CommonStr = "AFS"
                };
                IProcedure proc = new ProcGetOperatorByService(_dal);
                var Operator = (IEnumerable<OperatorDetail>)proc.Call(param);
                //IProcedure proc = new ProcGetAffiliateCategory(_dal);
                //var category = (List<AffiliateCategory>)proc.Call();
                IProcedure procRole = new ProcGetAllUserRole(_dal);
                var Roles = (List<RoleMaster>)procRole.Call(req);
                response.SlabID = SLabID;
                //response.AfCategories = category;
                response.Operators = Operator;
                response.Roles = Roles;
            }
            return response;
        }

        public ASlabDetailModel GetASlabDetailRole(int SLabID, int RoleID, int OID)
        {
            var response = new ASlabDetailModel();
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = SLabID,
                CommonInt2 = RoleID,
                CommonInt3 = OID
            };
            IProcedure proc = new ProcASlabDetailRole(_dal);
            response = (ASlabDetailModel)proc.Call(req);
            return response;
        }

        public ResponseStatus UpdateAfSlabComm(ASlabDetail req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateAfSlabComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }


        public async Task<IResponseStatus> GenrateAfLink(int productId, int loginId)
        {

            IProcedureAsync proc = new ProcGenrateAfLink(_dal);
            IResponseStatus responseStatus = (ResponseStatus)await proc.Call(new CommonReq
            {
                LoginID = loginId,
                CommonInt = productId
            });
            return responseStatus;
        }
    }
}
