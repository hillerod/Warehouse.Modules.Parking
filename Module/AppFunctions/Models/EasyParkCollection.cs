using Bygdrift.DataLakeTools;
using Module.Refines;
using Module.Services.Models.EasyPark;
using RepoDb;
using System;
using System.Threading.Tasks;

namespace Module.AppFunctions.Models
{
    public class EasyParkCollection
    {
        public EasyParkCollection(int operatorId, string countryCode, int areaNo, int parkings)
        {
            OperatorId = operatorId;
            CountryCode = countryCode;
            AreaNo = areaNo;
            Parkings = parkings;
        }

        public int OperatorId { get; }
        public string CountryCode { get; }
        public int AreaNo { get; }
        public int Parkings { get; }
        public bool IsLoaded { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public async Task RunAsync()
        {
            if (IsLoaded)
                return;

            var data = await GetDataFromDataLakeOrApiAsync();
            await EasyParkRefine.RefineAsync(TimerTrigger.App, data, OperatorId, AreaNo, Start, End);
            IsLoaded = true;
        }

        /// <summary>If data are already saved in datalake, then use it from ther. Else fetch it from API</summary>
        private async Task<Parking[]> GetDataFromDataLakeOrApiAsync()
        {
            Start = DateTime.Now.Date.AddMonths(-TimerTrigger.App.Settings.EasyParkGoBackMonths);
            End = DateTime.Now.Date.AddDays(1);

            var previousLast = TimerTrigger.App.Mssql.Connection.ExecuteScalar<DateTime?>($"IF OBJECT_ID('{TimerTrigger.App.ModuleName}.EasyPark') IS NOT NULL BEGIN SELECT TOP (1) [Start] FROM [{TimerTrigger.App.ModuleName}].[EasyPark] where AreaNo='{AreaNo}' order by Start desc END;");
            if (previousLast.HasValue && previousLast > DateTime.Now.Date.AddYears(-20))
                Start = ((DateTime)previousLast).Date;

            if (End - Start <= new TimeSpan(24, 0, 0))
                return null;

            Start = Start.AddHours(-24);  //To get an overlap

            var fileName = $"O{OperatorId}_A{AreaNo}_S{Start:yyyy-MM-dd}_E{End:yyyy-MM-dd}.json";
            if (TimerTrigger.App.DataLake.GetJson("RawEasyPark", fileName, FolderStructure.DatePath, out Parking[] data))
            {
                TimerTrigger.App.Log.LogInformation("Get {name} data. Got data from DataLake.", fileName);
                return data;
            }
            else
            {
                TimerTrigger.App.Log.LogInformation("Get {name} data. Loading data from WebService.", fileName);
                var res = await TimerTrigger.EasyParkWebService.GetParkingsAsync(OperatorId, CountryCode, AreaNo, Start, End);
                await TimerTrigger.App.DataLake.SaveObjectAsync(res, "RawEasyPark", fileName, FolderStructure.DatePath);
                return res;
            }
        }
    }
}