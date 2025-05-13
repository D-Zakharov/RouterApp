namespace Domain.Services;

/// <summary>
/// В данной реализации допустимы только делегаты с кол-вом аргументов от 0 до 2
/// </summary>
public interface IRouter
{
    void RegisterRoute(string template, Action action);
    void RegisterRoute<T>(string template, Action<T> action);
    void RegisterRoute<T1, T2>(string template, Action<T1, T2> action);

    void RunRoute(string route);

    // TOOD: рассмотреть необходимость добавления Task делегатов, возможность передачи callback метода
}
