using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class SlabdetailServicewise
    {
        public int ServiceID { get; set; }
        public string Name { get; set; }
        public string OpType { get; set; }
        public int OpTypeID { get; set; }
        public decimal Comm { get; set; }
        public bool CommType { get; set; }
        public bool AmtType { get; set; }
        public int SlabID { get; set; }
        public int EntryBy { get; set; }
        public DateTime EntryDate { get; set; }
        public int ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }


    }
}
