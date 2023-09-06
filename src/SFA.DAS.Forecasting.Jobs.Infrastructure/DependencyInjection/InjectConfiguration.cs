using Microsoft.Azure.WebJobs.Host.Config;
using SFA.DAS.Forecasting.Jobs.Infrastructure.Attributes;

namespace SFA.DAS.Forecasting.Jobs.Infrastructure.DependencyInjection;

internal class InjectConfiguration : IExtensionConfigProvider
{
    private readonly InjectBindingProvider _injectBindingProvider;

    public InjectConfiguration(InjectBindingProvider injectBindingProvider) =>
        _injectBindingProvider = injectBindingProvider;

    public void Initialize(ExtensionConfigContext context) => context
        .AddBindingRule<InjectAttribute>()
        .Bind(_injectBindingProvider);
}