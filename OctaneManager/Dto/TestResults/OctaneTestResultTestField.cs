using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    //  <test_field type="Framework" value="TestNG" />
    public class OctaneTestResultTestField
    {
        [XmlAttribute("type")]
        public String Type { get; set; }

        [XmlAttribute("value")]
        public String Value { get; set; }

        public static OctaneTestResultTestField Create(String type, String value)
        {
            OctaneTestResultTestField field = new OctaneTestResultTestField();
            field.Type = type;
            field.Value = value;
            return field;
        }
    }
}
