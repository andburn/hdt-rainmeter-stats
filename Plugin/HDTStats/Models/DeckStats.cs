using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace me.andburn.rainmeter.HDTStats.Models
{
	public class DeckStats
	{
		public Guid DeckId;
		public string Name;

		[XmlArray(ElementName = "Games")]
		[XmlArrayItem(ElementName = "Game")]
		public List<GameStats> Games;		
	}
}
