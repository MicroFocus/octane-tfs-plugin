using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults
{
    public class OctaneTestResultError
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("message")]
        public string Message { get; set; }
        [XmlTextAttribute]
        public string StackTrace { get; set; }

        public static OctaneTestResultError Create(String type, String message, String stackTrace)
        {
            OctaneTestResultError err = new OctaneTestResultError();
            if (!string.IsNullOrEmpty(type) && !type.Equals("None"))
            {
                err.Type = type;
            }
            err.StackTrace = stackTrace;
            err.Message = message;
            return err;
        }
    }
}
