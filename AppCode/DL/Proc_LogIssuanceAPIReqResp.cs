using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class Proc_LogIssuanceAPIReqResp : IProcedure
    {
        private readonly IDAL _dal;
        public Proc_LogIssuanceAPIReqResp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Req",req.CommonStr??string.Empty),
                new SqlParameter("@Resp",req.CommonStr1??string.Empty)
            };
            try
            {
                _dal.Execute(GetName(), param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            return 0;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "insert into Log_IssuanceAPIReqResp(_Req,_Resp,_EntryDate)values(@Req,@Resp,getdate())";
    }
}
