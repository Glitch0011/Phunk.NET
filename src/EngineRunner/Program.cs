using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EngineRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var task = Task.Run(() =>
            {
                try
                {
                    string[] payload = new string[] { args[0] };

                    if (new FileInfo(args[0]).Exists)
                        payload = File.ReadAllLines(args[0]);

                    dynamic engine = new Phunk.Luan.Engine();

                    var output = engine(payload);

                    if (output == null)
                        Console.WriteLine("Output is null");
                    else
                        Console.WriteLine(output.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }, source.Token);

            try
            {
                task.Wait(source.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation timed out");
            }
        }
    }
}
