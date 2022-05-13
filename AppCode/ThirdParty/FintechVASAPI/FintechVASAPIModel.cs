using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.FintechVASAPI
{
    public class FNTHVASAPISetting
    {
        public string UserID { get; set; }
        public string Token { get; set; }
        public string BaseURL { get; set; }
        public string PlanURL { get; set; }
    }

    public class VASAPITokenResp
    {
        public int statusCode { get; set; }
        public string msg { get; set; }
        public string token { get; set; }
    }
    
    
}
