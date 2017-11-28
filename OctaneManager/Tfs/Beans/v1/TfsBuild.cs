﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems
{
    public class TfsBuild
    {
        public int Id { get; set; }
        [JsonProperty("buildNumber")]
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
    }
}