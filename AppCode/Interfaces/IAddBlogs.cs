using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IAddBlogs
    {
        IResponseStatus AddBlog(Blog newsReq);
        Task<List<Blog>> GetBlogData(int id);
    }
}
