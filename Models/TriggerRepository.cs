using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;


namespace QuartzAdmin.web.Models
{
    using Quartz.Impl.Matchers;

    public class TriggerRepository
    {
        private InstanceModel quartzInstance;
        public TriggerRepository(string instanceName)
        {
            InstanceRepository repo = new InstanceRepository();
            quartzInstance = repo.GetInstance(instanceName);
        }

        public TriggerRepository(InstanceModel instance)
        {
            quartzInstance = instance;
        }

        public ITrigger GetTrigger(string triggerName, string groupName)
        {
            IScheduler sched =quartzInstance.GetQuartzScheduler();

            var tk = getTriggerKey(triggerName, groupName, sched);
            return sched.GetTrigger(tk);

        }

        public TriggerKey getTriggerKey(string jobName, string groupName, IScheduler sched)
        {
            var groupMatcher = GroupMatcher<TriggerKey>.GroupContains(groupName);
            var triggerKeys = sched.GetTriggerKeys(groupMatcher);
            return triggerKeys.First(x => x.Name == jobName);
        }

        public IList<TriggerStatusModel> GetAllTriggerStatus(string groupName)
        {
            IScheduler sched = quartzInstance.GetQuartzScheduler();

            var groupMatcher = GroupMatcher<TriggerKey>.GroupContains(groupName);
            var triggerNames = sched.GetTriggerKeys(groupMatcher);

           // string[] triggerNames= sched.GetTriggerNames(groupName);
            List<TriggerStatusModel> triggerStatuses = new List<TriggerStatusModel>();
            foreach (TriggerKey triggerkey in triggerNames)
            {
                ITrigger trig = sched.GetTrigger(triggerkey);
                TriggerState st = sched.GetTriggerState(triggerkey);
                var nextFireTimeOffset = trig.GetNextFireTimeUtc();
                var lastFireTimeeOffset = trig.GetPreviousFireTimeUtc();

                DateTime? nextFireTime = null;

                if (nextFireTimeOffset.HasValue)
                {
                    nextFireTime = nextFireTimeOffset.Value.DateTime;
                }

                DateTime? lastFireTime = null;

                if (lastFireTimeeOffset.HasValue)
                {
                    lastFireTime = lastFireTimeeOffset.Value.DateTime;
                }

             
                triggerStatuses.Add(new TriggerStatusModel()
                {
                    TriggerName = triggerkey.Name,
                    GroupName = groupName,
                    State = st,
                    NextFireTime = nextFireTime.HasValue?nextFireTime.Value.ToLocalTime().ToString():"",
                    LastFireTime = lastFireTime.HasValue ? lastFireTime.Value.ToLocalTime().ToString() : "",
                    JobName = trig.JobKey.Name
                });

            }

            return triggerStatuses;


        }

        public IList<TriggerStatusModel> GetAllTriggerStatus()
        {
            List<TriggerStatusModel> triggerStatuses = new List<TriggerStatusModel>();
            if (quartzInstance != null)
            {
                var groups = quartzInstance.FindAllGroups();
                foreach (string group in groups)
                {
                    triggerStatuses.AddRange(GetAllTriggerStatus(group));
                } 
            }
            
            return triggerStatuses;
        }

        public IList<ITrigger> GetTriggersForJob(string jobName, string groupName)
        {
            var sched = quartzInstance.GetQuartzScheduler();
            var jk = getJobKey(jobName, groupName, sched);
            return sched.GetTriggersOfJob(jk);
        }

        public JobKey getJobKey(string jobName, string groupName, IScheduler sched)
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var jobKeys = sched.GetJobKeys(groupMatcher);
            return jobKeys.First(x => x.Name == jobName);
        }

    }
}
