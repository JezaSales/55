using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class PlansReqModel
    {
        public string AccountNo { get; set; }
        public int ErrorCode { get; set; }

    }
    public class PlansRespModel
    {
        public int StatusCode { get; set; }
        public int ErrorCode { get; set; }
        public string Msg { get; set; }
    }
    public class PlansRespModel<T>: PlansRespModel
    {      
        public T Data {get;set;}
    }
    public class PlanLogReq
    {
        public int UserID { get; set; }
        public string Method { get; set; }
        public string UserToken { get; set; }
        public string JWTToken { get; set; }
        public string SPKey { get; set; }
        public string AccountNo { get; set; }
        public string CircleId { get; set; }
        public string Response { get; set; }
        public string O1 { get; set; }
        public string O2 { get; set; }
    }
    public class PSValidateReq
    {
        public int LoginId { get; set; }
        public int RequestMode { get; set; }
        public string Token { get; set; }
        public string SPKey { get; set; }
    }
    public class PSValidateResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int OID { get; set; }
    }
    public class PSRechPResp
    {
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public string Msg { get; set; }
    }

    public class PSCircleCode
    {
        public int CircleCode { get; set; }
        public string CircleName { get; set; }
    }


    public class PSOperatorCode
    {
        public string ServiceName { get; set; }
        public string OperatorName { get; set; }
        public string SpKey { get; set; }
    }

}
