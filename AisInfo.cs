namespace Ais;

public class AisInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public GeoJSON.Text.Geometry.Point Location { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}