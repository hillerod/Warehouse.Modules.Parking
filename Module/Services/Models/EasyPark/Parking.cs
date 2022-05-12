using System;

namespace Module.Services.Models.EasyPark
{
    public class Parking
    {
        public int parkingId { get; set; }
        public int areaNo { get; set; }
        public string areaCountryCode { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string licenseNumber { get; set; }
        public float? parkingFeeExclusiveVAT { get; set; }
        public float? parkingFeeInclusiveVAT { get; set; }
        public float? parkingFeeVAT { get; set; }
        public string currency { get; set; }
        public bool stopped { get; set; }
        public string sourceSystem { get; set; }
        public string subType { get; set; }
        public object spotNumber { get; set; }
        public string areaName { get; set; }
        public object externalParkingId { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
    }
}