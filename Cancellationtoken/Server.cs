using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class Server
{
    private static CancellationTokenSource cts;

    public static async Task AcceptMsg(CancellationToken cancellationToken)
    {
        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient = new UdpClient(5050);
        Console.WriteLine("Сервер ожидает сообщения. Для завершения нажмите клавишу...");

        Task exitTask = Task.Run(() =>
        {
            Console.ReadKey();
            RequestExit();
        });

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                UdpReceiveResult receiveResult;

                try
                {
                    receiveResult = await ReceiveWithCancellationAsync(udpClient, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Ignore the exception when cancellation is requested.
                    break;
                }

                string data1 = Encoding.UTF8.GetString(receiveResult.Buffer);

                Message msg = Message.FromJson(data1);
                Console.WriteLine(msg.ToString());

                if (msg.Text.ToLower() == "exit")
                {
                    RequestExit();
                    break;
                }

                Message responseMsg = new Message("Server", "Message accept on serv!");
                string responseMsgJs = responseMsg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
                await udpClient.SendAsync(responseData, responseData.Length, receiveResult.RemoteEndPoint);
            }
        }
        finally
        {
            udpClient.Close();
            // Дожидаемся завершения задачи по нажатию клавиши
            await exitTask;
        }
    }

    private static async Task<UdpReceiveResult> ReceiveWithCancellationAsync(UdpClient udpClient, CancellationToken cancellationToken)
    {
        var receiveTask = udpClient.ReceiveAsync();
        var completedTask = await Task.WhenAny(receiveTask, Task.Delay(-1, cancellationToken));

        if (completedTask == receiveTask)
        {
            return await receiveTask;
        }
        else
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private static void RequestExit()
    {
        if (!cts.Token.IsCancellationRequested)
        {
            cts.Cancel();
        }
    }
}
