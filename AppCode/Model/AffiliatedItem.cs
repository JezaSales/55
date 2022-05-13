using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class AffiliatedItem
    {
        public int LoginId { get; set; }
        public int LT { get; set; }
        public int ID { get; set; }
        public int VendorID { get; set; }
        public int OID { get; set; }
        public string Name { get; set; }
        public string VendorName { get; set; }
        public string Operator { get; set; }
        public string Link { get; set; }
        public string ImgUrl { get; set; }
        public string Tittle { get; set; }
        public string ImageURL { get; set; }
        public string EntryDate { get; set; }
        public bool IsActive { get; set; }
        public int LinkType { get; set; }
        public bool IsImageURL { get; set; }
        public string Description { get; set; }
        public string Offer { get; set; }
    }

    public class AffiliateItemModal : AffiliatedItem
    {
        public IEnumerable<OperatorDetail> Operator { get; set; }


    }

    public class AffiliatedOperator
    {
        public int OID { get; set; }
        public string Name { get; set; }
    }

    public class AffiliatedVendor
    {
        public int VID { get; set; }
        public string Name { get; set; }
        public string Commssion { get; set; }
    }
    public class AffiliatedItemViewModel
    {
        public IEnumerable<AffiliatedOperator> affiliatedOperator { get; set; }
        public IEnumerable<AffiliatedVendor> affiliatedVendor { get; set; }
    }
}
