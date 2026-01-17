using System.Text;
using GhCredentialProvider.Plugin;
using Xunit;

namespace GhCredentialProvider.Tests.Plugin;

public class JsonRpcTests
{
    [Fact]
    public async Task ReadLineAsync_ReadsHandshakeRequest()
    {
        var requestJson =
            """{"RequestType":2,"RequestId":"test-123","ProtocolVersion":"2.0.0","MinimumProtocolVersion":"2.0.0"}"""
            + "\n";
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
        var outputStream = new MemoryStream();
        var rpc = new JsonRpc(inputStream, outputStream);

        var line = await rpc.ReadLineAsync();

        Assert.NotNull(line);
        Assert.Contains("test-123", line);
        Assert.Contains("ProtocolVersion", line);
    }

    [Fact]
    public async Task ReadLineAsync_ReadsGetAuthenticationCredentialsRequest()
    {
        var requestJson =
            """{"RequestType":4,"RequestId":"auth-101","Uri":"https://nuget.pkg.github.com/owner/index.json","IsRetry":false,"NonInteractive":true,"CanShowDialog":false}"""
            + "\n";
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));
        var outputStream = new MemoryStream();
        var rpc = new JsonRpc(inputStream, outputStream);

        var line = await rpc.ReadLineAsync();

        Assert.NotNull(line);
        Assert.Contains("auth-101", line);
        Assert.Contains("nuget.pkg.github.com", line);
    }

    [Fact]
    public async Task WriteLineAsync_WritesResponse()
    {
        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();
        var rpc = new JsonRpc(inputStream, outputStream);

        var responseJson =
            """{"ResponseCode":0,"RequestId":"test-123","ProtocolVersion":"2.0.0"}""";
        await rpc.WriteLineAsync(responseJson);

        outputStream.Position = 0;
        var reader = new StreamReader(outputStream, Encoding.UTF8);
        var output = await reader.ReadToEndAsync();

        Assert.Contains("\"ResponseCode\":0", output);
        Assert.Contains("\"RequestId\":\"test-123\"", output);
        Assert.EndsWith("\n", output);
    }

    [Fact]
    public async Task ReadLineAsync_HandlesEmptyStream()
    {
        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();
        var rpc = new JsonRpc(inputStream, outputStream);

        var line = await rpc.ReadLineAsync();

        Assert.Null(line);
    }

    [Fact]
    public async Task ReadLineAsync_HandlesMultipleRequests()
    {
        var request1Json =
            """{"RequestType":2,"RequestId":"req-1","ProtocolVersion":"2.0.0","MinimumProtocolVersion":"2.0.0"}"""
            + "\n";
        var request2Json =
            """{"RequestType":3,"RequestId":"req-2","ClientVersion":"6.0.0","Culture":"en-US","RequestTimeout":5000}"""
            + "\n";
        var combinedJson = request1Json + request2Json;
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(combinedJson));
        var outputStream = new MemoryStream();
        var rpc = new JsonRpc(inputStream, outputStream);

        var line1 = await rpc.ReadLineAsync();
        var line2 = await rpc.ReadLineAsync();

        Assert.NotNull(line1);
        Assert.Contains("req-1", line1);
        Assert.NotNull(line2);
        Assert.Contains("req-2", line2);
    }
}
