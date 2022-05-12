using Bygdrift.CsvTools;
using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using Module.Services.Models.EasyPark;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Module.Refines
{
    public static class EasyParkRefine
    {
        public static async Task<Csv> RefineAsync(AppBase app, Parking[] data, int operatorId, int areaNo, DateTime start, DateTime end)
        {
            if (!data.Any())
                return null;

            var csv = CreateCsv(data, operatorId);
            var fileName = $"O{operatorId}_A{areaNo}_S{start:yyyy-MM-dd}_E{end:yyyy-MM-dd}.csv";
            await app.DataLake.SaveCsvAsync(csv, "RefinedEasyPark", fileName, FolderStructure.DatePath);
            app.Mssql.MergeCsv(csv, "EasyPark", "ParkingId", false, false);
            return csv;
        }

        private static Csv CreateCsv(Parking[] data, int operatorId)
        {
            var csv = new Csv("ParkingId, OperatorId, AreaNo, Start, End, AreaName, LicensNumber, ParkingFee, ParkingFeeVAT, Stopped, SpotNumber, SourceSystem, SubType, Lat, Lon");
            foreach (var i in data)
                csv.AddRow(i.parkingId, operatorId, i.areaNo, i.startDate, i.endDate, i.areaName, i.licenseNumber, i.parkingFeeExclusiveVAT ?? 0, i.parkingFeeVAT ?? 0, i.stopped, i.spotNumber, i.sourceSystem, i.subType, i.latitude, i.longitude);

            return csv;
        }
    }
}