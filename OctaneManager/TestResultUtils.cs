using MicroFocus.Ci.Tfs.Octane.Dto.TestResults;
using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane
{
    public static class TestResultUtils
    {
        public static String SerializeToXml(OctaneTestResult octaneTestResult)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OctaneTestResult));
            String output;
            using (StringWriter textWriter = new Utf8StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                xmlSerializer.Serialize(textWriter, octaneTestResult, ns);
                return output = textWriter.ToString();
            }
        }

        public static OctaneTestResult ConvertToOctaneTestResult(string serverId, TfsTestResults testResults)
        {
            if (testResults.Count > 0)
            {
                //Serialization prepare
                OctaneTestResult octaneTestResult = new OctaneTestResult();
                TfsItem build = testResults.Results[0].Build;
                TfsItem project = testResults.Results[0].Project;
                octaneTestResult.Build = OctaneTestResultBuild.Create(serverId, int.Parse(build.Id), build.Name, project.Id, project.Name);
                octaneTestResult.TestFields = new List<OctaneTestResultTestField>(new[] { OctaneTestResultTestField.Create("TestLevel", "UnitTest") });
                octaneTestResult.TestRuns = new List<OctaneTestResultTestRun>();
                foreach (TfsTestResult testResult in testResults.Results)
                {
                    OctaneTestResultTestRun run = new OctaneTestResultTestRun();
                    String[] testNameParts = testResult.AutomatedTestName.Split('.');
                    run.Name = testNameParts[testNameParts.Length - 1];
                    run.Class = testNameParts[testNameParts.Length - 2];
                    run.Package = String.Join(".", new ArraySegment<String>(testNameParts, 0, testNameParts.Length - 2));

                    run.Module = Path.GetFileNameWithoutExtension(testResult.AutomatedTestStorage);


                    run.Duration = (long)testResult.DurationInMs;
                    run.Status = testResult.Outcome;
                    if (run.Status.Equals("Failed"))
                    {
                        run.Error = OctaneTestResultError.Create(testResult.ErrorMessage, testResult.StackTrace);
                    }

                    TimeSpan span = testResult.StartedDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                    run.Started = (long)span.TotalSeconds;
                    octaneTestResult.TestRuns.Add(run);
                }
                return octaneTestResult;
            }
            return null;
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
