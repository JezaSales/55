
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model
{
    public class APIResponseTable
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Schema { get; set; }
    }

    public class APIResponseModel
    {
        public IEnumerable<APIResponseTable> Response { get; set; }
        public int APIID { get; set; }
    }
}