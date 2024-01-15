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
                    receiveResult = await udpClient.ReceiveAsync();
                }
                catch (OperationCanceledException)
                {
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
            
            await exitTask;
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
