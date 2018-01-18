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
using Nancy;
using Nancy.Bootstrapper;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.RestServer
{
	/*public class NancyGzipCompression : IApplicationStartup
	{
		public void Initialize(IPipelines pipelines)
		{
			pipelines.AfterRequest += CheckForCompression;
		}

		private static void CheckForCompression(NancyContext context)
		{
			if (!RequestIsGzipCompatible(context.Request))
			{
				return;
			}

			if (context.Response.StatusCode != HttpStatusCode.OK)
			{
				return;
			}

			if (!ResponseIsCompatibleMimeType(context.Response))
			{
				return;
			}

			if (ContentLengthIsTooSmall(context.Response))
			{
				return;
			}

			CompressResponse(context.Response);
		}

		private static void CompressResponse(Response response)
		{
			response.Headers["Content-Encoding"] = "gzip";

			var contents = response.Contents;
			response.Contents = responseStream =>
			{
				using (var compression = new GZipStream(responseStream, CompressionMode.Compress))
				{
					contents(compression);
				}
			};
		}

		private static bool ContentLengthIsTooSmall(Response response)
		{
			string contentLength;
			if (response.Headers.TryGetValue("Content-Length", out contentLength))
			{
				var length = long.Parse(contentLength);
				if (length < 4096)
				{
					return true;
				}
			}
			return false;
		}

		private static readonly List<string> ValidMimes = new List<string> {"application/json", "application/javascript" };// "text/plain", 

		private static bool ResponseIsCompatibleMimeType(Response response)
		{
			return ValidMimes.Any(x => x == response.ContentType);
		}

		private static bool RequestIsGzipCompatible(Request request)
		{
			return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
		}
	}*/
}