using Fintech.AppCode.Model;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace RoundpayFinTech.AppCode.Model
{
    public class PayGRTSetting
    {
        public string URL { get; set; }
        public string BalanceURL { get; set; }
        public string StatusCheck { get; set; }
        public string MERCHANTID { get; set; }
        public string APIKEY { get; set; }
    }

    public class PayGRTRechReq
    {
        public string api_ref_id { get; set; }
        public string recharge_number { get; set; }
        public string recharge_amount { get; set; }
        public string operator_code { get; set; }
        public string txn_date { get; set; }
        public bool IsRecharge { get; set; }
    }
}
