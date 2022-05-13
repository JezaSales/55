using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ICallbackML
    {
        Task<string> LogCallBackRequest(CallbackData callbackData, bool IpValidation = true);
        Task<bool> UpdateAPIURLHitting(_CallbackData _callbackData);
        Task<string> GetLapuTransactions(string APIName);
        Task<string> GetLapuSocialAlert(string APIName, int SocialAlertType);
        Task<IEnumerable<PSADetailForMachine>> GetPSADetailMachine(string Machine);
        ResponseStatus UpdatePSAFromMachine(_CallbackData _Callback);
        bool ValidateCallbackIP();
        Task<IResponseStatus> SaveICICIStatementAsync(PostStatetmentRequest postStatetmentRequest);
        Task<IResponseStatus> SaveICICIStatementNewAsync(PostStatetmentRequest postStatetmentRequest);
        Task<string> LogCallBackRequest(CallbackData callbackData);
        //Task ErrorLog(string className, string funcName, string error);
    }
    public interface IShoppingCallback
    {
        ResponseStatus GenerateShoppingOTP(ShoppingOTPReq shoppingOTPReq);
        ResponseStatus MatchShoppingOTP(ShoppingOTPReq shoppingOTPReq);
    }
}
