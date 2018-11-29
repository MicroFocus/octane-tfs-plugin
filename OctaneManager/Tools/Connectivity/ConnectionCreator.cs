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
using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Web;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity
{
    public class ConnectionCreator
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void InitRestConnectorForUI()
        {
            NetworkSettings.EnableAllSecurityProtocols();
            NetworkSettings.IgnoreServerCertificateValidation();
            RestConnector.AwaitContinueOnCapturedContext = false;
        }

		public static void CheckMissingValues(ConnectionDetails connectionDetails)
		{
			if (string.IsNullOrEmpty(connectionDetails.ALMOctaneUrl))
			{
				throw new ArgumentException("ALM Octane Url is not specified");
			}
			if (!connectionDetails.ALMOctaneUrl.Contains("p="))
			{
				throw new ArgumentException("ALM Octane Url is missing sharedspace id");
			}
			if (string.IsNullOrEmpty(connectionDetails.ClientId))
			{
				throw new ArgumentException("ClientId is missing");
			}
			if (string.IsNullOrEmpty(connectionDetails.ClientSecret))
			{
				throw new ArgumentException("Client secret is missing");
			}
			if (string.IsNullOrEmpty(connectionDetails.Pat))
			{                
				//throw new ArgumentException("Pat is missing");
			}
			if (string.IsNullOrEmpty(connectionDetails.TfsLocation))
			{
			    throw new ArgumentException("TFS Location not specified");
            }
			if (connectionDetails.TfsLocation.Contains("localhost"))
			{
				throw new ArgumentException("TFS Location should contain external domain and not 'localhost'");
			}
			if (string.IsNullOrEmpty(connectionDetails.InstanceId))
			{
				throw new ArgumentException("InstanceId is missing");
			}
			if (connectionDetails.InstanceId.Length > 40)
			{
				throw new ArgumentException("InstanceId length must be less than or equal to 40 characters");
			}
		}

		public static TfsApis CreateTfsConnection(ConnectionDetails connectionDetails)
		{
			var tfsServerUriStr = connectionDetails.TfsLocation ?? GetTfsLocationFromHostName();
		    var tfsManager = new TfsApis(tfsServerUriStr, connectionDetails.Pat,connectionDetails.Password);
			try
			{
				var start = DateTime.Now;
				Log.Debug($"Validate connection to TFS  {tfsServerUriStr}");
				tfsManager.ConnectionValidation("connection-validation");
				var end = DateTime.Now;
				Log.Debug($"Validate connection to TFS finished in {(long)((end - start).TotalMilliseconds)} ms");
			}
			catch (Exception e)
			{
                const string tfsNotFoundError = "Please check that TFS Location URL is valid and TFS server is online";
			    const string msgPrefix = "Generic error connecting to TFS server: ";

			    var innerException = e.InnerException;
			    if (innerException is HttpRequestException)
			    {
			        Log.Error(tfsNotFoundError);
			        throw new Exception(tfsNotFoundError);
                }

				if (e is HttpException httpException)
				{
				    if (httpException.GetHttpCode() == 404)
					{
						Log.Error(tfsNotFoundError);
						throw new Exception(tfsNotFoundError);
					}
				}
				var msg = msgPrefix + (e.InnerException != null ? e.InnerException.Message : e.Message);
				throw new Exception(msg);
			}

			return tfsManager;
		}

        public static string GetTfsLocationFromHostName()
        {
            var hostName = Dns.GetHostName();
            var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var fullHost = hostName + (String.IsNullOrEmpty(domainName) ? "" : "." + domainName);
            var result = $"http://{fullHost}:8080/tfs/";
            return result;
        }

        public static OctaneApis CreateOctaneConnection(ConnectionDetails connectionDetails)
		{

			Log.Debug($"Validate connection to Octane {connectionDetails.Host} and sharedspace {connectionDetails.SharedSpace}");
			var start = DateTime.Now;
			var restConnector = new RestConnector();

			//1.validate connectivity
			try
			{
				restConnector.Connect(connectionDetails.Host, new APIKeyConnectionInfo(connectionDetails.ClientId, connectionDetails.ClientSecret));			   
			}
			catch (Exception ex)
			{
				var innerException = ExceptionHelper.GetMostInnerException(ex);
			    string msg;

			    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			    if (innerException.Message.Contains("401"))
			    {

			        msg = $"Connection to ALM Octane not authorized, please check ALM Octane client id and secret!";
			    }
                else if(innerException.Message.Contains("No connection could be made because the target machine actively refused it"))
			    {
			        msg = $"ALM Octane server ({connectionDetails.Host}) could not be reached! Please check ALM Octane is availble on specified port.";
                }
                else if (innerException.Message.Contains("The handshake failed due to an unexpected packet format."))
			    {
			        if (connectionDetails.Host.Contains("https"))
			        {
			            msg =
			                $"ALM Octane server ({connectionDetails.Host}) could not be reached! Please check if server supports https connection";
			        }
			        else
			        {
			            msg =
			                $"ALM Octane server ({connectionDetails.Host}) could not be reached (seems like a http/https problem) ! Please check connection url and port";
                    }
			    }
			    else
			    {
			        msg =
			            $"ALM Octane server ({connectionDetails.Host}) could not be reached\n please check ALM Octane location Url\\IP and proxy settings";
			    }
				
                Log.Error(msg,innerException);

				throw new Exception(msg,innerException);
			}

			//2.validate sharedspace exist
			try
			{
				var workspacesUrl = $"/api/shared_spaces/{connectionDetails.SharedSpace}/workspaces?limit=1";
				restConnector.ExecuteGet(workspacesUrl, null);
			}
			catch (Exception ex)
			{
				var msg = $"Could not connect to ALM Octane : sharedspace {connectionDetails.SharedSpace} does not exist";
			    Log.Error(msg, ex);
                throw new Exception(msg);
			}

            //validate authorization
		    try
		    {
		        string request = $"/internal-api/shared_spaces/{connectionDetails.SharedSpace}/analytics/ci/servers/connectivity/status";
		        restConnector.ExecuteGet(request, null);                
		    }
		    catch (Exception ex)
		    {
		        if (ex.InnerException is MqmRestException restEx && restEx.StatusCode == HttpStatusCode.Forbidden)
		        {
		            const string msg = "Could not connect to ALM Octane : Provided credentials are not sufficient for requested resource";
                    Log.Error(msg);
		            throw new Exception(msg);
		        }
		        else
		        {
		            var msg = $"Could not connect to ALM Octane (generic error): {ex.Message}";

                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }		        		        
		    }

            var end = DateTime.Now;
			Log.Debug($"Validate connection to Octane finished in {(long)((end - start).TotalMilliseconds)} ms");
			return new OctaneApis(restConnector, connectionDetails);
		}
	}
}
