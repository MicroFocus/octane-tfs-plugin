using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane;

namespace TfsConsolePluginRunner
{
    class Program
    {
        private static readonly TfsManager _tfsManager = new TfsManager();
        public static void GetJobsList()
        {
            //_tfsManager.ListProjectsInCollection();
            _tfsManager.GetJobsList();
        }
        static void Main(string[] args)
        {
            GetJobsList();
        }
    }
}
