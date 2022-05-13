namespace RoundpayFinTech.AppCode.ThirdParty
{
    public class DecentroAppSetting
    {
        public string BaseURL { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string module_secret { get; set; }
        public string provider_secret { get; set; }
    }

    public class DecentroVPAResp
    {
        public string decentroTxnId { get; set; }
        public string status { get; set; }
        public string responseCode { get; set; }
        public string message { get; set; }
        public DecentroData data { get; set; }
    }

    public class DecentroData
    {
        public string upiId { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string statusDescription { get; set; }
    }

}