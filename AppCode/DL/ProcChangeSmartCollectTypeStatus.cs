using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcChangeSmartCollectTypeStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangeSmartCollectTypeStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            SmartCollect req = (SmartCollect)obj;
            SqlParameter[] param = {
                   new SqlParameter("@Id",req.Id),
                   new SqlParameter("@IsActive",req.IsActive),
                   new SqlParameter("@IsVirtual",req.IsVirtual),
                   new SqlParameter("@IsQR",req.IsQR),
                   new SqlParameter("@IsUPI",req.IsVPA)
            };
            var res = new ResponseStatus
            {
                Statuscode = -1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0]["Statuscode"]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    Error = ex.Message
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Update MASTER_Smart_Collect_Type set _IsActive=@IsActive,_IsVirtual=@IsVirtual,_IsQR=@IsQR,_IsUPI=@IsUPI where _ID=@ID;select 1 Statuscode,'Status changed successfuly' msg";
    }
}
