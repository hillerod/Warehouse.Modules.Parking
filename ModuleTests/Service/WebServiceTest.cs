using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module;
using Module.Services;
using Module.Services.Models.EasyPark;
using Newtonsoft.Json;
using RepoDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.Service
{
    [TestClass]
    public class WebServiceTest
    {
        private readonly EasyParkWebService service;
        private readonly AppBase<Settings> app = new();

        /// <summary>Path to project base</summary>
        public static readonly string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        public WebServiceTest()
        {
            service = new EasyParkWebService(app);
        }

        [TestMethod]
        public async Task GetDataAsync()
        {
            var a = app.Settings.EasyParkOperators;
            var b = a.First().Areas.First();

            try
            {
                var er = app.Mssql.Connection.ExecuteScalar<DateTime?>($"SELECT TOP (1) [Start] FROM [{app.ModuleName}j].[EasyPark] where AreaNo=3400 order by Start desc");
            }
            catch (Exception)
            {
            }


            //var start = DateTime.Now.AddMonths(-app.Settings.EasyParkGoBackMonths);
            //var end = DateTime.Now;

            //var previousFirstLast = app.Mssql.GetFirstAndLastAscending<DateTime>("EasyPark", "Start", false);
            ////if (previousFirstLast.Last > DateTime.Now.AddYears(-20))
            ////    start = previousFirstLast.Last;

            //start = DateTime.Now.AddDays(-3);
            //end = DateTime.Now;

            //if (end - start > new TimeSpan(24, 0, 0))
            //{
            //    start = start.AddHours(-24);  //To get an overlap
            //    var res = await service.GetParkingsAsync(start, end);
            //    await Module.Refines.EasyParkRefine.RefineAsync(app, res);
            //}
            //var errors = app.Log.GetErrorsAndCriticals().ToList();
            //Assert.IsFalse(errors.Any());
        }


        private void SaveToFile<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var fileName = typeof(T).Name + ".json";
            var filePath = Path.Combine(BasePath, "Files", "In", fileName);
            File.WriteAllText(filePath, json);
        }
    }
}
