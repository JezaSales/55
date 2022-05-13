using RoundpayFinTech.AppCode.Model.Issuance;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IIssuanceML
    {
        IssuanceOTPResponse GenerateOTP(IssuanceOTPRequest issuanceOTPRequest);
        IssuanceMatchOTPResponse VerifyOTP(IssuanceOTPRequest issuanceOTPRequest);
        IssuanceCustomerOnboardingResponse CustomerOnboarding(IssuanceCustomerOnboardingRequest onboardingRequest);
    }
}
