﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class Client
{
    public static async Task SendMsg(string name, CancellationToken cancellationToken)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050);
        UdpClient udpClient = new UdpClient();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Введите сообщение (или 'Exit' для завершения): ");
                string text = Console.ReadLine();

                if (text.ToLower() == "exit")
                    break;

                Message msg = new Message(name, text);
                string responseMsgJs = msg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);

                await udpClient.SendAsync(responseData, responseData.Length, ep);

                var receiveTask = udpClient.ReceiveAsync();

                if (await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, cancellationToken)) == receiveTask)
                {
                    byte[] answerData = receiveTask.Result.Buffer;
                    string answerMsgJs = Encoding.UTF8.GetString(answerData);
                    Message answerMsg = Message.FromJson(answerMsgJs);
                    Console.WriteLine(answerMsg.ToString());
                }
                else
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
        }
        finally
        {
            udpClient.Close();
        }
    }
}
