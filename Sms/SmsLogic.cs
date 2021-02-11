using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Trader.Sms
{
    public class SmsLogic
    {
        private string baseUri = "https://gateway.sms77.io/api/";

        private static readonly HttpClient httpClient = new HttpClient();


        public async Task SendSmsAsync(string text)
        {

            if (!Config.Sms77Active)
                return;

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "basic " + Config.Sms77ApiKey);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("to", Config.Sms77SendTo),
                new KeyValuePair<string, string>("from", "drbor bot"),

                new KeyValuePair<string, string>("text", text)


            };

            var content = new FormUrlEncodedContent(pairs);

            // try
            // {
            //     var result = await httpClient.PostAsync(baseUri + "sms", content);

            //     if (!result.IsSuccessStatusCode)
            //     {
            //         var str = await result.Content.ReadAsStringAsync();
            //         Presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            //     }


            //     var errorCode = await result.Content.ReadAsStringAsync();
            //     if (errorCode != "100")
            //         Presenter.ShowError($"SMS77 failed with error code {errorCode}");

            // }
            // catch (System.Exception ex)
            // {
            //     Presenter.ShowError($"SMS77 failed with error message {ex}");
            // }


        }
    }
}