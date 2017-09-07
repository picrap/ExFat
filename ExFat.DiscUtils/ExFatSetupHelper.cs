// This is ExFat, an exFAT accessor written in pure C#
// Released under MIT license
// https://github.com/picrap/ExFat

namespace ExFat.DiscUtils
{
    using System.Reflection;
    using global::DiscUtils.Setup;

    public static class ExFatSetupHelper
    {
        public static void SetupFileSystems()
        {
            SetupHelper.RegisterAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
