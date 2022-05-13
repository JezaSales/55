namespace RoundpayFinTech.AppCode.ThirdParty.UPIGateway
{
    public class UPIGatewayStatusResponse
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public UPIGatewayStatusData data { get; set; }
    }
    public class UPIGatewayStatusData {
        public int id { get; set; }
        public string customer_vpa { get; set; }
        public decimal amount { get; set; }
        public string client_txn_id { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string customer_mobile { get; set; }
        public string p_info { get; set; }
        public string upi_txn_id { get; set; }
        public string status { get; set; }
        public string remark { get; set; }
        public string redirect_url { get; set; }
        public string txnAt { get; set; }
        public string createdAt { get; set; }
        public UPIGatewayStatusMerchant Merchant { get; set; }
    }
    public class UPIGatewayStatusMerchant {
        public string name { get; set; }
        public string upi_id { get; set; }
    }
    public class UPIGatewayNewRequest
    {
        public string key { get; set; }
        public string client_txn_id { get; set; }
        public string amount { get; set; }
        public string p_info { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string customer_mobile { get; set; }
        public string redirect_url { get; set; }
    }
    public class UPIGatewayNewResponse
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public UPIGatewayNewData data { get; set; }
    }
    public class UPIGatewayNewData
    {
        public int order_id { get; set; }
        public string payment_url { get; set; }
    }
    public class UPIGatewayRequest
    {
        public string client_vpa { get; set; }
        public int amount { get; set; }
        public int client_name { get; set; }
        public int client_email { get; set; }
        public int client_mobile { get; set; }
        public int p_info { get; set; }
        public int client_txn_id { get; set; }
        public int udf1 { get; set; }
        public int udf2 { get; set; }
        public int udf3 { get; set; }
        public string key { get; set; }
        public string redirect_url { get; set; }
        public string hash { get; set; }
    }

    public class UPIGatewayResponse
    {
        public string client_vpa { get; set; }
        public string amount { get; set; }
        public string client_name { get; set; }
        public string client_email { get; set; }
        public string client_mobile { get; set; }
        public string p_info { get; set; }
        public int client_txn_id { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string socket_id { get; set; }
        public string upi_txn_id { get; set; }
        public string remark { get; set; }
        public string txnAt { get; set; }
        public string status { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }
}
