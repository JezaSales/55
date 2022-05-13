using RoundpayFinTech.AppCode.Model.Issuance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IIssuanceAPIML
    {
        IssuanceOTPResponse GenerateOTP(IssuanceOTPRequest oTPRequest, Func<string, string, int> SaveAPILog);
        IssuanceMatchOTPResponse VerifyOTP(IssuanceOTPRequest oTPRequest, Func<string, string, int> SaveAPILog);
        IssuanceCustomerOnboardingResponse CustomerOnBoarding(IssuanceCustomerOnboardingRequest onboardingRequest, Func<string, string, int> SaveAPILog);
    }
}
