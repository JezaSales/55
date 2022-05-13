using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Paymentgateway;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Globalization;
using System.Threading.Tasks;
namespace RoundpayFinTech.AppCode.ThirdParty.UPIGateway
{
    public partial class UPIGatewayML
    {
        public PGModelForRedirection GeneratePGRequestForWebNew(PGTransactionResponse pGTransactionResponse, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var res = new PGModelForRedirection
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var uPIGatewayRequest = new UPIGatewayNewRequest
            {
                key = pGTransactionResponse.MerchantID,
                amount = pGTransactionResponse.Amount.ToString(),
                client_txn_id = pGTransactionResponse.TID.ToString(),
                customer_email = pGTransactionResponse.EmailID,
                customer_name = pGTransactionResponse.Name,
                customer_mobile = pGTransactionResponse.MobileNo,
                p_info = "Add Money to consume services"
            };
            try
            {
                //pGTransactionResponse.Domain = "https://roundpay.net";
                uPIGatewayRequest.redirect_url = pGTransactionResponse.Domain + "/PGCallback/UPIGatewayRedirectNew?TID=" + pGTransactionResponse.TID;
                var apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(pGTransactionResponse.URL, uPIGatewayRequest).Result;
                int VendorID;
                if (apiResp != null)
                {
                    var UGRes = JsonConvert.DeserializeObject<UPIGatewayNewResponse>(apiResp);
                    if (UGRes != null)
                    {
                        if (UGRes.status)
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = "Transaction intiated";
                            res.URL = UGRes.data.payment_url;
                            VendorID = UGRes.data.order_id;
                        }
                        else
                        {
                            res.Msg = UGRes.msg;
                        }
                    }
                }

                savePGTransactionLog(pGTransactionResponse.PGID, pGTransactionResponse.TID, JsonConvert.SerializeObject(res), pGTransactionResponse.TransactionID, (pGTransactionResponse.URL ?? string.Empty) + JsonConvert.SerializeObject(uPIGatewayRequest) + (apiResp ?? string.Empty), RequestMode.PANEL, true, pGTransactionResponse.Amount, string.Empty);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GeneratePGRequestForWebNew",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = pGTransactionResponse.PGID
                });
            }
            return res;
        }

        public UPIGatewayStatusResponse StatusCheckPG(PGTransactionParam transactionPGLog, Func<int, int, string, string, string, int, bool, decimal, string, Task> savePGTransactionLog)
        {
            var payresp = new UPIGatewayStatusResponse();
           
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(transactionPGLog.TransactionID);
            var paytmPGRequest = new
            {
                key= transactionPGLog.MerchantID,
                client_txn_id= transactionPGLog.TID.ToString(),
                txn_date= Convert.ToDateTime(fromD).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
            };
            string apiResp = string.Empty;
            try
            {
                apiResp = AppWebRequest.O.PostJsonDataUsingHWRTLS(transactionPGLog.StatusCheckURL, paytmPGRequest).Result;
                if (!string.IsNullOrEmpty(apiResp))
                {
                    payresp = JsonConvert.DeserializeObject<UPIGatewayStatusResponse>(apiResp);
                }
            }
            catch (Exception ex)
            {
                apiResp = "Exception:" + ex.Message + "|" + apiResp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "StatusCheckPG",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = transactionPGLog.PGID
                });
            }
            savePGTransactionLog(transactionPGLog.PGID, transactionPGLog.TID, transactionPGLog.StatusCheckURL+JsonConvert.SerializeObject(paytmPGRequest) , transactionPGLog.TransactionID, apiResp, RequestMode.API, true, transactionPGLog.Amount, string.Empty);
            return payresp;
        }
    }
}
