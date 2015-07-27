using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace me.andburn.rainmeter.HDTStats.Models
{
	public class DeckStatsList
	{
		[XmlArray(ElementName = "DeckStats")]
		[XmlArrayItem(ElementName = "Deck")]
		public List<DeckStats> DeckStats;
	}
}
