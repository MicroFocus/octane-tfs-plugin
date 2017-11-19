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
        //private static readonly TfsManager _tfsManager = new TfsManager();
        private static OctaneManager _octaneManager;

        static void Main(string[] args)
        {
            _octaneManager = new OctaneManager(9999);
            _octaneManager.Init();

            Console.WriteLine("TFS plugin is running , press any key to exit...");
            Console.ReadLine();
            _octaneManager.ShutDown();
            _octaneManager.WaitShutdown();
        }
    }
}
