using System;
using NServiceBus.Raw;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.NServicebus
{
    public static class RawEndpointConfigurationExtension
    {
        public static void License(this RawEndpointConfiguration config, string licenseText)
        {
            if (string.IsNullOrEmpty(licenseText))
            {
                throw new ArgumentException("NServiceBus license text much not be empty", nameof(licenseText));
            }

            config.Settings.Set("LicenseText", (object)licenseText);
        }
    }
}
