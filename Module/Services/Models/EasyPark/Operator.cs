using System.Collections.Generic;

namespace Module.Services.Models.EasyPark
{
    public class Operator
    {
        public int Id { get; set; }

        public string CountryCode { get; set; }

        public List<Area> Areas { get; set; }
    }

    public class Area
    {
        public int AreaNo { get; set; }
        public int Parkings { get; set; }

    }
}
