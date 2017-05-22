using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;

namespace QuartzAdmin.web.Models
{
    using Quartz.Impl.Matchers;

    public class JobRepository
    {
                private InstanceModel quartzInstance;
        public JobRepository(string instanceName)
        {
            InstanceRepository repo = new InstanceRepository();
            quartzInstance = repo.GetInstance(instanceName);
        }

        public JobRepository(InstanceModel instance)
        {
            quartzInstance = instance;
        }


        public IJobDetail GetJob(string jobName, string groupName)
        {
            IScheduler sched = quartzInstance.GetQuartzScheduler();
            JobDataMap jdm = new JobDataMap();
            var jk = getJobKey(jobName, groupName, sched);
            return sched.GetJobDetail(jk);

        }

        public JobKey getJobKey(string jobName, string groupName, IScheduler sched)
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var jobKeys = sched.GetJobKeys(groupMatcher);
            return jobKeys.First(x => x.Name == jobName);
        }

        public void RunJobNow(string jobName, string groupName)
        {
            IScheduler sched = quartzInstance.GetQuartzScheduler();
            var jk = getJobKey(jobName, groupName, sched);
            sched.TriggerJob(jk);
        }
        public void RunJobNow(string jobName, string groupName, JobDataMap jdm)
        {

            IScheduler sched = quartzInstance.GetQuartzScheduler();
            var jk = getJobKey(jobName, groupName, sched);
            sched.TriggerJob(jk, jdm);
        }

        public void DeleteJob(string jobName, string groupName)
        {
            IScheduler sched = quartzInstance.GetQuartzScheduler();
            var jk = getJobKey(jobName, groupName, sched);
            sched.DeleteJob(jk);
        }

        public TriggerKey getTriggerKey(string jobName, string groupName, IScheduler sched)
        {
            var groupMatcher = GroupMatcher<TriggerKey>.GroupContains(groupName);
            var triggerKeys = sched.GetTriggerKeys(groupMatcher);
            return triggerKeys.First(x => x.Name == jobName);
        }
    }
}
