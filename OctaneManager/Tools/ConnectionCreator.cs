using Hpe.Nga.Api.Core.Connector;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using System;
using System.Net;
using System.Reflection;
using System.Web;

namespace MicroFocus.Ci.Tfs.Octane.Tools
{
	public class ConnectionCreator
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void CheckMissingValues(ConnectionDetails connectionDetails)
		{
			if (String.IsNullOrEmpty(connectionDetails.WebAppUrl))
			{
				throw new ArgumentException("WebAppUrl is missing");
			}
			if (!connectionDetails.WebAppUrl.Contains("p="))
			{
				throw new ArgumentException("WebAppUrl missing sharedspace id");
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
		}

		public static TfsManager CreateTfsConnection(PluginRunMode runMode, ConnectionDetails connectionDetails)
		{
			var tfsServerUriStr = connectionDetails.TfsLocation;
			if (tfsServerUriStr == null)
			{
				var hostName = Dns.GetHostName();
				var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
				tfsServerUriStr = $"http://{hostName}.{domainName}:8080/tfs/";
			}
			tfsServerUriStr = tfsServerUriStr.EndsWith("/") ? tfsServerUriStr : tfsServerUriStr + "/";
			Uri tfsServerUri = new Uri(tfsServerUriStr);


			TfsManager tfsManager = new TfsManager(runMode, tfsServerUri, connectionDetails.Pat);
			try
			{
				DateTime start = DateTime.Now;
				Log.Debug($"Validate connection to TFS  {tfsServerUri.ToString()}");
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
				Log.Error(msg);
				throw new Exception(msg);
			}

			return tfsManager;
		}

		public static RestConnector CreateOctaneConnection(ConnectionDetails connectionDetails)
		{
			try
			{
				RestConnector restConnector = new RestConnector();
				DateTime start = DateTime.Now;
				Log.Debug($"Validate connection to Octane {connectionDetails.Host}");
				var octaneConnected = restConnector.Connect(connectionDetails.Host, new APIKeyConnectionInfo(connectionDetails.ClientId, connectionDetails.ClientSecret));
				DateTime end = DateTime.Now;
				Log.Debug($"Validate connection to Octane finished in {(long)((end - start).TotalMilliseconds)} ms");
				return restConnector;
			}
			catch (Exception e)
			{
				var msg = "Invalid connection to Octane : " + (e.InnerException != null ? e.InnerException.Message : e.Message);
				Log.Error(msg);
				throw new Exception(msg);
			}
		}
	}
}
