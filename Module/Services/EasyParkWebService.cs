using Bygdrift.Warehouse;
using Module.Services.Models.EasyPark;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

//Documentation: https://external-gw.easyparksystem.net/api/swagger-ui.html
namespace Module.Services
{
    public class EasyParkWebService
    {
        private HttpClient _client;
        private readonly string refresTokenUrl = "https://sso.easyparksystem.net/api/login";
        private Token token;
        private readonly string baseUrl = "https://external-gw.easyparksystem.net/api/";
        public HttpResponseMessage ClientResponse { get; private set; }

        public AppBase<Settings> App { get; }

        public EasyParkWebService(AppBase<Settings> app) => App = app;

        public HttpClient Client
        {
            get
            {
                if (token == null || DateTime.Now.Subtract(token.Loaded).TotalHours > 23)
                    UpdateRefreshToken();

                if (_client == null)
                    _client = GetHttpClient();

                return _client;
            }
        }

        public void UpdateRefreshToken()
        {
            var user = "{\"userName\":\"" + App.Settings.EasyParkUser + "\", \"password\":\"" + App.Settings.EasyParkPassword + "\"}";
            var data = new StringContent(user, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = client.PostAsync(refresTokenUrl, data).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                App.Log.LogError($"The EasyParkWebservice failed to fetch refreshToken. Error: {ClientResponse.ReasonPhrase}.");
                return;
            }
            var json = response.Content.ReadAsStringAsync().Result;
            token = JsonConvert.DeserializeObject<Token>(json);
        }

        private HttpClient GetHttpClient()
        {
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler);
            client.Timeout = new TimeSpan(10, 0, 0);
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("X-Authorization", "Bearer " + token.idToken);
            return client;
        }

        public async Task<Parking[]> GetParkingsAsync(int operatorId, string countryCode, int areaNo, DateTime start, DateTime end)
        {
            var response = await Client.GetAsync($"export/operator-parkings-full?operatorId={operatorId}&countryCode={countryCode}&areaNo={areaNo}&from={start:O}&to={end:O}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                App.Log.LogError($"The webservice export/operator-parkings-standard failed. Error: {response.ReasonPhrase}.");
                return default;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Parking[]>(json);
        }
    }
}
