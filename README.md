# HDT Rainmeter Plugin

A plugin for rainmeter to display Hearthstone ranked play statistics taken from [Heathstone Deck Tracker](https://github.com/Epix37/Hearthstone-Deck-Tracker). The plugin is intended to provide a season summary and your current rank. There is also a sample skin to illustrate using the plugin's measures. Download the [latest release](https://github.com/andburn/hdt-rainmeter-stats/releases/latest) which includes the skin and plugin as a rainmeter installer package.

![Sample Skin](http://i.imgur.com/qrcBknh.png)

For efficiency the plugin uses the parent/child model of rainmeter plugin development. There are five measure types that can be used:

1. **Rank**: your current rank
- **Won**: overall ranked wins this season/month
- **Lost**: overall ranked losses this season/month
- **WonToday**: number of ranked wins today
- **LostToday**: number of ranked losses today

The plugin also uses two optional parameters:

- **Path**: this is the path to you Hearthstone Deck Tracker data files. This defaults to the standard *AppData* location, if the parameter is omitted.
- **Server**: this is the Hearthstone server you want to display statistics for. The default is *US*, other values are *EU*, *ASIA* and *CHINA*.

See the [sample skin](https://github.com/andburn/hdt-rainmeter-stats/blob/master/Skin/Season%20Stats/stats.ini) for further details on using the plugin. Using the measures takes the following form:

```
[mRank]
Measure=Plugin
Plugin=HDTStats.dll
Server="EU"
Type=Rank

[mSeasonWon]
Measure=Plugin
Plugin=HDTStats.dll
ParentName=mRank
Type=Won

[mSeasonLost]
Measure=Plugin
Plugin=HDTStats.dll
ParentName=mRank
Type=Lost

[mTodayWon]
Measure=Plugin
Plugin=HDTStats.dll
ParentName=mRank
Type=WonToday

[mTodayLost]
Measure=Plugin
Plugin=HDTStats.dll
ParentName=mRank
Type=LostToday
```

**NOTE**: *You need to have [Heathstone Deck Tracker](https://github.com/Epix37/Hearthstone-Deck-Tracker) installed and be using it to track your games in order for this plugin to work properly.*
