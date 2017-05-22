using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Iesi.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;

namespace QuartzAdmin.web.Models
{
    using Quartz.Impl.Matchers;

    [ActiveRecord(Table="tbl_instances")]
    public class InstanceModel : ActiveRecordValidationBase<InstanceModel>
    {
        public InstanceModel()
        {
            InstanceProperties = new HashedSet<InstancePropertyModel>();
            
            
        }

        [PrimaryKey(Generator=PrimaryKeyType.Identity)]
        public virtual int InstanceID { get; set; }
        [Property, ValidateNonEmpty]
        public virtual string InstanceName { get; set; }

        [HasMany(typeof(InstancePropertyModel), Table = "tbl_instanceproperties",
                 ColumnKey = "InstanceID",
                 Cascade = ManyRelationCascadeEnum.All, Inverse=true)]
        public virtual Iesi.Collections.Generic.ISet<InstancePropertyModel> InstanceProperties { get; set; }



        private IScheduler _CurrentScheduler = null;
        public IScheduler GetQuartzScheduler()
        {
            if (_CurrentScheduler == null)
            {
                System.Collections.Specialized.NameValueCollection props = new System.Collections.Specialized.NameValueCollection();

                foreach (InstancePropertyModel prop in this.InstanceProperties)
                {
                    props.Add(prop.PropertyName, prop.PropertyValue);
                }
                ISchedulerFactory sf = new StdSchedulerFactory(props);
                _CurrentScheduler = sf.GetScheduler();
            }

            return _CurrentScheduler;

        }

        public IQueryable<string> FindAllGroups()
        {
            IScheduler sched = this.GetQuartzScheduler();

            List<string> groups = new List<string>();

            string[] jobGroups = sched.GetJobGroupNames().ToArray();
            string[] triggerGroups = sched.GetTriggerGroupNames().ToArray();

            foreach (string jg in jobGroups)
            {
                groups.Add(jg);
            }

            foreach (string tg in triggerGroups)
            {
                if (!groups.Contains(tg))
                {
                    groups.Add(tg);
                }
            }

            return sched.GetJobGroupNames().AsQueryable();
        }

        public List<IJobDetail> GetAllJobs(string groupName)
        {
            List<IJobDetail> jobs = new List<IJobDetail>();
            IScheduler sched = this.GetQuartzScheduler();
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var jobKeys = sched.GetJobKeys(groupMatcher);

            foreach (var jobkey in jobKeys)
            {
                jobs.Add(sched.GetJobDetail(jobkey));
            }

            return jobs;
        }

        public List<IJobDetail> GetAllJobs()
        {
            List<IJobDetail> jobs = new List<IJobDetail>();
            var groups = FindAllGroups();
            foreach (string group in groups)
            {
                List<IJobDetail> groupJobs = GetAllJobs(group);
                jobs.AddRange(groupJobs);
            }
            return jobs;
        }

        public List<ITrigger> GetAllTriggers(string groupName)
        {
            List<ITrigger> triggers = new List<ITrigger>();
            IScheduler sched = this.GetQuartzScheduler();
            var groupMatcher = GroupMatcher<TriggerKey>.GroupContains(groupName);
            var triggerKeys = sched.GetTriggerKeys(groupMatcher);

            foreach (var triggerkey in triggerKeys)
            {
                triggers.Add(sched.GetTrigger(triggerkey));
            }

            return triggers;
        }

        public JobKey getJobKey(string jobName, string groupName, IScheduler sched)
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var jobKeys = sched.GetJobKeys(groupMatcher);
            return jobKeys.First(x => x.Name == jobName);
        }

        public List<ITrigger> GetAllTriggers()
        {
            List<ITrigger> triggers = new List<ITrigger>();
            var groups = FindAllGroups();
            foreach (string group in groups)
            {
                List<ITrigger> groupTriggers = GetAllTriggers(group);
                triggers.AddRange(groupTriggers);
            }

            return triggers;
        }



    }
}
