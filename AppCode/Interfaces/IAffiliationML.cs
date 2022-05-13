using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IAffiliationML
    {
        ResponseStatus SaveAffiliateVendor(string VendorName, string VendorBanner, int Id, bool IsActive);
        AfVendorCommission GetAfCategoryCommission(int VendorID);
        IEnumerable<AffiliateVendors> GetAffiliatedVendors(int id);
        ResponseStatus SaveAfVendorCateComm(int OID, int VendorID, decimal comm, int AmtType);
        //ResponseStatus AddAffiliatedItems(int VendorID, string Link, string ImgUrl, bool IsActive, int ID, int LinkType, int OID, bool IsDel, bool IsImageURL, string Tittle);
        ResponseStatus AddAffiliatedItems(AffiliatedItem affiliatedItem, bool isDel = false);
        IEnumerable<AffiliatedItem> GetAllAfItems(int vendorId, int userID, int loginTypeId, int categoryId = 0);
        Task<AffiliatedItemViewModel> GetAllAffiliatedItems(int userId, int loginTypeId);
        IEnumerable<AffiliatedItem> GetAfItems(int ID, int UserID, int LoginTypeID);
        IEnumerable<AffiliatedItem> GetAffilietedproduct(int OID);
        IEnumerable<AffiliatedItem> GetAffilietedVproduct(int VID);
        AffiliateItemModal GetAfItemsById(int id, int VendorID);
       // object SaveAffiliateVendor(string vendorName, IFormFile file, int id, bool isActive);
        ASlabDetailModel GetASlabDetail(int SLabID, bool IsAdminDefined);
        ASlabDetailModel GetASlabDetailRole(int SLabID, int RoleID, int CategoryID);
        ResponseStatus UpdateAfSlabComm(ASlabDetail req);
        AFItemsForDisplay GetAfItemsForDisplay();
        List<AfCategoriesWithItems> GetAfCategoriesWithProduct(int userId, int loginTypeId, int categoryId = 0);
        IEnumerable<AfVenodrsWithItems> GetAfVendorsWithProduct(int userId, int loginTypeId, int vendorId = 0);
        Task<IResponseStatus> GenrateAfLink(int productId, int loginId);
    }
}
