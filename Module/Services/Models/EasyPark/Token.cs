using Newtonsoft.Json;
using System;

namespace Module.Services.Models.EasyPark
{
    public class Token
    {
        public Token()
        {
            Loaded = DateTime.Now;
        }

        public string idToken { get; set; }
        public string refreshToken { get; set; }
        public long createdDate { get; set; }
        public long validUntilDate { get; set; }

        [JsonIgnore]
        public DateTime Loaded { get; set; }
    }
}
