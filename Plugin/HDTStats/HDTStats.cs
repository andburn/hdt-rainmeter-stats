using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using me.andburn.rainmeter.HDTStats.Enums;
using Rainmeter;

namespace me.andburn.rainmeter.HDTStats
{
	internal class Measure
	{
		internal enum MeasureType
		{
			Rank,
			Won,
			Lost,
			WonToday,
			LostToday,
			HighestRank
		}

		internal MeasureType Type = MeasureType.Rank;

		internal virtual void Dispose()
		{
		}

		// this is for initial "parent" type,
		// sent down inheritenace chain, overidden though? 
		internal virtual void Reload(Rainmeter.API api, ref double maxValue)
		{
			string type = api.ReadString("Type", "");
			switch(type.ToLowerInvariant())
			{
			case "rank":
				Type = MeasureType.Rank;
				break;
			case "won":
				Type = MeasureType.Won;
				break;
			case "lost":
				Type = MeasureType.Lost;
				break;
			case "wontoday":
				Type = MeasureType.WonToday;
				break;
			case "losttoday":
				Type = MeasureType.LostToday;
				break;
			case "highestrank":
				Type = MeasureType.HighestRank;
				break;	
			default:
				API.Log(API.LogType.Error, "Type=" + type + " is not valid");
				break;
			}
		}

		internal virtual double Update()
		{
			return 0.0;
		}
	}

	internal class ParentMeasure : Measure
	{
		// This list of all parent measures is used by the child measures to find their parent.
		internal static List<ParentMeasure> ParentMeasures = new List<ParentMeasure>();

		internal string Name;
		internal IntPtr Skin;

		internal string Path;
		internal string Server;
		internal string Format;

		private SeasonSummary summary;

		internal ParentMeasure()
		{
			ParentMeasures.Add(this);
		}

		internal override void Dispose()
		{
			ParentMeasures.Remove(this);
		}

		internal override void Reload(Rainmeter.API api, ref double maxValue)
		{
			base.Reload(api, ref maxValue);

			Name = api.GetMeasureName();
			Skin = api.GetSkin();

			Path = api.ReadString("Path", "");
			Server = api.ReadString("Server", "");
			Format = api.ReadString("Format", "");
		}

		internal override double Update()
		{
			//Rainmeter.API.Log(API.LogType.Debug, "Update()");
			summary = StatsDB.RankedSummary(Path, Server, Format);
			return GetValue(Type);
		}

		internal double GetValue(MeasureType type)
		{
			//Rainmeter.API.Log(API.LogType.Debug, "GetValue(): type=" + type);
			switch(type)
			{
				case MeasureType.Rank:
					return summary.Rank;
				case MeasureType.Won:
					return summary.Won;
				case MeasureType.Lost:
					return summary.Lost;
				case MeasureType.WonToday:
					return summary.WonToday;
				case MeasureType.LostToday:
					return summary.LostToday;
				case MeasureType.HighestRank:
					return summary.HighestRank;
				default:
					return 0.0;
			}
		}
	}

	internal class ChildMeasure : Measure
	{
		private ParentMeasure ParentMeasure = null;

		internal override void Reload(Rainmeter.API api, ref double maxValue)
		{
			base.Reload(api, ref maxValue);

			string parentName = api.ReadString("ParentName", "");
			IntPtr skin = api.GetSkin();

			// Find parent using name AND the skin handle to be sure that it's the right one.
			ParentMeasure = null;
			foreach(ParentMeasure parentMeasure in ParentMeasure.ParentMeasures)
			{
				if(parentMeasure.Skin.Equals(skin) && parentMeasure.Name.Equals(parentName))
				{
					ParentMeasure = parentMeasure;
				}
			}

			if(ParentMeasure == null)
			{
				API.Log(API.LogType.Error, "ParentChild.dll: ParentName=" + parentName + " is not valid");
			}
		}

		internal override double Update()
		{
			if(ParentMeasure != null)
			{
				return ParentMeasure.GetValue(Type);
			}

			return 0.0;
		}
	}

	public static class Plugin
	{
		[DllExport]
		public static void Initialize(ref IntPtr data, IntPtr rm)
		{
			Rainmeter.API api = new Rainmeter.API(rm);

			string parent = api.ReadString("ParentName", "");
			Measure measure;
			if(String.IsNullOrEmpty(parent))
			{
				measure = new ParentMeasure();
			}
			else
			{
				measure = new ChildMeasure();
			}

			data = GCHandle.ToIntPtr(GCHandle.Alloc(measure));
		}

		[DllExport]
		public static void Finalize(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			measure.Dispose();
			GCHandle.FromIntPtr(data).Free();
		}

		[DllExport]
		public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			measure.Reload(new Rainmeter.API(rm), ref maxValue);
		}

		[DllExport]
		public static double Update(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			return measure.Update();
		}
	}
}
