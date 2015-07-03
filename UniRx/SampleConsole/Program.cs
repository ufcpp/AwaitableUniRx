using SampleModels;
using System;
using System.Threading.Tasks;
using UniRx;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var c = new Class1();

            var s = new Subject<string>();

            s.Subscribe(
                x => Console.WriteLine(x.Substring(0, 100)),
                ex => Console.WriteLine(ex.Message)
                );

            await c.RunAsync(s);
        }
    }
}
