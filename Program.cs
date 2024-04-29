using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ais;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using GeoJSON.Text.Geometry;

var source = new CancellationTokenSource();
var token = source.Token;

using var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("wss://stream.aisstream.io/v0/stream"), token);

var message = "{\"APIKey\":\"" + Environment.GetEnvironmentVariable("AISSTREAM_APIKEY") + "\",\"BoundingBoxes\":[[[-180,-90],[180, 90]]]}";
Console.WriteLine($"WS Send: {message}");
await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, token);

var client = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("https://es01:9200"))
    .ServerCertificateValidationCallback((sender, cert, chain, errors) => true)
    .Authentication(new BasicAuthentication("elastic", Environment.GetEnvironmentVariable("ELASTIC_PASSWORD"))));

var buffer = new byte[4096];
while (ws.State == WebSocketState.Open)
{
    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
    if (result.MessageType == WebSocketMessageType.Close)
    {
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
    }
    else
    {
        var received = Encoding.Default.GetString(buffer, 0, result.Count);
        Console.WriteLine($"WS Received {Encoding.Default.GetString(buffer, 0, result.Count)}");
        
        var json = JsonSerializer.Deserialize<JsonNode>(received);
        var id = json["MetaData"]["MMSI"].GetValue<long>().ToString();
        var name = json["MetaData"]["ShipName"].GetValue<string>();
        var lat = json["MetaData"]["latitude"].GetValue<double>();
        var lng = json["MetaData"]["longitude"].GetValue<double>();
        var time = json["MetaData"]["time_utc"].GetValue<string>()[..23] + "Z";
        var timestamp = DateTimeOffset.ParseExact(time, "yyyy-MM-dd HH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        var indexResponse = await client.IndexAsync(new AisInfo
        {
            Id = id,
            Name = name,
            Location =  new Point(new Position(lat, lng)),
            Timestamp = timestamp
        }, index: $"ais-{timestamp:yyyy-MM-dd}", token);
        Console.WriteLine($"ES response: {indexResponse.IsValidResponse}");
    }
}