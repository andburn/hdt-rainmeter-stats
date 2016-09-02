using System;
using System.Collections.Generic;
using System.Text;
using me.andburn.rainmeter.HDTStats.Enums;

namespace me.andburn.rainmeter.HDTStats.Models
{
	public class GameStats
	{		
		public Guid GameId { get; set; }
		public Format? Format { get; set; }
		public string PlayerHero { get; set; }
		public GameMode GameMode { get; set; }
		public GameResult Result { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool IsClone { get; set; }
		public int Rank { get; set; }
		public Region Region { get; set; }
	}
}
