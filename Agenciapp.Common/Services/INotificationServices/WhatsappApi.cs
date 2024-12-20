namespace Agenciapp.Common.Services.INotificationServices
{
    internal class WhatsappApi
    {
        private readonly string _baseUrl;
        private readonly string _token;
        public WhatsappApi(string instanceId, string token)
        {
            this._baseUrl = "https://api.ultramsg.com/" + instanceId;
            this._token = token;
        }

        public void SendWhatsappMessage(string to, string message)
        {
            using(HttpClient client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("content-type", "application/x-www-form-urlencoded");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", _token),
                    new KeyValuePair<string, string>("to", to),
                    new KeyValuePair<string, string>("body", message)
                });
                var result = client.PostAsync(_baseUrl + "/messages/chat", content).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending message: " + resultContent);
                }
            }
        }

    }
}
