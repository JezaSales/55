using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SmartCollect
    {
        public int Id { get; set; }
        public string SmartCollectTypeName { get; set; }
        public string Remark { get; set; }
        public bool IsActive { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsQR { get; set; }
        public bool IsVPA { get; set; }

    }
    public class SmartCollectViewModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public SmartCollectDataModel iciciCollectData { get; set; }
        public SmartCollectDataModel razorpayCollectData { get; set; }
        public SmartCollectDataModel cashfreeCollectData { get; set; }
    }
    public class SmartCollectDataModel
    {
        public bool IsQRShow { get; set; }
        public bool IsVPAShow { get; set; }
        public bool IsVirtualShow { get; set; }
        public object data { get; set; }
    }
    public class SmartCollectData
    {
        public string BankName { get; set; }
        public string AccountHolder { get; set; }
        public string Account { get; set; }
        public string IFSC { get; set; }
        public string VPA { get; set; }
        public string QR { get; set; }
        
    }
}
