using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Logging;
using Module.AppFunctions.Models;
using Module.Services;

namespace Module.AppFunctions
{
    public class TimerTrigger
    {
        public TimerTrigger(ILogger<TimerTrigger> logger, IDurableClientFactory clientFactory)
        {
            if (App == null)
                App = new AppBase<Settings>(logger);

            if (EasyParkWebService == null)
                EasyParkWebService = new EasyParkWebService(App);

            if (Client == null)
                Client = clientFactory.CreateClient(new DurableClientOptions { TaskHub = App.ModuleName });
        }

        public static AppBase<Settings> App { get; private set; }
        public static EasyParkWebService EasyParkWebService { get; private set; }

        public static IDurableClient Client { get; private set; }

        [FunctionName(nameof(Starter))]
        public async Task Starter([TimerTrigger("%ScheduleExpression%"
#if DEBUG
            ,RunOnStartup = true
#endif
            )] TimerInfo timerInfo)
        {
            if (await Basic.IsRunning(App, Client)) return;
            App.LoadedUtc = DateTime.UtcNow;
            App.Config["QualifiedInstanceId"] = await Client.StartNewAsync(nameof(RunOrchestrator));
        }

        [FunctionName(nameof(RunOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (!Basic.IsQualifiedInstance(App, context.InstanceId)) return;
            if (!context.IsReplaying)
                App.Log.LogInformation("Has started instance {Instance}", context.InstanceId);

            var easyParks = new List<EasyParkCollection>();
            foreach (var @operator in App.Settings.EasyParkOperators)
                foreach (var item in @operator.Areas)
                    easyParks.Add(new EasyParkCollection(@operator.Id, @operator.CountryCode, item.AreaNo, item.Parkings));

            for (int i = 0; i <= 100; i++)
            {
                if (easyParks.All(o=> o.IsLoaded))
                    break;

                easyParks = await context.CallActivityAsync<List<EasyParkCollection>>(nameof(HandleDataAsync), easyParks);
            }

            if (!easyParks.All(o=> o.IsLoaded))
            {
                var finished = string.Join(',', easyParks.Where(o => o.IsLoaded).Select(o => o.AreaNo));
                var missing = string.Join(',', easyParks.Where(o => !o.IsLoaded).Select(o => o.AreaNo));
                App.Log.LogError("The orchestrator did not finish the run with the preestimated {Runs} runs. Has finished: {Finished}. Missing: {Missing}", 100, finished, missing);
            }

            App.Log.LogInformation($"Finished reading in {context.CurrentUtcDateTime.Subtract(App.LoadedUtc).TotalSeconds / 60} minutes.");
        }



        [FunctionName(nameof(HandleDataAsync))]
        public async Task<List<EasyParkCollection>> HandleDataAsync([ActivityTrigger] IDurableActivityContext context)
        {
            if (!Basic.IsQualifiedInstance(App, context.InstanceId)) return default;
            var easyParks = context.GetInput<List<EasyParkCollection>>();

            var started = DateTime.Now;
            foreach (var item in easyParks.Where(o => !o.IsLoaded))
            {
                await item.RunAsync();

                if (started.AddMinutes(5) < DateTime.Now)
                    break;
            }

            return easyParks;
        }
    }
}