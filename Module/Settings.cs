using Bygdrift.Warehouse.Helpers.Attributes;
using Module.Services.Models.EasyPark;
using System.Collections.Generic;

namespace Module
{
    public class Settings
    {
        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string EasyParkUser { get; set; }

        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string EasyParkPassword { get; set; }

        [ConfigSetting(IsJson = true, NotSet = NotSet.ThrowError)]
        public List<Operator> EasyParkOperators { get; set; }
        
        [ConfigSetting(Default = 6, NotSet = NotSet.ShowLogInfo, ErrorMessage = "EasyParkGoBackMonths is not set. A default value on 6 is used.")]
        public int EasyParkGoBackMonths { get; set; }
    }
}
