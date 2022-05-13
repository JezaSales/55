using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDeleteUserFromUserList : IProcedure
    {
        private readonly IDAL _dal;

        public ProcDeleteUserFromUserList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus();
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@NewMob", _req.CommonStr),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            { }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_DeleteUserFromUserList";
    }
}