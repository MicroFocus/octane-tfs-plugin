using MicroFocus.Ci.Tfs.Octane.Tfs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
	public class TfsHttpConnector
	{
		private readonly TfsConfiguration _tfsConf;

		public TfsHttpConnector(TfsConfiguration tfsConfiguration)
		{
			_tfsConf = tfsConfiguration;
		}

		public TfsHttpConnector Create(TfsConfiguration tfsConfiguration)
		{
			return new TfsHttpConnector(tfsConfiguration);
		}

		public T SendPost<T>(string urlSuffix, string data)
		{
			return Send<T>(HttpMethodEnum.POST, urlSuffix, data);
		}

		public T SendGet<T>(string urlSuffix)
		{
			return Send<T>(HttpMethodEnum.GET, urlSuffix, null);
		}

		public T Send<T>(HttpMethodEnum httpType, string urlSuffix, string data)
		{
			//encode your personal access token                   
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _tfsConf.Pat)));


			//use the httpclient
			using (var client = new HttpClient())
			{
				client.BaseAddress = _tfsConf.Uri;
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
				HttpResponseMessage response = null;

				switch (httpType)
				{
					case HttpMethodEnum.GET:
						response = client.GetAsync(urlSuffix, HttpCompletionOption.ResponseContentRead).Result;
						break;
					case HttpMethodEnum.POST:
						StringContent requestContent = new StringContent(data, Encoding.UTF8, "application/json");
						response = client.PostAsync(urlSuffix, requestContent).Result;
						break;
					default:
						throw new NotSupportedException("Not supported http type");
				}



				//check to see if we have a succesfull respond
				string content = response.Content.ReadAsStringAsync().Result;
				if (response.IsSuccessStatusCode)
				{
					T result = JsonConvert.DeserializeObject<T>(content);
					return result;
				}
				else
				{
					String msg = $"Failed to set {httpType} {urlSuffix} : {content})";
					Trace.WriteLine(msg);
					throw new Exception(msg);
				}
			}
		}
	}
}