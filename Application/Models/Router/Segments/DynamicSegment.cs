namespace Application.Models.Router.Segments;

internal class DynamicSegment : AbstractSegment
{
    public string Name { get; init; }
    public Type Type { get; init; }

    public DynamicSegment(string segment)
    {
        Span<Range> splitRanges = stackalloc Range[2];

        var rawValue = segment
            .AsSpan()
            .Slice(1, segment.Length - 2);

        int rangesCount = rawValue.Split(splitRanges, ':');

        if (rangesCount < 2)
            throw new ArgumentException($"Некорректный формат динамического сегмента {segment}");

        Name = new string(rawValue[splitRanges[0]]);
        Type = GetTypeFromString(rawValue[splitRanges[1]]);
    }

    public static bool IsDynamic(string segment) => segment.StartsWith('{') && segment.EndsWith('}');

    public override bool IsMatches(string inputSegment, out object? value)
    {
        value = null;

        if (Type == typeof(int) && int.TryParse(inputSegment, out int intVal))
        {
            value = intVal;
        }
        else if (Type == typeof(float) && float.TryParse(inputSegment, out float floatVal))
        {
            value = floatVal;
        }
        else if (Type == typeof(Guid) && Guid.TryParse(inputSegment, out Guid guidVal))
        {
            value = guidVal;
        }
        else if (Type == typeof(DateTime) && DateTime.TryParse(inputSegment, out DateTime dateVal))
        {
            value = dateVal;
        }
        
        return value != null;
    }

    private static Type GetTypeFromString(ReadOnlySpan<char> typeName)
    {
        return typeName switch
        {
            "int" => typeof(int),
            "float" => typeof(float),
            "guid" => typeof(Guid),
            "datetime" => typeof(DateTime),
            _ => throw new NotImplementedException($"Тип переменной {typeName} не поддерживается")
        };
    }
}
