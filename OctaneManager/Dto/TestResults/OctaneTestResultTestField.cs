using System;
using System.Xml.Serialization;

namespace MicroFocus.Ci.Tfs.Octane.Dto.TestResults
{
    //  <test_field type="Framework" value="TestNG" />
    public class OctaneTestResultTestField
    {
        public static string FRAMEWORK = "Framework";
        public static string TEST_LEVEL_TYPE = "Test_Level";
        public static string TESTING_TOOL_TYPE_TYPE = "Testing_Tool_Type";

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
