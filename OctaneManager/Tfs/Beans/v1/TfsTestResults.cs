using MicroFocus.Ci.Tfs.Octane.Tfs.ApiItems;
using MicroFocus.Ci.Tfs.Octane.Tfs.Beans.v1;

namespace MicroFocus.Ci.Tfs.Octane.Tfs.Beans
{
    public class TfsTestResults : TfsCollection<TfsTestResult>
    {
        public TfsRun Run { get; set; }
    }
}
