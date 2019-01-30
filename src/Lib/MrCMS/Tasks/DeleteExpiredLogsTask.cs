using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Logging;
using MrCMS.Settings;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Tasks
{
    public class DeleteExpiredLogsTask : SchedulableTask
    {
        private readonly SiteSettings _siteSettings;
        private readonly IStatelessSession _statelessSession;
        private readonly IGetDateTimeNow _getDateTimeNow;

        public DeleteExpiredLogsTask(SiteSettings siteSettings, IStatelessSession statelessSession, IGetDateTimeNow getDateTimeNow)
        {
            _siteSettings = siteSettings;
            _statelessSession = statelessSession;
            _getDateTimeNow = getDateTimeNow;
        }

        public override int Priority { get { return 0; } }

        protected override void OnExecute()
        {
            var logs = GetLogs();

            while (logs.Any())
            {
                IList<Log> currentLogs = logs;
                _statelessSession.Transact(session =>
                {
                    foreach (var log in currentLogs)
                    {
                        _statelessSession.Delete(log);
                    }

                });
                logs = GetLogs();
            }
        }

        private IList<Log> GetLogs()
        {
            var now = _getDateTimeNow.LocalNow;
            return
                _statelessSession.QueryOver<Log>()
                    .Where(data => data.CreatedOn <= now.AddDays(-_siteSettings.DaysToKeepLogs))
                    .Take(1000)
                    .List();
        }
    }
}