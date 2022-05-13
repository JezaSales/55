using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.IDFC
{
    public class IDFCCommonRequest
    {
        public string requestId { get; set; }
        public string entityId { get; set; }
        public string txnTime { get; set; }
        public string chkSm { get; set; }
    }
    public class IDFCOTPRequest : IDFCCommonRequest
    {
        public string custId { get; set; }
        public string vrn { get; set; }
        public string mobNo { get; set; }
    }

    public class IDFCOTPResponse
    {
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public string txnNo { get; set; }
    }

    public class IDFCMatchOTPRequest : IDFCCommonRequest
    {
        public string otp { get; set; }
        public string txnNo { get; set; }
    }
    public class IDFCMatchOTPRepsonse
    {
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public string customerType { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string gstNo { get; set; }
        public string uId { get; set; }
        public string status { get; set; }
        public string mobileNo { get; set; }
        public string phoneNo { get; set; }
        public string emailId { get; set; }
        public string usrConsent { get; set; }
        public IDFCAddressDetail addressDetail { get; set; }
        public object bankDetail { get; set; }
    }
    public class IDFCAddressDetail
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string district { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
    }

    public class IDFCOnboardRequest : IDFCCommonRequest
    {
        public string customerId { get; set; }
        public string action { get; set; }
        public string customerType { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string gstNo { get; set; }
        public string uId { get; set; }
        public string status { get; set; }
        public string mobileNo { get; set; }
        public string phoneNo { get; set; }
        public string emailId { get; set; }
        public string usrConsent { get; set; }
        public object bankDetail { get; set; }
        public List<IDFCDoc> docdtls { get; set; }
        public IDFCAddressDetail addressDetail { get; set; }
    }

    public class IDFCOnboardResponse
    {
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public string custId { get; set; }
        public IDFCDocDetailResponse docdtls { get; set; }
    }

    public class IDFCDocDetailResponse
    {
        public int successCnt { get; set; }
        public int resCode { get; set; }
        public int resMsg { get; set; }
        public int custid { get; set; }
        public List<IDFCdocResponseList> docResponseList { get; set; }
    }
    public class IDFCdocResponseList
    {
        public string docName { get; set; }
        public string issuingAuthority { get; set; }
        public string docNo { get; set; }
        public string resCode { get; set; }
        public string resMsg { get; set; }
    }
    public class IDFCIssuanceTagResponse
    {
        public string respCode { get; set; }
        public string respMessage { get; set; }
        public string requestid { get; set; }
    }

    public class IDFCVRNResponse
    {
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public List<string> successVrn { get; set; }
    }

    public class IDFCVehicleDetail
    {
        public string vrn { get; set; }
        public string tagId { get; set; }
        public string vehClass { get; set; }
        public string vehDesc { get; set; }
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public string availableBal { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }
    public class IDFCDoc
    {
        public string docType { get; set; }
        public string docName { get; set; }
        public string docNo { get; set; }
        public string uploadFile { get; set; }
    }

    public class IDFCAppSetting
    {
        public string EnityID { get; set; }
        public string BasicAuth { get; set; }
        public string SecretToken { get; set; }
        public string GetOTPURL { get; set; }
        public string VerifyOTPURL { get; set; }
        public string CustomerOnboardURL { get; set; }
        public string CustomerUpdateURL { get; set; }
        public string TagIssuanceURL { get; set; }
        public string ManageVehicleURL { get; set; }
        public string VehicleDetailURL { get; set; }
    }
    public class IDFCUpdateKYCRequest : IDFCCommonRequest
    {
        public string makerId { get; set; }
        public string custId { get; set; }
        public string kycUpdate { get; set; }
        public string dateTime { get; set; }
        public IDFCDoc docdtls { get; set; }
    }
    public class IDFCUpdateKYCResponse
    {
        public string resCode { get; set; }
        public string resMsg { get; set; }
        public string requestid { get; set; }
    }

    public class IDFCIssuanceTagRequest : IDFCCommonRequest
    {
        public string action { get; set; }
        public string createUser { get; set; }
        public string custId { get; set; }
        public string product { get; set; }
        public string tvc { get; set; }
        public string cch { get; set; }
        public string tagId { get; set; }
        public string chasisNo { get; set; }
        public string barcode { get; set; }
        public string engineNo { get; set; }
        public string vehicleMakeModel { get; set; }
        public string vehicleColor { get; set; }
        public string vehicleRegAvlbl { get; set; }
        public string vrn { get; set; }
        public string regDate { get; set; }
        public string issFee { get; set; }
        public string security { get; set; }
        public string initial { get; set; }
        public string minBal { get; set; }
        public string exempted { get; set; }
        public string exemptedCatg { get; set; }
        public string commVeh { get; set; }
        public string mobileNo { get; set; }
        public string vehImg { get; set; }
        public IDFCDoc docdtls { get; set; }
    }

    public class IDFCManageVRNRequest
    {
        public string entityId { get; set; }
        public List<string> vrn { get; set; }
        public string action { get; set; }
    }
}
