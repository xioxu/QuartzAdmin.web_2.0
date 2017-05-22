using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuartzAdmin.web.Models
{
    using Quartz;

    public class JobViewModel
    {
        public Quartz.IJobDetail JobDetail { get; set; }
        public IList<ITrigger> Triggers { get; set; }
    }
}
