using Application.Services;
using Domain.Services;
using Domain.Settings;

namespace UnitTests;

/// <summary>
/// Пара базовых тестов
/// </summary>
public class Tests
{
    [Fact]
    public void Router_ParameterlessAction_Ok()
    {
        IRouter router = CreateRouter();

        List<int> list = new();
        router.RegisterRoute("/test1/test2/", () => { list.Add(1); });

        router.RunRoute("test1/test2");
        router.RunRoute("test1/test2");

        Assert.Equal(1, list[1]);
    }

    [Fact]
    public void Router_TwoIntParametersReverse_Ok()
    {
        IRouter router = CreateRouter();

        List<int> list = new();
        router.RegisterRoute<int, int>("/test1/test2/{b:int}/{a:int}", (a, b) => { list.Add(a); list.Add(b); });

        router.RunRoute("test1/test2/1/2");

        Assert.Equal(1, list[1]);
        Assert.Equal(2, list[0]);
    }

    [Fact]
    public void Router_IntAndGuidParameters_Ok()
    {
        IRouter router = CreateRouter();

        List<string> list = new();
        router.RegisterRoute<int, Guid>("/test1/test2/{a:int}/{b:guid}", (a, b) => { list.Add($"{a + 1} : {b}"); });

        Guid guid = Guid.NewGuid();
        router.RunRoute($"test1/test2/1/{guid}");

        Assert.Equal($"2 : {guid}", list[0]);
    }

    [Fact]
    public void Router_SimilarRoutes_Ok()
    {
        IRouter router = CreateRouter();

        List<string> list = new();
        router.RegisterRoute<int>("/test1/test2/{a:int}/", (a) => { list.Add($"{a + 1}"); });
        router.RegisterRoute<int, Guid>("/test1/test2/{a:int}/test3/{b:guid}/", (a, b) => { list.Add($"{a + 1} : {b}"); });

        Guid guid = Guid.NewGuid();
        router.RunRoute($"test1/test2/1");
        router.RunRoute($"test1/test2/1/test3/{guid}");

        Assert.Equal("2", list[0]);
        Assert.Equal($"2 : {guid}", list[1]);
    }

    [Fact]
    public void Router_UnknownRoute_ThrowsException()
    {
        IRouter router = CreateRouter();

        router.RegisterRoute("/test1/test2/", Console.WriteLine);

        Exception exception = Assert.Throws<ArgumentException>(() => router.RunRoute("/test1/test2/test3"));
        Assert.Equal("Не найден подходящий маршрут", exception.Message);
    }

    [Fact]
    public void Router_WrongArguments_ThrowsException()
    {
        IRouter router = CreateRouter();

        List<string> list = new();
        router.RegisterRoute<int, Guid>("/test1/test2/{a:int}/{b:guid}", (a, b) => { list.Add($"{a + 1} : {b}"); });

        Exception exception = Assert.Throws<ArgumentException>(() => router.RunRoute("/test1/test2/1/2"));
        Assert.Equal("Не найден подходящий маршрут", exception.Message);
    }

    private static IRouter CreateRouter()
    {
        return new Router(new RouterSettings() { MaxParallelExecutions = 100 });
    }
}