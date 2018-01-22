﻿/*!
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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs;
using System;
using System.Net;
using System.Reflection;
using System.Web;
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public class ConnectionCreator
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void CheckMissingValues(ConnectionDetails connectionDetails)
		{
			if (String.IsNullOrEmpty(connectionDetails.ALMOctaneUrl))
			{
				throw new ArgumentException("ALMOctaneUrl is missing");
			}
			if (!connectionDetails.ALMOctaneUrl.Contains("p="))
			{
				throw new ArgumentException("ALMOctaneUrl missing sharedspace id");
			}
			if (String.IsNullOrEmpty(connectionDetails.ClientId))
			{
				throw new ArgumentException("ClientId is missing");
			}
			if (String.IsNullOrEmpty(connectionDetails.ClientSecret))
			{
				throw new ArgumentException("Client secret is missing");
			}
			if (String.IsNullOrEmpty(connectionDetails.Pat))
			{
				throw new ArgumentException("Pat is missing");
			}
			if (String.IsNullOrEmpty(connectionDetails.TfsLocation))
			{
				throw new ArgumentException("TfsLocation is missing");
			}
			if (connectionDetails.TfsLocation.Contains("localhost"))
			{
				throw new ArgumentException("TfsLocation should contain external domain and not 'localhost'");
			}
			if (String.IsNullOrEmpty(connectionDetails.InstanceId))
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
			var tfsServerUriStr = connectionDetails.TfsLocation;
			if (tfsServerUriStr == null)
			{
				tfsServerUriStr = GetTfsLocationFromHostName();
			}
			TfsApis tfsManager = new TfsApis(tfsServerUriStr, connectionDetails.Pat);
			try
			{
				DateTime start = DateTime.Now;
				Log.Debug($"Validate connection to TFS  {tfsServerUriStr}");
				tfsManager.GetProjectCollections();
				DateTime end = DateTime.Now;
				Log.Debug($"Validate connection to TFS finished in {(long)((end - start).TotalMilliseconds)} ms");
			}
			catch (Exception e)
			{
				string msgPrefix = "Invalid connection to TFS : ";
				if (e is HttpException)
				{
					HttpException httpException = (HttpException)e;
					if (httpException.GetHttpCode() == 404)
					{
						Log.Error("Tfs location is invalid");
						throw new Exception(msgPrefix + "TFS location is invalid");
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
			var result = $"http://{hostName}.{domainName}:8080/tfs/";
			return result;
		}

		public static OctaneApis CreateOctaneConnection(ConnectionDetails connectionDetails)
		{

			Log.Debug($"Validate connection to Octane {connectionDetails.Host} and sharedspace {connectionDetails.SharedSpace}");
			DateTime start = DateTime.Now;
			RestConnector restConnector = new RestConnector();

			//1.validate connectivity
			try
			{
				restConnector.Connect(connectionDetails.Host, new APIKeyConnectionInfo(connectionDetails.ClientId, connectionDetails.ClientSecret));			   
			}
			catch (Exception e)
			{
				Exception innerException = ExceptionHelper.GetMostInnerException(e);
				var msg = $"Invalid connection to Octane : {innerException.Message}";
				throw new Exception(msg);
			}

			//2.validate sharedspace exist
			try
			{
				string workspacesUrl = $"/api/shared_spaces/{connectionDetails.SharedSpace}/workspaces?limit=1";
				restConnector.ExecuteGet(workspacesUrl, null);
			}
			catch (Exception)
			{
				var msg = $"Invalid connection to Octane : sharedspace {connectionDetails.SharedSpace} does not exist";
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
                MqmRestException restEx = ex.InnerException as MqmRestException;
		        if (restEx.StatusCode == HttpStatusCode.Forbidden)
		        {
		            throw new Exception("Provided credentials are not sufficient for requested resource");
		        }
		        else
		        {
		            throw new Exception(ex.Message);
                }		        		        
		    }

            DateTime end = DateTime.Now;
			Log.Debug($"Validate connection to Octane finished in {(long)((end - start).TotalMilliseconds)} ms");
			return new OctaneApis(restConnector, connectionDetails);
		}
	}
}
