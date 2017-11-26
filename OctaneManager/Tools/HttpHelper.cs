using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
    class HttpHelper
    {
        public static HttpClient GetHttpClient(Uri tfsUri, string pat)
        {
            string credentials =
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"));


            //use the httpclient
            var client = new HttpClient
            {
                BaseAddress = tfsUri
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            return client;
        }
    }
}