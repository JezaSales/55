using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ICollect
{
    public class ICollectStatusResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string Amount { get; set; }
        public string UTR { get; set; }
    }
    public class ICollectRequest
    {
        public string UTR { get; set; }
        public string Amount { get; set; }
        public string CorporateCode { get; set; }
        public string PaymentMode { get; set; }
        public string Account { get; set; }
        public string IMPSAccount { get; set; }
        public string AccountName { get; set; }
        public string IMPSAccountName { get; set; }
        public string IFSC { get; set; }
    }

}
