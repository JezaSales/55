using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class VStock: CommonReq
    {
        public int ID { get; set; }
        public int Amount { get; set; }
        public int VoucherID { get; set; }
        public string VoucherType { get; set; }
        public int TotalCount { get; set; }
        public int Remain { get; set; }

    }
}
