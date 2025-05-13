using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RouterApp.Extensions;

internal static class ServiceProviderExtensions
{
    internal static IRouter GetRouter(this ServiceProvider sp)
    {
        return sp.GetService<IRouter>() ?? throw new Exception($"{nameof(IRouter)} не зарегистрирован");
    }
}
