using Hpe.Nga.Api.Core.Connector;
using log4net;
using MicroFocus.Ci.Tfs.Octane.Configuration;
using System;
using System.Net;
using System.Reflection;


namespace MicroFocus.Ci.Tfs.Octane.Tools
{
	public class ConnectionCreator
	{
		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void CheckMissingValues(ConnectionDetails connectionDetails)
		{
			if (String.IsNullOrEmpty(connectionDetails.WebAppUrl))
			{
				throw new ArgumentException("WebAppUrl is empty");
			}
			if (String.IsNullOrEmpty(connectionDetails.ClientId))
			{
				throw new ArgumentException("ClientId is empty");
			}
			if (String.IsNullOrEmpty(connectionDetails.Pat))
			{
				throw new ArgumentException("Pat is empty");
			}
			if (String.IsNullOrEmpty(connectionDetails.TfsLocation))
			{
				throw new ArgumentException("TfsLocation is empty");
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
				var msg = "Invalid connection to TFS : " + (e.InnerException != null ? e.InnerException.Message : e.Message);
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
