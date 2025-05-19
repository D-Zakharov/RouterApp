using System.Collections.Concurrent;
using Application.Models.Router.Segments;

namespace Application.Models.Router;

/// <summary>
/// Маршруты храним в виде древоподобной структуры
/// Каждый узел хранит информацию о сегменте, набор дочерних узлов и маршрут, который соответствует текущему узлу
/// Может быть маршрут /foo/bar/{p:int}, а может вместе с ним быть и /foo/bar/{p:int}/baz
/// </summary>
internal class RouteNode
{
    required public AbstractSegment Segment { get; set; }
    public List<RouteNode> ChildNodes { get; set; } = new();
    public RouteAction? RouteAction { get; set; }
}
