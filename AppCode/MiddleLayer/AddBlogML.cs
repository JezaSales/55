using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
	public class AddBlogML : IAddBlogs
	{
		private readonly IHttpContextAccessor _accessor;
		private readonly IHostingEnvironment _env;
		private readonly ISession _session;
		private readonly IDAL _dal;
		private readonly IConnectionConfiguration _c;
		private readonly LoginResponse _lr;

		public AddBlogML(IHttpContextAccessor accessor, IHostingEnvironment env)
		{
			_accessor = accessor;
			_env = env;
			_c = new ConnectionConfiguration(_accessor, _env);
			_session = _accessor.HttpContext.Session;
			_dal = new DAL(_c.GetConnectionString());
			_lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
		}
		public IResponseStatus AddBlog(Blog newsReq)
		{
			IResponseStatus _resp = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			
				
				CommonReq commonReq = new CommonReq
				{
					LoginID = _lr.UserID,
					LoginTypeID = _lr.LoginTypeID,
					CommonInt = newsReq.ID,
					str = newsReq.ContentDetails,
					CommonStr = newsReq.Tittle
				};

				IProcedure _proc = new ProcAddBlogs(_dal);
				_resp = (ResponseStatus)_proc.Call(commonReq);

			
			return _resp;
		}

		public async Task<List<Blog>> GetBlogData(int id)
		{
			IProcedureAsync _proc = new ProcGetBlogdata(_dal);
			var resp = (List<Blog>)await _proc.Call(id).ConfigureAwait(false);
			return resp ?? new List<Blog>();
		}
	}
}
