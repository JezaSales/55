﻿namespace RoundpayFinTech
{
    public class CashPointIndiaOnboardRequest
    {
        public string api_key { get; set; }
        public string vle_id { get; set; }
        public string vle_name { get; set; }
        public string vle_mob { get; set; }
        public string vle_email { get; set; }
        public string vle_shop { get; set; }
        public string vle_loc { get; set; }
        public string vle_state { get; set; }
        public string vle_pin { get; set; }
        public string vle_uid { get; set; }
        public string vle_pan { get; set; }
    }
    public class CashPointIndiaRequest
    {
        public string api_key { get; set; }
        public string vle_id { get; set; }
        public string type { get; set; }
        public string qty { get; set; }
        public string order_id { get; set; }
    }
    public class CashPointIndiaResponse
    {
        public string order_id { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string vle_id { get; set; }
        public string vle_status { get; set; }
        public string type { get; set; }
        public string qty { get; set; }
        public string rate { get; set; }
        public string amount { get; set; }
        public string balance { get; set; }
        public string old_bal { get; set; }
        public string new_bal { get; set; }
        

    }
    public class CashPointIndiaAppSetting
    {
        public string api_key { get; set; }
        public string BaseURL { get; set; }
    }
}
