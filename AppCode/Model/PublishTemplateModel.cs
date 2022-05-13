using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class PublishTemplateModel
    {
        public string Template { get; set; }
        public string Published { get; set; }
    }

    public class PublishTemplateViewModel
    {
        public bool IsAdmin { get; set; }
        public string Content { get; set; }
    }
}
