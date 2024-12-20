
using CSharpFunctionalExtensions;
using System.Net;
using Newtonsoft.Json;

namespace Agenciapp.Common.Request
{
    public static class RequestApi
    {
        public static async Task<Result<string>> SendPostAsync<T>(string url, T objectRequest, string method = "POST")
        {

            string json = JsonConvert.SerializeObject(objectRequest);

            WebRequest request = WebRequest.Create(url);
            request.Method = method;
            request.PreAuthenticate = true;
            request.ContentType = "application/json;charset=utf-8'";
            request.Timeout = 10000;

            using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var httpResponse = (HttpWebResponse)await request.GetResponseAsync();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return Result.Success(result);
        }

        public static async Task<Result<string>> SendGetAsync(string url, Dictionary<string,string> parameters)
        {
            String username = "010087";
            String password = "Conv $ 2021";
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

            WebClient webClient = new WebClient();
            webClient.Headers.Add("Authorization", "Basic " + encoded);

            /*if (parameters  != null)
            {
                foreach (var item in parameters)
                {
                    webClient.QueryString.Add(item.Key, item.Value);
                }
            }*/

            string result = webClient.DownloadString(url);
            return Result.Success(result);
        }
    }
}

