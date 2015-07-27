using System;
using System.Collections.Generic;
using System.Text;

namespace me.andburn.rainmeter.HDTStats
{
	public class DeckRecord
	{
		public Guid Id { get; set; }	
		public string Name { get; set; }
		public int HeroClass { get; set; }
		public int Won { get; set; }
		public int Lost { get; set; }		

		public DeckRecord()
		{
			Id = Guid.Empty;
			HeroClass = 1;
			Name = "None";
			Won = 0;
			Lost = 0;			
		}

		public DeckRecord(Guid id) : this()
		{
			Id = id;
		}

		public DeckRecord(Guid id, int hero, string name, int won, int lost)
		{
			Id = id;
			HeroClass = hero;
			Name = name;
			Won = won;
			Lost = lost;
		}
	}
}
