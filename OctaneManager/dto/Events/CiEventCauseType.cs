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
	public class CiEventCauseType : IDtoBase
    {
        private string _value;
        private CiEventCauseType(string type)
        {
            _value = type;
        }

        public static CiEventCauseType Scm => new CiEventCauseType("scm");
        public static CiEventCauseType User => new CiEventCauseType("user");
        public static CiEventCauseType Timer => new CiEventCauseType("timer");
        public static CiEventCauseType Upstream => new CiEventCauseType("upstream");
        public static CiEventCauseType Undefined => new CiEventCauseType("undefined");

        public override string ToString()
        {
            return _value;
        }
    }
}
