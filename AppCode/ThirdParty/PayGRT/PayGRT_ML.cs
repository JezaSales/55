using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty
{
    public class PayGRT_ML
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly PayGRTSetting _payGRTSetting;
        private readonly string _APICode;
        private readonly int _APIID;
        public PayGRT_ML(string APICode, IHostingEnvironment env)
        {
            _APICode = APICode;
            _env = env;
            var builder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _payGRTSetting = GetAppSetting();
        }

        private PayGRTSetting GetAppSetting()
        {
            var res = new PayGRTSetting();
            try
            {
                res.URL = Configuration[$"RECHARGE:{_APICode}:URL"];
                res.BalanceURL = Configuration[$"RECHARGE:{_APICode}:BalanceURL"];
                res.StatusCheck = Configuration[$"RECHARGE:{_APICode}:StatusCheck"];
                res.MERCHANTID = Configuration[$"RECHARGE:{_APICode}:MERCHANTID"];
                res.APIKEY = Configuration[$"RECHARGE:{_APICode}:APIKEY"];
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetJustRechargeSetting",
                    Error = "Exception:APICode= JSTREC | [" + ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;
        }

        public async Task<string> PayGRTRecharge(TransactionHelper objTransactionHelper, RechargeAPIHit rechargeAPIHit)
        {
            string resp = string.Empty;
            var payGRTRechReq = rechargeAPIHit.payGRTRechReq;
            string body = $"api_ref_id={payGRTRechReq.api_ref_id}&recharge_number={payGRTRechReq.recharge_number}&recharge_amount={payGRTRechReq.recharge_amount}&operator_code={payGRTRechReq.operator_code}";
            var header = new Dictionary<string, string> 
            {
                {"api-key", _payGRTSetting.APIKEY },
                {"Content-Type","application/x-www-form-urlencoded" }
            };
            string URL = string.Concat(_payGRTSetting.URL, "/v1/recharges/", _payGRTSetting.MERCHANTID);
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, body, header).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                resp = "Exception:[ " + ex.Message + "] |" + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = "TransactionHelper",
                    FuncName = "PayGRTRecharge",
                    Error = resp,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            rechargeAPIHit.aPIDetail.URL = URL + "?" + JsonConvert.SerializeObject(payGRTRechReq);
            rechargeAPIHit.Response = resp;
            objTransactionHelper.UpdateAPIResponse(Convert.ToInt32(payGRTRechReq.api_ref_id), rechargeAPIHit);
            return resp;
        }

        public async Task<string> PayGRTStatusCheck(TransactionHelper objTransactionHelper, RechargeAPIHit rechargeAPIHit)
        {
            string resp = string.Empty;
            var payGRTRechReq = rechargeAPIHit.payGRTRechReq;
            string body = $"api_ref_id={payGRTRechReq.api_ref_id}&txn_date={payGRTRechReq.txn_date}";
            var header = new Dictionary<string, string>
            {
                {"api-key", _payGRTSetting.APIKEY },
                {"Content-Type","application/x-www-form-urlencoded" }
            };
            string URL = string.Concat(_payGRTSetting.StatusCheck, "v1/recharges/status/", _payGRTSetting.MERCHANTID);
            try
            {
                resp = await AppWebRequest.O.HWRPostAsync(URL, body, header).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                resp = "Exception:[ " + ex.Message + "] |" + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = "TransactionHelper",
                    FuncName = "PayGRTRecharge",
                    Error = resp,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            rechargeAPIHit.aPIDetail.URL = URL + "?" + JsonConvert.SerializeObject(payGRTRechReq);
            rechargeAPIHit.Response = resp;
            objTransactionHelper.UpdateAPIResponse(Convert.ToInt32(payGRTRechReq.api_ref_id), rechargeAPIHit);
            return resp;
        }
        //public async Task<string> JRRechargeHit(JRRechargeReq objJRRechargeReq, TransactionHelper objTransactionHelper, RechargeAPIHit rechargeAPIHit)
        //{
        //    JRRechargeResp objJRRechargeResp = new JRRechargeResp();
        //    string recCheckSum = CreateMD5HashForRecharge(objJRRechargeReq.CorporateId, objJRRechargeReq.AuthKey, objJRRechargeReq.Mobile, objJRRechargeReq.Amount.ToString(), objJRRechargeReq.SystemReference, objJRRechargeReq.MD5Key_JR);
        //    objJRRechargeReq.APIChkSum = recCheckSum;
        //    var resp = string.Empty;
        //    try
        //    {
        //        resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(objJRRechargeReq.rURL, objJRRechargeReq).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        resp = "Exception:[ " + ex.Message + "] |" + resp;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = "TransactionHelper",
        //            FuncName = "JustRechargeCorporateLogin",
        //            Error = resp,
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    rechargeAPIHit.aPIDetail.URL = objJRRechargeReq.rURL + "?" + JsonConvert.SerializeObject(objJRRechargeReq);
        //    rechargeAPIHit.Response = resp;
        //    objTransactionHelper.UpdateAPIResponse(Convert.ToInt32(objJRRechargeReq.SystemReference), rechargeAPIHit);
        //    return resp;
        //}
        //private string CreateMD5HashForCorporateLogin(string username, string password, string MD5Key)
        //{
        //    //Lower case(MD5 Encryption of (EmailId + Password + Checksum key/MD5Key))
        //    return HashEncryption.O.MD5Hash(username + password + MD5Key).ToLower();
        //}
        //private string CreateMD5HashForRecharge(string CorporateId, string AuthenticationKey, string AccountNo, string AmountR, string SystemReference, string MD5Key_JR)
        //{
        //    //Lower case(MD5 Encryption of (CorporateId +Authkey + Mobile + Amount + SystemReferenceNo + Checksum key / MD5Key))
        //    return HashEncryption.O.MD5Hash(CorporateId + AuthenticationKey + AccountNo + AmountR + SystemReference + MD5Key_JR).ToLower();
        //}
        //public async Task<string> JRCorporateLogin(TransactionHelper objTransactionHelper, RechargeAPIHit rechargeAPIHit)
        //{
        //    JRRechargeReq objJRRechargeReq = rechargeAPIHit.JRRechReq;
        //    var jrApiSeting = GetJustRechargeSetting();
        //    JRCorporateLogin objJRCorporateLogin = new JRCorporateLogin();
        //    objJRCorporateLogin.SecurityKey = jrApiSeting.SecurityKey;
        //    objJRCorporateLogin.EmailId = jrApiSeting.Username;
        //    objJRCorporateLogin.Password = jrApiSeting.Password;
        //    string crtCheckSum = CreateMD5HashForCorporateLogin(jrApiSeting.Username, jrApiSeting.Password, jrApiSeting.MD5Key_JR);
        //    objJRCorporateLogin.APIChkSum = crtCheckSum;
        //    string _url = jrApiSeting.URL;
        //    var resp = string.Empty;
        //    JRCorporateLoginResp JR_Resp = new JRCorporateLoginResp();
        //    var JR_Rech_Resp = string.Empty;
        //    try
        //    {
        //        resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(_url, objJRCorporateLogin).ConfigureAwait(false);
        //        JR_Resp = JsonConvert.DeserializeObject<JRCorporateLoginResp>(resp);
        //        objJRRechargeReq.CorporateId = JR_Resp.CorporateId;
        //        objJRRechargeReq.SecurityKey = jrApiSeting.SecurityKey;
        //        objJRRechargeReq.AuthKey = JR_Resp.AuthenticationKey;
        //        objJRRechargeReq.MD5Key_JR = jrApiSeting.MD5Key_JR;
        //        JR_Rech_Resp = await JRRechargeHit(objJRRechargeReq, objTransactionHelper, rechargeAPIHit).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        resp = "Exception:[ " + ex.Message + "] |" + resp;
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = "JustRechargeIT_ML",
        //            FuncName = "JustRechargeCorporateLogin",
        //            Error = resp,
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }
        //    rechargeAPIHit.aPIDetail.URL = _url + "?" + JsonConvert.SerializeObject(objJRCorporateLogin);
        //    rechargeAPIHit.Response = resp;
        //    objTransactionHelper.UpdateAPIResponse(Convert.ToInt32(objJRRechargeReq.SystemReference), rechargeAPIHit);
        //    return JR_Rech_Resp;
        //}
        ////will be deleted not in use
        //private JRSetting GetJustRechargeSetting()
        //{
        //    var res = new JRSetting();

        //    string SERVICESETTING = "RECHARGE:JSTREC";
        //    try
        //    {
        //        //RECHARGE:JSTREC:Username
        //        res.Username = Configuration["RECHARGE:JSTREC:Username"];
        //        res.Password = Configuration[SERVICESETTING + ":Password"];
        //        res.SecurityKey = Configuration[SERVICESETTING + ":SecurityKey"];
        //        res.MD5Key_JR = Configuration[SERVICESETTING + ":MD5Key"];
        //        res.URL = Configuration[SERVICESETTING + ":URL"];
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorLog = new ErrorLog
        //        {
        //            ClassName = GetType().Name,
        //            FuncName = "GetJustRechargeSetting",
        //            Error = "Exception:APICode= JSTREC | [" + ex.Message,
        //        };
        //        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
        //    }

        //    return res;
        //}
    }
}
