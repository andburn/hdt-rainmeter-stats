using System;
using System.Collections.Generic;
using System.Text;

namespace me.andburn.rainmeter.HDTStats
{
	public class SeasonSummary
	{
		public int Won { get; set; }
		public int Lost { get; set; }
		public int Rank { get; set; }
		public DeckRecord LastPlayedDeck { get; set; }

		public SeasonSummary()
		{
			Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "Summary()");
			Won = 0;
			Lost = 0;
			Rank = -1;
			LastPlayedDeck = new DeckRecord();
		}

		public SeasonSummary(int won, int lost, int rank, DeckRecord deck)
		{
			Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "Summary(...)");
			Won = won;
			Lost = lost;
			Rank = rank;
			LastPlayedDeck = deck;
		}
	}
}
