
using System;
using System.Collections.Generic;

namespace RoundpayFinTech
{
    public class DiskshuPayAppSetting
    {
        public string Token { get; set; }
        public string MobileNo { get; set; }
        public string CustomerID { get; set; }
        public string BaseURL { get; set; }
    }


    public class DKPVerifyData
    {
        public string transid { get; set; }
        public string optransid { get; set; }
        public string referenceid { get; set; }
        public string beneName { get; set; }
    }

    public class DKPVerifyResp
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DKPVerifyData Data { get; set; }
    }
    public class DKPayATData
    {
        public string transid { get; set; }
        public string optransid { get; set; }
        public string referenceid { get; set; }
        public int amount { get; set; }
        public string beneName { get; set; }
    }

    public class DKPayAccTrfResp
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public DKPayATData Data { get; set; }
    }
}
