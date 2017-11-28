using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    public class OctaneTestResultError
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("message")]
        public string Message { get; set; }
        [XmlTextAttribute]
        public string StackTrace { get; set; }

        public static OctaneTestResultError Create(String message, String stackTrace)
        {
            OctaneTestResultError err = new OctaneTestResultError();
            err.StackTrace = stackTrace;
            err.Message = message;
            return err;
        }
    }
}
