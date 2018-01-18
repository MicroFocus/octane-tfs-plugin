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
using System;
using System.Xml.Serialization;

namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.TestResults
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
