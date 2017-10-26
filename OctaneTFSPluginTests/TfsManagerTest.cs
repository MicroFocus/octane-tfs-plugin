﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroFocus.Ci.Tfs.Octane;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Ci.Tfs.Tests
{
    [TestClass]
    public class TfsManagerTest : OctaneManagerBaseTest
    {    
        private readonly TfsManager _tfsManager = new TfsManager();
        [TestMethod]
        public void GetJobsList()
        {            
            _tfsManager.GetJobsList();
        }
    }
}
