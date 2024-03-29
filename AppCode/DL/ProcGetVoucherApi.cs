﻿using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetVoucherApi : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVoucherApi(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();
        public object Call()
        {
            List<ApiListModel> _res = new List<ApiListModel>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                foreach (DataRow dr in dt.Rows)
                {
                    ApiListModel Mode = new ApiListModel
                    {
                        Id = Convert.ToInt32(dr["_ID"]),
                        Name = Convert.ToString(dr["_Name"])
                    };
                    _res.Add(Mode);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public string GetName() => @"select * from tbl_API(nolock) where _IsVoucher=1";
    }
}
