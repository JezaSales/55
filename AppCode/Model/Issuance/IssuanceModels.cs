using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Issuance
{
    public class IssuanceOTPRequest
    {
        public string TransactionID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string OTP { get; set; }
        public string APIOutletID { get; set; }
        public string CustomerID { get; set; }
        public string VRN { get; set; }
        public string MobileNo { get; set; }
        public int UserID { get; set; }
        public string ReffrenceID { get; set; }
    }

    public class IssuanceCustomerOnboardingRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsKYC { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string GSTIN { get; set; }
        public string UID { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public List<DocDetail> DocDetails { get; set; }
    }
    public class IssuanceCustomerOnboardingResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
    }
    public class DocDetail
    {
        public string DocType { get; set; }
        public string DocName { get; set; }
        public string DocNo { get; set; }
        public string UploadFile { get; set; }
    }
    public class IssuanceOTPResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string RefferencNo { get; set; }
    }
    public class IssuanceMatchOTPResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerFound { get; set; }
        public bool IsKYCDone { get; set; }
    }

    public class IssuanceUpdateKYCRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public DocDetail docdtls { get; set; }
    }
    public class IssuanceCommonResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
    }
    public class IssuanceTagRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionID { get; set; }
        public string CustomerID { get; set; }
        public bool IsUpdate { get; set; }
        public string MobileNo { get; set; }
        public string Product { get; set; }
        public string TVC { get; set; }
        public string CCH { get; set; }
        public string TagID { get; set; }
        public string BarCode { get; set; }
        public string EngineNumber { get; set; }
        public string VehicleNumberAvailability { get; set; }
        public string VRN { get; set; }
        public string ChasisNumber { get; set; }
        public string RegistrationDate { get; set; }
        public double InitialAmount { get; set; }
        public string VehicleMakeModel { get; set; }
        public string VehicleColor { get; set; }
        public double IssuanceFees { get; set; }
        public double SecurityFees { get; set; }
        public double MinBalance { get; set; }
        public bool IsCommercialVehicle { get; set; }
        public string VehicleRegistraionNo { get; set; }
        public string VehicleRegistraionFile { get; set; }
    }

    public class ManageVRNRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionID { get; set; }
        public bool IsRemove { get; set; }
        public List<string> VRN { get; set; }
    }
    public class ManageVRNResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<string> SuccessVRN { get; set; }
    }
    public class VehicleDetailRequest
    {
        public int UserID { get; set; }
        public int OutletID { get; set; }
        public int OID { get; set; }
        public string SPKey { get; set; }
        public string APIOutletID { get; set; }
        public string TransactionID { get; set; }
        public string VRN { get; set; }
        public string TagID { get; set; }
    }

    public class VehicleDetailResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string VRN { get; set; }
        public string TagID { get; set; }
        public string VehicleClass { get; set; }
        public string VehicleDescription { get; set; }
        public string AvailableBalance { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
