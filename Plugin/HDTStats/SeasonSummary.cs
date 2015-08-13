using System;
using System.Collections.Generic;
using System.Text;

namespace me.andburn.rainmeter.HDTStats
{
	public class SeasonSummary
	{
		public int Rank { get; set; }
		public int Won { get; set; }
		public int Lost { get; set; }		
		public int WonToday { get; set; }
		public int LostToday { get; set; }

		public SeasonSummary()
		{
			Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "Summary()");
			Won = 0;
			Lost = 0;
			WonToday = 0;
			LostToday = 0;
			Rank = -1; // TODO: don't like this
		}

		public SeasonSummary(int won, int lost, int rank, int wonToday, int lostToday)
		{
			Rainmeter.API.Log(Rainmeter.API.LogType.Notice, "Summary(...)");
			Won = won;
			Lost = lost;
			Rank = rank;
			WonToday = wonToday;
			LostToday = lostToday;
		}
	}
}
