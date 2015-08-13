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
		private static int WonToday;
		private static int LostToday;
		private static DateTime FirstOfMonth;
		private static DateTime LastOfMonth;
		private static DateTime StartOfToday;
		private static DateTime EndOfToday;
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
				case "china":
					Server = Region.CHINA; break;
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
			StartOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0); // TODO: check this is today!
			EndOfToday = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
			TotalWon = 0;
			TotalLost = 0;
			WonToday = 0;
			LostToday = 0;
		}

		// TODO: is this used, enforces delay so db file isn't continally in use
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
			GameStats latest = new GameStats();
			latest.StartTime = DateTime.Now.AddMonths(-1);

			foreach(var deck in stats)
			{
				foreach(var game in deck.Games)
				{
					if(IsRanked(game) && IsThisSeason(game) && IsOnThisServer(game))
					{
						AddGame(game, IsToday(game));
						if(IsLatest(latest, game))
						{
							latest = game;
						}
					}
				}
			}

			return new SeasonSummary(TotalWon, TotalLost, latest.Rank, WonToday, LostToday);
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

		private static bool IsToday(GameStats g)
		{
			return g.StartTime >= StartOfToday && g.StartTime <= EndOfToday;
		}

		private static bool IsLatest(GameStats latest, GameStats game)
		{
			return game.StartTime >= latest.StartTime;
		}

		private static void AddGame(GameStats game, bool today = false)
		{
			if(game.Result == GameResult.Win)
			{
				TotalWon++;
				if(today)
					WonToday++;
			}
			else if(game.Result == GameResult.Loss)
			{
				TotalLost++;
				if(today)
					LostToday++;
			}
			// ignore Draw and "other" results
		}
	}		
}
