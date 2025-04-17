namespace Lynx_Event_Combine
{
    class Event
    {
        public int eventNumber;
        public int roundNumber;
        public int heatNumber;
        public string eventName = "";
        public double distance;
        public string fullTextString = "";
        public List<EventEntry> entries = new List<EventEntry>();
    }

    class EventEntry
    {
        public int athleteNumber;
        public int laneNumber;
        public string lastName = "";
        public string firstName = "";
        public string teamName = "";
        public string fullTextString = "";
    }
}
