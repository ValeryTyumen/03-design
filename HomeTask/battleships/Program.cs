using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using NLog;

namespace battleships
{
	public class Program
	{
		private static readonly Logger resultsLog = LogManager.GetLogger("results");

	    private static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: {0} <ai.exe>", Process.GetCurrentProcess().ProcessName);
				return;
			}
			var aiPath = args[0];
			var settings = new Settings("settings.txt");
			var tester = new AiTester(settings);
		    tester.Info += resultsLog.Info;
			if (File.Exists(aiPath))
				tester.TestSingleFile(aiPath);
			else
				Console.WriteLine("No AI exe-file " + aiPath);
		}
	}
}