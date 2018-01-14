using log4net;
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Ci.Tfs.Octane;
using System;
using System.Security.Authentication;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools
{
	public class ExceptionHelper
	{
		/// <summary>
		/// Check if exception related to communication issues and restart plugin to obtaine new connections
		/// <returns>True if restart was done</returns>
		public static bool HandleExceptionAndRestartIfRequired(Exception e, ILog log, string methodName)
		{
			if (e is ServerUnavailableException || e is InvalidCredentialException || e is UnauthorizedAccessException)
			{
				log.Error($"{methodName} failed : {e.Message}");
				PluginManager.GetInstance().RestartPlugin();
				return true;
			}
			else
			{
				log.Error($"{methodName} failed : {e.Message}", e);
				return false;
			}

		}

		public static Exception GetMostInnerException(Exception e)
		{
			Exception myEx = e;
			while (myEx.InnerException != null)
			{
				myEx = myEx.InnerException;
			}
			return myEx;
		}
	}
}
