using Hpe.Nga.Api.Core.Connector;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Octane
{
	public class OctaneApis
	{
		private RestConnector _restConnector;
		private ConnectionDetails _connectionDetails;
		private OctaneUriResolver _octaneUriResolver;


		public OctaneApis(RestConnector restConnector, ConnectionDetails connectionDetails)
		{
			_restConnector = restConnector;
			_connectionDetails = connectionDetails;
			_octaneUriResolver = new OctaneUriResolver(connectionDetails);
		}

		public ResponseWrapper GetTask(int pollingTimeout)
		{
			return _restConnector.ExecuteGet(_octaneUriResolver.GetTasksUri(), _octaneUriResolver.GetTaskQueryParams(),
							RequestConfiguration.Create().SetTimeout(pollingTimeout));
		}
	}
}
