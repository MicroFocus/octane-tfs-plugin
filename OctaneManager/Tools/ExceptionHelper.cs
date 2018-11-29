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
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Ci.Tfs.Octane;
using System;
using System.Net;
using System.Security.Authentication;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity
{
	public static class ExceptionHelper
	{
		/// <summary>
		/// Check if exception related to communication issues and restart plugin to obtain new connections
		/// <returns>True if restart was done</returns>
		public static bool HandleExceptionAndRestartIfRequired(Exception e, ILog log, string methodName)
		{

			Exception myEx = e;
			if (e is AggregateException && e.InnerException != null)
			{
				myEx = e.InnerException;
			}
			if (myEx is ServerUnavailableException || myEx is InvalidCredentialException || myEx is UnauthorizedAccessException)
			{
				log.Error($"{methodName} failed with {myEx.GetType().Name} : {myEx.Message}");
				PluginManager.GetInstance().RestartPlugin();
				return true;
			}
			//else if (myEx is GeneralHttpException)
			//{
			//	HttpStatusCode status = ((GeneralHttpException)myEx).StatusCode;
			//	log.Error($"{methodName} failed with {myEx.GetType().Name} : {(int)status} {status} - {myEx.Message}");
			//	return false;
			//}
			else
			{
				log.Error($"{methodName} failed with {myEx.GetType().ToString()} : {myEx.Message}", myEx);
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
