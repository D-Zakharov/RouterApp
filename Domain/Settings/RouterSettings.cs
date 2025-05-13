namespace Domain.Settings;

public class RouterSettings
{
    /// <summary>
    /// Макс. кол-во одновременно выполняемых методов. Значения меньше 1 трактуются как отсутствие ограничений.
    /// </summary>
    required public int MaxParallelExecutions { get; set; }
}
