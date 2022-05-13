using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAndroidAppReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcAndroidAppReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Method",req.CommonStr??""),
                new SqlParameter("@Req",req.CommonStr2??""),
                new SqlParameter("@Resp",req.CommonStr3??""),
                new SqlParameter("@IsWeb",false)
            };
            try
            {
                await _dal.GetByProcedureAsync(GetName(), param);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_AndroidAndWebAppReqResp";
    }

    public class ProcWebAppReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcWebAppReqResp(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Method",req.CommonStr??""),
                new SqlParameter("@Req",req.CommonStr2??""),
                new SqlParameter("@Resp",req.CommonStr3??""),
                new SqlParameter("@IsWeb",true)
            };
            try
            {
                await _dal.GetByProcedureAsync(GetName(), param);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_AndroidAndWebAppReqResp";
    }



    public class ProcLogPlansServicesReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogPlansServicesReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (PlanLogReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserId",req.UserID),
                new SqlParameter("@UserToken",req.UserToken??string.Empty),
                new SqlParameter("@JWTToken",req.JWTToken??string.Empty),
                new SqlParameter("@SPKey",req.SPKey??string.Empty),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                new SqlParameter("@CircleId",req.CircleId??string.Empty),
                new SqlParameter("@Method",req.Method??string.Empty),
                new SqlParameter("Response",req.Response??string.Empty),
                new SqlParameter("@O1",req.O1??string.Empty),
                new SqlParameter("@O2",req.O2??string.Empty)
            };
            try
            {
                await _dal.GetByProcedureAsync(GetName(), param);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_Log_PlansServicesReqResp";
    }





}
