namespace Application.Models.Router.Segments;

internal abstract class AbstractSegment
{
    required public string RawSegmentDescription { get; init; }

    public static AbstractSegment Parse(string segment)
    {
        if (DynamicSegment.IsDynamic(segment))
        {
            return new DynamicSegment(segment) { RawSegmentDescription = segment };
        }
        else
        {
            return new StaticSegment { RawSegmentDescription = segment };
        }
    }

    public abstract bool IsMatches(string inputSegment, out object? value);
}
