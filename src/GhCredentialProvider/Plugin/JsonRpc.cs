using System.Text;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Plugin;

public class JsonRpc
{
    private readonly Stream _inputStream;
    private readonly Stream _outputStream;

    public JsonRpc(Stream inputStream, Stream outputStream)
    {
        _inputStream = inputStream;
        _outputStream = outputStream;
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new List<byte>();
        var byteBuffer = new byte[1];

        while (true)
        {
            var bytesRead = await _inputStream.ReadAsync(byteBuffer, 0, 1, cancellationToken);
            if (bytesRead == 0)
            {
                break; // End of stream
            }

            if (byteBuffer[0] == '\n')
            {
                break; // End of line
            }

            if (byteBuffer[0] != '\r')
            {
                buffer.Add(byteBuffer[0]);
            }
        }

        if (buffer.Count == 0)
        {
            return null;
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    public async Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes(line + "\n");
        await _outputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        await _outputStream.FlushAsync(cancellationToken);
    }
}
