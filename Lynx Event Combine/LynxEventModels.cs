namespace Lynx_Event_Combine
{
    public class Event
    {
        public int eventNumber;
        public int roundNumber;
        public int heatNumber;
        public string eventName = "";
        public double distance;
        public string fullTextString = "";
        public List<EventEntry> entries = new List<EventEntry>();
        public string displayName = "";
    }

    public class EventEntry
    {
        public string athleteNumber = "";
        public string laneNumber = "";
        public string lastName = "";
        public string firstName = "";
        public string teamName = "";
        public string fullTextString = "";
    }
}
