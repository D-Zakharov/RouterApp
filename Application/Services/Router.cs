using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Application.Models.Router;
using Application.Models.Router.Segments;
using Domain.Services;
using Domain.Settings;

namespace Application.Services;

/// <summary>
/// Непосредственно маршрутизатор
/// Маршруты хранятся в виде древоподобной структуры для упрощения поиска
/// </summary>
internal class Router : IRouter
{
    /// <summary>
    /// Настройки маршрутизаора
    /// </summary>
    private readonly RouterSettings _settings;

    /// <summary>
    /// "Головы" всех сохраненных маршрутов
    /// </summary>
    private readonly List<RouteNode> _routeHeads = new();

    /// <summary>
    /// Семафор, ограничивающий одновременное выполнение делегатов
    /// </summary>
    private readonly SemaphoreSlim _runRouteSemaphore;

    private const char SplitSymbol = '/';

    private bool AreParallelRunsLimited => _settings.MaxParallelExecutions > 0;

    public Router(RouterSettings settings)
    {
        _settings = settings;

        int parallelCount = _settings.MaxParallelExecutions > 0 ? _settings.MaxParallelExecutions : 1;
        _runRouteSemaphore = new(parallelCount, parallelCount);
    }

    public void RegisterRoute(string template, Action action)
    {
        Register(template, action, Array.Empty<Type>());
    }

    public void RegisterRoute<T>(string template, Action<T> action)
    {
        Register(template, action, [typeof(T)]);
    }

    public void RegisterRoute<T1, T2>(string template, Action<T1, T2> action)
    {
        Register(template, action, [typeof(T1), typeof(T2)]);
    }

    public void RunRoute(string route)
    {
        if (AreParallelRunsLimited)
        {
            _runRouteSemaphore.Wait();

            try
            {
                GetAndRunRoute(route);
            }
            finally
            {
                _runRouteSemaphore.Release();
            }
        }
        else
        {
            GetAndRunRoute(route);
        }
    }

    private void GetAndRunRoute(string route)
    {
        var action = FindAppropriateRouteAction(route, out var parameters);
        action.Invoke(parameters);
    }

    private RouteAction FindAppropriateRouteAction(string route, out object[] parameters)
    {
        // найденные аргументы делегата
        List<object> extractedValues = new();

        RouteAction? result = null;

        var inputSegments = SplitRoute(route);
        var currentHeads = _routeHeads;
        foreach (string inputSegment in inputSegments)
        {
            bool wasFound = false;
            foreach (var headNode in currentHeads)
            {
                if (headNode.Segment.IsMatches(inputSegment, out object? value))
                {
                    if (value is not null)
                    {
                        extractedValues.Add(value);
                    }

                    wasFound = true;
                    result = headNode.RouteAction;
                    currentHeads = headNode.ChildNodes;

                    break;
                }
            }

            if (!wasFound)
                throw new ArgumentException("Не найден подходящий маршрут");
        }

        if (result is null)
            throw new ArgumentException("Не найден подходящий маршрут");


        // расставляем аргументы в нужном порядке
        parameters = new object[result.ParameterIndices.Count];
        for (int i = 0; i < extractedValues.Count; i++)
        {
            parameters[result.ParameterIndices[i]] = extractedValues[i];
        }

        return result;
    }

    private void Register(string template, Delegate action, Type[] paramTypes)
    {
        lock (_routeHeads)
        {
            ParameterInfo[] methodParams = action.Method.GetParameters();
            if (methodParams.Length != paramTypes.Length)
                throw new ArgumentException("Количество параметров в шаблоне не совпадает с параметрами делегата.");

            var inputSegments = SplitRoute(template);
            var currentHeads = _routeHeads;

            RouteNode? lastNode = null;
            List<DynamicSegment> dynamicSegments = new();
            foreach (string rawSegment in SplitRoute(template))
            {
                bool wasFound = false;
                foreach (var headNode in currentHeads)
                {
                    if (headNode.Segment.RawSegmentDescription == rawSegment)
                    {
                        wasFound = true;
                        currentHeads = headNode.ChildNodes;
                        lastNode = headNode;

                        break;
                    }
                }

                if (!wasFound)
                {
                    lastNode = new RouteNode() { Segment = AbstractSegment.Parse(rawSegment) };
                    currentHeads.Add(lastNode);
                    currentHeads = lastNode.ChildNodes;
                }

                if (lastNode!.Segment is DynamicSegment dynamicSegment)
                    dynamicSegments.Add(dynamicSegment);
            }

            if (lastNode is null)
            {
                throw new UnreachableException($"Ошибка в методе {nameof(Register)}");
            }

            if (dynamicSegments.Count != paramTypes.Length)
            {
                throw new ArgumentException("Количество параметров в шаблоне не совпадает с объявленными параметрами.");
            }

            // теперь требуется определить индексы параметров, т.к. их порядок в шаблоне может не совпадать с порядком передачи в делегат
            var parameterIndices = new int[dynamicSegments.Count];
            for (int i = 0; i < dynamicSegments.Count; i++)
            {
                var segment = dynamicSegments[i];
                var delegateParam = methodParams.Where(x => segment.Name == x.Name).FirstOrDefault();

                if (delegateParam is null)
                {
                    throw new ArgumentException($"У делегата нет параметра с названием '{segment.Name}'.");
                }
                if (delegateParam.ParameterType != segment.Type)
                {
                    throw new ArgumentException($"Не совпадает тип для параметра '{segment.Name}'.");
                }

                parameterIndices[i] = delegateParam.Position;
            }

            lastNode.RouteAction = new RouteAction(action, parameterIndices);
        }
    }

    private static string[] SplitRoute(string route)
    {
        return route.Split(SplitSymbol, StringSplitOptions.RemoveEmptyEntries);
    }
}


