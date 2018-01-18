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


namespace MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Dto.Events
{
    internal class PhaseType
    {
        private readonly string _value;
        private PhaseType(string type)
        {
            _value = type;
        }

        public static PhaseType Post => new PhaseType("post");
        public static PhaseType Internal => new PhaseType("internal");       

        public override string ToString()
        {
            return _value;
        }
    }
}
