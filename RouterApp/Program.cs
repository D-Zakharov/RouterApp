using Application;
using Domain.Services;
using Domain.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RouterApp.Extensions;

namespace RouterApp;

internal class Program
{
    static void Main(string[] args)
    {
        var sc = new ServiceCollection();
        sc.AddApplicationServices();
        AddSettings(sc);

        using var serviceProvider = sc.BuildServiceProvider();

        AddRoutes(serviceProvider);
        RunRoutes(serviceProvider);

        Console.WriteLine("Done");
        Console.ReadKey();
    }

    private static void RunRoutes(ServiceProvider sp)
    {
        IRouter router = sp.GetRouter();
        List<Task> tasks = new();
        for (int i = 0; i < 20000; i++)
        {
            tasks.Add(Task.Run(() => RunRoute(router)));
        }

        Task.WaitAll(tasks.ToArray());
    }

    private static void RunRoute(IRouter router)
    {
        Random rand = new();
        router.RunRoute($"foo/bar/{rand.Next(1,100)}/{Guid.NewGuid()}");
    }

    private static void AddRoutes(ServiceProvider sp)
    {
        IRouter router = sp.GetRouter();
        router.RegisterRoute<Guid, int>("foo/bar/{b:int}/{a:guid}", (a, b) =>
        {
            Random rand = new();
            Console.WriteLine($"Thread id: {Environment.CurrentManagedThreadId}; {a} : {b + 1}");
            Thread.Sleep(rand.Next(300, 800));
            Console.WriteLine($"Thread id: {Environment.CurrentManagedThreadId}; done");
        });
    }

    private static void AddSettings(ServiceCollection sc)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false);

        IConfiguration config = builder.Build();

        var routerSettings = config.GetSection(nameof(RouterSettings)).Get<RouterSettings>() ??
            throw new Exception("Настройки маршрутизатора отсутствуют");

        sc.AddSingleton(routerSettings);
    }
}
