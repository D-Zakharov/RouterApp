using Application.Services;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Registration
{
    public static void AddApplicationServices(this IServiceCollection sc)
    {
        sc.AddSingleton<IRouter, Router>();
    }
}
