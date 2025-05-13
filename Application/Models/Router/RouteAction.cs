using System.Linq.Expressions;

namespace Application.Models.Router;

internal class RouteAction
{
    /// <summary>
    /// Индексы аргументов делегата в маршруте
    /// </summary>
    public IReadOnlyList<int> ParameterIndices { get; init; }

    public RouteAction(Delegate action, IReadOnlyList<int> parameterIndices)
    {
        ParameterIndices = parameterIndices;

        _invoker = CreateInvoker(action);
    }

    public void Invoke(params object[] parameters)
    {
        _invoker(parameters);
    }

    /// <summary>
    /// Относительно универсальный способ вызвать делегат с помощью скомпилированных деревьев выражений
    ///
    /// Бенчмарк:
    /// - ручной вызов 5,8 ns
    /// - вызов через DynamicInvoke 180,8 ns
    /// - вызов через эту скомпилированную лямбду 16,1 ns
    /// </summary>
    private static Func<object[], object> CreateInvoker(Delegate del)
    {
        var method = del.Method;
        var instance = del.Target != null ? Expression.Constant(del.Target) : null;
        var parameters = method.GetParameters();
        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var argExpressions = new Expression[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var index = Expression.Constant(i);
            var paramType = parameters[i].ParameterType;
            argExpressions[i] = Expression.Convert(Expression.ArrayIndex(argsParam, index), paramType);
        }

        var call = Expression.Call(instance, method, argExpressions);
        var body = method.ReturnType == typeof(void) ?
            (Expression)Expression.Block(call, Expression.Default(typeof(object))) :
            Expression.Convert(call, typeof(object));

        return Expression.Lambda<Func<object[], object>>(body, argsParam).Compile();
    }

    private readonly Func<object[], object> _invoker;
}
