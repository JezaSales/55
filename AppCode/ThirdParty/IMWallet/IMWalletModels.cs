using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class IMWAppSetting
    {
        public string BaseURL { get; set; }
        public string webToken { get; set; }
        public string userCode { get; set; }
    }
    public class IMWResp
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public double remainingLimit { get; set; }
        public string senderMobile { get; set; }
        public int errorCode { get; set; }
        public string status { get; set; }
        public string msg { get; set; }
        public string otp_token { get; set; }
        public string requestID { get; set; }
        public string utr { get; set; }
        public string beneName { get; set; }
    }
    public enum IMWErrCode
    {
        success = 0,
        MobileNumberNotFound = 1032,
        Error = 999,
        InSufficientFund = 101,
        OTPNotMatchedError = 9009,
    }
    public class IMWBeneList
    {
        public string beneID { get; set; }
        public string bankName { get; set; }
        public string ifsc { get; set; }
        public string accountHolderName { get; set; }
        public string account { get; set; }
    }
    public class IMWGetBeneResp
    {
        public List<IMWBeneList> data { get; set; }
        public int linkedAccounts { get; set; }
        public int errorCode { get; set; }
        public string status { get; set; }
    }

    public class IMWAccTrData
    {
        public double amount { get; set; }
        public string utr { get; set; }
        public string imwsubID { get; set; }
        public string status { get; set; }
    }

    public class IMWAccTrResp
    {
        public string msg { get; set; }
        public double amount { get; set; }
        public double charges { get; set; }
        public string imwid { get; set; }
        public string beneName { get; set; }
        public List<IMWAccTrData> data { get; set; }
        public string bankName { get; set; }
        public string ifsc { get; set; }
        public string tid { get; set; }
        public string account { get; set; }
        public string status { get; set; }
    }
}
