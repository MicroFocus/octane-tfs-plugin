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
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tfs.Beans;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Octane
{
    public static class OctaneTestResutsUtils
	{
		private static readonly ILog Log = LogManager.GetLogger(LogUtils.OCTANE_TEST_RESULTS_LOGGER);

		public static string SerializeToXml(OctaneTestResult octaneTestResult)
		{
			var xmlSerializer = new XmlSerializer(typeof(OctaneTestResult));
			using (StringWriter textWriter = new Utf8StringWriter())
			{
				var ns = new XmlSerializerNamespaces();
				ns.Add("", "");
				xmlSerializer.Serialize(textWriter, octaneTestResult, ns);
				string result = textWriter.ToString();
				Log.Debug(result);

				return result;
			}
		}

		public static OctaneTestResult ConvertToOctaneTestResult(string serverId, string projectCiId, string buildCiId, IList<TfsTestResult> testResults, string runWebAccessUrl)
		{
			if (testResults.Count <= 0) return null;
			//Serialization prepare
			var octaneTestResult = new OctaneTestResult();
			var build = testResults[0].Build;
			var project = testResults[0].Project;
			octaneTestResult.Build = OctaneTestResultBuild.Create(serverId, buildCiId, projectCiId);
			/*octaneTestResult.TestFields = new List<OctaneTestResultTestField>(new[] {
		        OctaneTestResultTestField.Create(OctaneTestResultTestField.TEST_LEVEL_TYPE, "UnitTest")
		    });*/

			octaneTestResult.TestRuns = new List<OctaneTestResultTestRun>();
			foreach (var testResult in testResults)
			{
				var run = new OctaneTestResultTestRun();
				if (testResult.AutomatedTestType.Equals("JUnit"))
				{
					var testNameParts = testResult.AutomatedTestStorage.Split('.');
					run.Name = testResult.AutomatedTestName;
					run.Class = testNameParts[testNameParts.Length - 1];
					run.Package = String.Join(".", new ArraySegment<String>(testNameParts, 0, testNameParts.Length - 1));
				}
				else // UnitTest
				{
					var testNameParts = testResult.AutomatedTestName.Split('.');
					run.Name = testNameParts[testNameParts.Length - 1];
					run.Class = testNameParts[testNameParts.Length - 2];
					run.Package = String.Join(".", new ArraySegment<String>(testNameParts, 0, testNameParts.Length - 2));
					run.Module = Path.GetFileNameWithoutExtension(testResult.AutomatedTestStorage);
				}


				run.Duration = (long)testResult.DurationInMs;
				run.Status = testResult.Outcome;
				if (run.Status.Equals("NotExecuted"))
				{
					run.Status = "Skipped";
				}

				if (run.Status.Equals("Failed"))
				{
				    if (testResult.FailureType == "None" || String.IsNullOrEmpty(testResult.FailureType))
				    {
				        testResult.FailureType = FindExceptionName(testResult.StackTrace);
				    }
                    
                    run.Error = OctaneTestResultError.Create(testResult.FailureType, testResult.ErrorMessage, testResult.StackTrace);
				}

				run.Started = OctaneUtils.ConvertToOctaneTime(testResult.StartedDate);

				if (!string.IsNullOrEmpty(runWebAccessUrl))
				{
					//Run WebAccessUrl        http://berkovir:8080/tfs/DefaultCollection/Test2/_TestManagement/Runs#runId=8&_a=runCharts
					//Run Result WebAccessUrl http://berkovir:8080/tfs/DefaultCollection/Test2/_TestManagement/Runs#runId=8&_a=resultSummary&resultId=100000
					run.ExternalReportUrl = runWebAccessUrl.Replace("_a=runCharts", ($"_a=resultSummary&resultId={testResult.Id}"));
				}

				octaneTestResult.TestRuns.Add(run);
			}
			return octaneTestResult;
		}

	    public static string FindExceptionName(string input)
	    {
            var retVal = "Exception";
            if (!string.IsNullOrEmpty(input) && input.IndexOf(":", StringComparison.Ordinal) > 0)
	        {
	            retVal  = input.Substring(0, input.IndexOf(":", StringComparison.Ordinal));
            }
            return  retVal;
	    }

	}
}
