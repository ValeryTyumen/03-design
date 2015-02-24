using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using NLog;
using Ninject;
namespace battleships
{
    public class Program
    {
        private static void Main(string[] args)
        {

            IKernel kernel = new StandardKernel();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: {0} <ai.exe>", Process.GetCurrentProcess().ProcessName);
                return;
            }
            var aiPath = args[0];
            kernel.Bind<Logger>().ToConstant(LogManager.GetLogger("results"));
            kernel.Bind<Settings>().ToConstant(new Settings("settings.txt"));
            kernel.Bind<Ai>().To<Ai>().WithConstructorArgument(aiPath);
            var tester = kernel.Get<AiTester>();
            var generator = kernel.Get<MapGenerator>();
            var visualizer = kernel.Get<GameVisualizer>();
            var monitor = kernel.Get<ProcessMonitor>();
            var ai = kernel.Get<Ai>();
            if (File.Exists(aiPath))
                tester.TestSingleFile(aiPath, generator, visualizer, monitor, ai);
            else
                Console.WriteLine("No AI exe-file " + aiPath);
            Console.ReadKey();
        }
    }
}