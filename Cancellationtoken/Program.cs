using System.Threading;
using System.Threading.Tasks;

namespace ChatTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            if (args.Length == 0)
            {
                await Server.AcceptMsg(cts.Token);
            }
            else
            {
                await Client.SendMsg(args[0], cts.Token);
            }

            cts.Cancel();
        }
    }
}

