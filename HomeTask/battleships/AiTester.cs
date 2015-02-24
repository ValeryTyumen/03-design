using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace battleships
{
    public class AiTester
    {
        private readonly Logger _resultsLog;
        private readonly Settings _settings;

        public AiTester(Settings settings, Logger resultsLog)
        {
            _settings = settings;
            _resultsLog = resultsLog;
        }

        public void TestSingleFile(string aiSource, MapGenerator generator,
            GameVisualizer visualizer, ProcessMonitor monitor, Ai ai)
        {
            var badShots = 0;
            var crashes = 0;
            var gamesPlayed = 0;
            var shots = new List<int>();
            for (var gameIndex = 0; gameIndex < _settings.GamesCount; gameIndex++)
            {
                var map = generator.GenerateMap();
                var game = new Game(map, ai);
                RunGameToEnd(game, visualizer);
                gamesPlayed++;
                badShots += game.BadShots;
                if (game.AiCrashed)
                {
                    crashes++;
                    if (crashes > _settings.CrashLimit) break;
                    ai = new Ai(aiSource, monitor);
                }
                else
                    shots.Add(game.TurnsCount);
                if (_settings.Verbose)
                {
                    Console.WriteLine(
                        "Game #{3,4}: Turns {0,4}, BadShots {1}{2}",
                        game.TurnsCount, game.BadShots, game.AiCrashed ? ", Crashed" : "", gameIndex);
                }
            }
            ai.Dispose();
            WriteTotal(ai, shots, crashes, badShots, gamesPlayed);
        }

        private void RunGameToEnd(Game game, GameVisualizer visualizer)
        {
            while (!game.IsOver())
            {
                game.MakeStep();
                if (_settings.Interactive)
                {
                    visualizer.Visualize(game);
                    if (game.AiCrashed)
                        Console.WriteLine(game.LastError.Message);
                    Console.ReadKey();
                }
            }
        }

        private void WriteTotal(Ai ai, List<int> shots, int crashes, int badShots, int gamesPlayed)
        {
            if (shots.Count == 0) shots.Add(1000 * 1000);
            shots.Sort();
            var median = shots.Count % 2 == 1 ? shots[shots.Count / 2] : (shots[shots.Count / 2] + shots[(shots.Count + 1) / 2]) / 2;
            var mean = shots.Average();
            var sigma = Math.Sqrt(shots.Average(s => (s - mean) * (s - mean)));
            var badFraction = (100.0 * badShots) / shots.Sum();
            var crashPenalty = 100.0 * crashes / _settings.CrashLimit;
            var efficiencyScore = 100.0 * (_settings.Width * _settings.Height - mean) / (_settings.Width * _settings.Height);
            var score = efficiencyScore - crashPenalty - badFraction;
            var headers = FormatTableRow(new object[] { "AiName", "Mean", "Sigma", "Median", "Crashes", "Bad%", "Games", "Score" });
            var message = FormatTableRow(new object[] { ai.Name, mean, sigma, median, crashes, badFraction, gamesPlayed, score });
            _resultsLog.Info(message);
            Console.WriteLine();
            Console.WriteLine("Score statistics");
            Console.WriteLine("================");
            Console.WriteLine(headers);
            Console.WriteLine(message);
        }

        private string FormatTableRow(object[] values)
        {
            return FormatValue(values[0], 15)
                + string.Join(" ", values.Skip(1).Select(v => FormatValue(v, 7)));
        }

        private static string FormatValue(object v, int width)
        {
            return v.ToString().Replace("\t", " ").PadRight(width).Substring(0, width);
        }
    }
}