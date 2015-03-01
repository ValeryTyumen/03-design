using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battleships
{
	public class GameResult
	{
		public int TurnsCount { get; private set; }
		public int BadShots { get; private set; }
		public bool AiCrashed { get; private set; }
		public int GameIndex { get; private set; }

		public GameResult(int turnsCount, int badShots, bool aiCrashed, int gameIndex)
		{
			TurnsCount = turnsCount;
			BadShots = badShots;
			AiCrashed = aiCrashed;
			GameIndex = gameIndex;
		}
	}

	public class GameSequenceResult
	{
		public string AiName { get; private set; }
		public List<int> Shots { get; private set; }
		public int Crashes { get; private set; }
		public int BadShots { get; private set; }
		public int GamesPlayed { get; private set; }

		public GameSequenceResult(string aiName, List<int> shots, int crashes, int badShots, int gamesPlayed)
		{
			AiName = aiName;
			Shots = shots;
			Crashes = crashes;
			BadShots = badShots;
			GamesPlayed = gamesPlayed;
		}
	}

	public class GameSequence
	{
		public event Action<GameResult> ProvideGameResult;
		public event Action<GameSequenceResult> ProvideSequanceResults;

		private Settings settings;

		public GameSequence(Settings settings)
		{
			this.settings = settings;
		}

		public void Run(string aiPath)
		{
			var generator = new MapGenerator(settings, new Random(settings.RandomSeed));
			var visualizer = new GameVisualizer();
			var monitor = new ProcessMonitor(TimeSpan.FromSeconds(settings.TimeLimitSeconds * settings.GamesCount), settings.MemoryLimit);
			var badShots = 0;
			var crashes = 0;
			var gamesPlayed = 0;
			var shots = new List<int>();
			var ai = new Ai(aiPath);
			ai.ProcessCreated += monitor.Register;
			for (var gameIndex = 0; gameIndex < settings.GamesCount; gameIndex++)
			{
				var map = generator.GenerateMap();
				var game = new Game(map, ai);
				RunGameToEnd(game, visualizer);
				gamesPlayed++;
				badShots += game.BadShots;
				if (game.AiCrashed)
				{
					crashes++;
					if (crashes > settings.CrashLimit) break;
					game.RepairAi();
				}
				else
					shots.Add(game.TurnsCount);
				if (settings.Verbose)
					ProvideGameResult(new GameResult(game.TurnsCount, badShots, game.AiCrashed, gameIndex));
			}
			ai.Dispose();
			ProvideSequanceResults(new GameSequenceResult(ai.Name, shots, crashes, badShots, gamesPlayed));
		}

		private void RunGameToEnd(Game game, GameVisualizer visualizer)
		{
			while (!game.IsOver())
			{
				game.MakeStep();
				if (settings.Interactive)
				{
					visualizer.Visualize(game);
					if (game.AiCrashed)
						Console.WriteLine(game.LastError.Message);
					Console.ReadKey();
				}
			}
		}
	}
}
