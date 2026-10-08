using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Bridge.Desktop.NativeMessaging;

public static class NativeMessagingHost
{
    public static async Task RunAsync(Func<string, Task<string>> messageHandler, CancellationToken ct = default)
    {
        using var stdin = Console.OpenStandardInput();
        using var stdout = Console.OpenStandardOutput();

        byte[] lengthBuffer = new byte[4];

        while (!ct.IsCancellationRequested)
        {
            int bytesRead = await stdin.ReadAsync(lengthBuffer, 0, 4, ct);
            if (bytesRead < 4) break; // Stream closed

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (messageLength <= 0 || messageLength > 10 * 1024 * 1024) break; // Safety ceiling 10MB

            byte[] messageBuffer = new byte[messageLength];
            int totalRead = 0;
            while (totalRead < messageLength)
            {
                int chunk = await stdin.ReadAsync(messageBuffer, totalRead, messageLength - totalRead, ct);
                if (chunk == 0) break;
                totalRead += chunk;
            }

            string requestJson = Encoding.UTF8.GetString(messageBuffer);

            string responseJson;
            try
            {
                responseJson = await messageHandler(requestJson);
            }
            catch (Exception ex)
            {
                responseJson = JsonSerializer.Serialize(new { error = ex.Message, success = false });
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            byte[] respLengthBytes = BitConverter.GetBytes(responseBytes.Length);

            await stdout.WriteAsync(respLengthBytes, 0, 4, ct);
            await stdout.WriteAsync(responseBytes, 0, responseBytes.Length, ct);
            await stdout.FlushAsync(ct);
        }
    }
}
