/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans.v1;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs
{
	public class TfsRestConnector
	{
		private static readonly ILog Log = LogManager.GetLogger(LogUtils.TFS_REST_CALLS_LOGGER);

		private readonly TfsConfiguration _tfsConfiguration;

		public TfsRestConnector(TfsConfiguration tfsConfiguration)
		{
			_tfsConfiguration = tfsConfiguration;
		}

		public T SendPost<T>(string urlSuffix, string data)
		{
			return Send<T>(HttpMethodEnum.POST, urlSuffix, data, null);
		}

		public T SendGet<T>(string urlSuffix)
		{
			return SendGet<T>(urlSuffix, null);
		}

		public T SendGet<T>(string urlSuffix, string resultLoggerName)
		{
			return Send<T>(HttpMethodEnum.GET, urlSuffix, null, resultLoggerName);
		}

		public List<T> GetCollection<T>(string uriSuffix)
		{
			var collections = SendGet<TfsBaseCollection<T>>(uriSuffix, null);
			return collections.Items;
		}

		public List<T> GetPagedCollection<T>(string uriSuffix, int pageSize, int maxPages, string resultLoggerName)
		{
			var top = pageSize;
			var skip = 0;
			var completed = false;
			List<T> finalResults = null;
			var joiner = uriSuffix.Contains("?") ? "&" : "?";
			var pages = 0;
			while (!completed && pages < maxPages)
			{
				var uriSuffixWithPage = ($"{uriSuffix}{joiner}$skip={skip}&$top={top}");
				var results = SendGet<TfsBaseCollection<T>>(uriSuffixWithPage, resultLoggerName);
				skip += top;

				if (finalResults == null)
				{
					finalResults = results.Items;
				}
				else
				{
					finalResults.AddRange(results.Items);
				}
				pages++;
				completed = results.Count < top;
			}
			return finalResults;
		}

		public T Send<T>(HttpMethodEnum httpType, string urlSuffix, string data, string resultLoggerName)
		{
			var start = DateTime.Now;
			HttpStatusCode statusCode = 0;
			var content = "";
			try
			{			    
                //encode your personal access token                   
			    var credentials =
			        !string.IsNullOrEmpty(_tfsConfiguration.Password) ?
			            $"{_tfsConfiguration.Pat}:{_tfsConfiguration.Password}": $":{_tfsConfiguration.Pat}";
			    credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));

                //use the httpclient
                using (var client = new HttpClient())
				{
					client.BaseAddress = _tfsConfiguration.Uri;
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
							var requestContent = new StringContent(data, Encoding.UTF8, "application/json");
							response = client.PostAsync(urlSuffix, requestContent).Result;
							break;
					    case HttpMethodEnum.PUT:
					        break;
					    case HttpMethodEnum.DELETE:
					        break;
					    default:
							Log.Warn("Not supported http type");
                            break;					        
					}



					//check to see if we have a succesfull respond
				    if (response != null)
				    {
				        statusCode = response.StatusCode;
				        content = response.Content.ReadAsStringAsync().Result;
                        if (response.IsSuccessStatusCode)
				        {
				            var result = JsonHelper.DeserializeObject<T>(content);
				            return result;
				        }
				        else
				            switch (response.StatusCode)
				            {
				                case HttpStatusCode.Unauthorized:
				                    throw new UnauthorizedAccessException(
				                        "TFS PAT is not valid or does not have required permissions.");
				                case HttpStatusCode.NotFound:
				                    throw new HttpException(404,
				                        $"Url is not found : {_tfsConfiguration.Uri.ToString()}{urlSuffix}");
				                default:
				                    var msg = $"Failed to set {httpType} {urlSuffix} : {content})";
				                    Log.Error(msg);
				                    throw new Exception(msg);
				            }
				    }
				    else
				    {
				        var msg = $"Response object was null! {httpType} {urlSuffix} : {content})";
				        Log.Error(msg);
				        throw new Exception(msg);
                    }
				}
			}
			finally
			{
			    var end = DateTime.Now;
			    var timeMsStr = $"{(long) (end - start).TotalMilliseconds,7} ms";
			    var responseSize = $"{content.Length,7} B";
			    Log.Info($"{(int)statusCode} | {timeMsStr} | {responseSize} |  {httpType}:{urlSuffix}");

			    if (resultLoggerName != null)
			    {
			        LogManager.GetLogger(resultLoggerName).Debug($"{(int)statusCode} | {timeMsStr} | {responseSize} | {httpType}:{urlSuffix} | {content}");
			    }
			}
        }
	}
}