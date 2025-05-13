namespace Application.Models.Router.Segments;

internal class StaticSegment : AbstractSegment
{
    public override bool IsMatches(string inputSegment, out object? value)
    {
        value = null;

        return RawSegmentDescription == inputSegment;
    }
}
