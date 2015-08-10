using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using me.andburn.rainmeter.HDTStats.Enums;
using me.andburn.rainmeter.HDTStats.Models;

namespace me.andburn.rainmeter.HDTStats
{
	public class StatsDB
	{
		private const string DB_NAME = "DeckStats.xml";
		private const int MIN_INTERVAL = 30; // seconds

		private static string DBPath;
		private static Region Server;
		private static int TotalWon;
		private static int TotalLost;
		private static DateTime FirstOfMonth;
		private static DateTime LastOfMonth;
		private static DateTime LastRun;

		public static SeasonSummary RankedSummary(string path, string server) {
			Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "RankedSummary()");
			try
			{
				// check if path and server are valid
				Validate(path, server);
				// setup up initial state
				Initialize();
				// load stats from file
				List<DeckStats> data = Load();
				// return the summary from loaded data
				return Summarize(data);
			} 
			catch(Exception e)
			{
				Rainmeter.API.Log(Rainmeter.API.LogType.Error, "Error: " + e.Message);
			}
			return new SeasonSummary();
		}

		// Set DBPath and Server to params or default values
		private static void Validate(string path, string server)
		{
			var region = String.IsNullOrEmpty(server) ? "" : server.ToLowerInvariant();
			switch(region)
			{
				case "eu": 
					Server = Region.EU; break;				
				case "asia": 
					Server = Region.ASIA; break;
				case "us":
				default:
					Server = Region.US; break;
			}
			if(!String.IsNullOrEmpty(path) && Directory.Exists(path))
			{
				DBPath = path;
			}
			else
			{
				Rainmeter.API.Log(Rainmeter.API.LogType.Debug, path + " does not exist, using AppData");
				DBPath = Path.Combine(Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData), "HearthstoneDeckTracker");
			}
			
		}

		private static void Initialize() {
			var now = DateTime.Now;
			var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
			FirstOfMonth = new DateTime(now.Year, now.Month, 1);
			LastOfMonth = new DateTime(now.Year, now.Month, daysInMonth);
			TotalWon = 0;
			TotalLost = 0;
		}

		public static bool IntervalHasLapsed()
		{
			if(LastRun == null)
			{
				LastRun = DateTime.Now;
				return true;
			}
			return (DateTime.Now - LastRun).TotalSeconds >= MIN_INTERVAL;
		}

		// load the xml data and deserialize
		private static List<DeckStats> Load()
		{
			try
			{
				using(TextReader reader = new StreamReader(Path.Combine(DBPath, DB_NAME)))
				{
					var xml = new XmlSerializer(typeof(DeckStatsList));
					var list = (DeckStatsList)xml.Deserialize(reader);
					return list.DeckStats;
				}
			}
			catch(Exception e)
			{
				Rainmeter.API.Log(Rainmeter.API.LogType.Error, "Error reading DB (" + e.Message + ")");
			}
			return new List<DeckStats>();
		}

		private static SeasonSummary Summarize(List<DeckStats> stats)
		{
			Dictionary<Guid, DeckRecord> records = new Dictionary<Guid, DeckRecord>();
			GameStats latest = new GameStats();
			latest.StartTime = DateTime.Now.AddMonths(-1);
			Guid lastPlayed = Guid.Empty;

			foreach(var deck in stats)
			{
				foreach(var game in deck.Games)
				{
					if(IsRanked(game) && IsThisSeason(game) && IsOnThisServer(game))
					{
						AddGame(records, deck, game);
						if(IsLatest(latest, game))
						{
							latest = game;
							lastPlayed = deck.DeckId;
						}
					}
				}
			}

			var lastPlayedDeck = new DeckRecord();
			if(records.ContainsKey(lastPlayed))
				lastPlayedDeck = records[lastPlayed];
			return new SeasonSummary(TotalWon, TotalLost, latest.Rank, lastPlayedDeck);
		}

		private static bool IsOnThisServer(GameStats g)
		{
			return g.Region == Server;
		}

		private static bool IsRanked(GameStats g)
		{
			return g.GameMode == GameMode.Ranked;
		}

		private static bool IsThisSeason(GameStats g)
		{
			return g.StartTime >= FirstOfMonth && g.StartTime <= LastOfMonth;
		}

		private static bool IsLatest(GameStats latest, GameStats game)
		{
			return game.StartTime >= latest.StartTime;
		}

		private static void AddGame(Dictionary<Guid, DeckRecord> records, DeckStats deck, GameStats game)
		{
			var deckId = deck.DeckId;
			if(!records.ContainsKey(deckId))
			{
				records[deckId] = new DeckRecord(deckId);
				// TODO: does deck name change or in games?
				records[deckId].Name = deck.Name;
				records[deckId].HeroClass = HeroNum(game.PlayerHero);
			}

			if(game.Result == GameResult.Win)
			{
				records[deckId].Won++;
				TotalWon++;
			}
			else if(game.Result == GameResult.Loss)
			{
				records[deckId].Lost++;
				TotalLost++;
			}
			// ignore Draw and "other" results
		}

		// convert hero names into numbers, easier for measures
		private static int HeroNum(string hero)
		{
			int num = 0;
			switch(hero.ToLowerInvariant())
			{
				case "warrior": num = 1; break;
				case "shaman": num = 2; break;
				case "rogue": num = 3; break;
				case "paladin": num = 4; break;
				case "hunter": num = 5; break;
				case "druid": num = 6; break;
				case "warlock": num = 7; break;
				case "mage": num = 8; break;
				case "priest": num = 9; break;
			}
			return num;
		}

		// Replaced this with Last Played instead.
		// Compares decks to see which has greatest total so far
		private static Guid MostPlayed(Dictionary<Guid, DeckRecord> records, Guid most, Guid deck)
		{
			if(most == Guid.Empty || !records.ContainsKey(most))
				return deck;

			var mostTotal = records[most].Won + records[most].Lost;
			var deckTotal = 0;
			if (records.ContainsKey(deck))
			{
				deckTotal = records[deck].Won + records[deck].Lost;
			}
			return deckTotal >= mostTotal ? deck : most;
		}
	}		
}
