using EvokeDish.Abstractions;
using Android.OS;

namespace EvokeDish.Common.Droid
{
    public class EnvironmentService : IEnvironmentService
    {
        #region IEnvironmentService implementation
        public bool IsRealDevice
        {
            get
            {
                string f = Build.Fingerprint;
                return !(f.Contains("vbox") || f.Contains("generic") || f.Contains("vsemu"));
            }
        }
        #endregion
    }
}

