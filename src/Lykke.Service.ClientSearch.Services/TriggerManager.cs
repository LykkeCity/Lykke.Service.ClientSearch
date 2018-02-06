using Autofac.Extensions.DependencyInjection;
using Lykke.JobTriggers.Triggers;
using Lykke.Service.ClientSearch.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services
{
    public class TriggerManager : ITriggerManager
    {
        private TriggerHost _triggerHost;
        private Task _triggerHostTask;

        private readonly AutofacServiceProvider _autofacServiceProvider;

        public TriggerManager(
            AutofacServiceProvider autofacServiceProvider
            )
        {
            _autofacServiceProvider = autofacServiceProvider;
        }

        public void StartTriggers()
        {
            _triggerHost = new TriggerHost(_autofacServiceProvider);
            _triggerHostTask = _triggerHost.Start();
        }
    }
}
