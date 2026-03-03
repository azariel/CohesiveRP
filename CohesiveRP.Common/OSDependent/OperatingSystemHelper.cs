using System.Runtime.InteropServices;

namespace CohesiveRP.Common.OSDependent
{
    public static class OperatingSystemHelper
    {
        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static SupportedOperatingSystem GetCurrentOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return SupportedOperatingSystem.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return SupportedOperatingSystem.Windows;

            throw new ArgumentOutOfRangeException("Current operating system isn't handled.");
        }
    }
}
