using System.Text.Json.Serialization;

namespace Lynx_Event_Combine
{
    /// <summary>
    /// Identifies an event by the three numbers FinishLynx keys everything on.
    /// </summary>
    public class EventKey
    {
        public int number { get; set; }
        public int round { get; set; }
        public int heat { get; set; }

        public EventKey() { }

        public EventKey(Event ev)
        {
            number = ev.eventNumber;
            round = ev.roundNumber;
            heat = ev.heatNumber;
        }

        // The LIF file FinishLynx writes for this event
        [JsonIgnore]
        public string lifFileName
        {
            get { return $"{number.ToString("D3")}-{round}-{heat.ToString("D2")}.lif"; }
        }

        public bool Matches(Event ev)
        {
            return ev.eventNumber == number && ev.roundNumber == round && ev.heatNumber == heat;
        }

        public override string ToString()
        {
            return $"{number},{round},{heat}";
        }
    }

    /// <summary>
    /// One entry as it took part in a combine: the lane it was seeded in, and the lane it
    /// actually ran in. The two differ only when the combine re-assigned lane numbers.
    /// </summary>
    public class CombinedEntry
    {
        public string athleteNumber { get; set; } = "";
        public string seededLane { get; set; } = "";
        public string runLane { get; set; } = "";
    }

    /// <summary>
    /// One of the events taking part in a combine, and the entries it contributed. Splitting
    /// works entirely off these, so it does not depend on the event file still being loaded.
    /// </summary>
    public class CombineSource
    {
        public EventKey ev { get; set; } = new EventKey();

        // The event's own name, kept unstripped so the split results go back under the name
        // the meet management software is expecting
        public string eventName { get; set; } = "";
        public string displayName { get; set; } = "";
        public List<CombinedEntry> entries { get; set; } = new List<CombinedEntry>();
    }

    /// <summary>
    /// Everything needed to split a combined LIF file back into one LIF file per event. Saved
    /// alongside the event file, so a reload, a restart or a crash cannot lose it.
    /// </summary>
    public class CombineRecord
    {
        // Bumped if the shape of this file ever changes, so an old file can be recognised
        public int version { get; set; } = 1;

        public CombineSource mainEvent { get; set; } = new CombineSource();
        public List<CombineSource> sources { get; set; } = new List<CombineSource>();

        // The event the combined entries were written to, and therefore the event whose LIF
        // file holds the results. Same as the main event unless writeToNewEvent was set.
        public EventKey resultsEvent { get; set; } = new EventKey();

        public bool removeGenderedEventName { get; set; }
        public bool reassignLanes { get; set; }
        public bool writeToNewEvent { get; set; }

        public DateTime combinedAt { get; set; }
        public bool splitCompleted { get; set; }

        // The main event and the events added to it, which is every event a split writes to
        [JsonIgnore]
        public IEnumerable<CombineSource> allEvents
        {
            get { return sources.Append(mainEvent); }
        }
    }
}
