namespace Lynx_Event_Combine
{
    class LynxEventManager
    {
        public string eventFilePath { get; set; }
        public List<Event> events { get; set; }
        public List<string> eventNames
        {
            get { return events.Select(e => e.displayName).ToList(); }
        }

        // Saved events from last combine action
        private Event? _mainEvent;
        private List<Event> _eventsToCombine = new List<Event>();
        public bool hasCombinedData
        {
            get { return _mainEvent != null && _eventsToCombine.Count > 0; }
        }

        public LynxEventManager(string eventFilePath)
        {
            this.eventFilePath = eventFilePath;
            events = new List<Event>();
            LoadEvents();
        }

        public void LoadEvents()
        {
            if (!File.Exists(eventFilePath))
            {
                throw new FileNotFoundException(
                    $"The file at path {eventFilePath} does not exist."
                );
            }

            using (var reader = new StreamReader(eventFilePath))
            {
                Event? currentEvent = null;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split(',');

                    // Check if the line starts with a number (event definition)
                    if (int.TryParse(values[0], out int eventNumber))
                    {
                        currentEvent = new Event
                        {
                            eventNumber = eventNumber,
                            roundNumber = int.Parse(values[1]),
                            heatNumber = int.Parse(values[2]),
                            eventName = values[3],
                            distance = double.TryParse(values[4], out double distance)
                                ? distance
                                : 0,
                            fullTextString = line,
                            displayName = $"{values[3]} ({eventNumber},{values[1]},{values[2]})",
                        };
                        events.Add(currentEvent);
                    }
                    else if (currentEvent != null) // Entry line
                    {
                        var entry = new EventEntry
                        {
                            athleteNumber = int.TryParse(values[1], out int athleteNumber)
                                ? athleteNumber
                                : 0,
                            laneNumber = int.TryParse(values[2], out int laneNumber)
                                ? laneNumber
                                : 0,
                            lastName = values[3],
                            firstName = values[4],
                            teamName = values[5],
                            fullTextString = line,
                        };
                        currentEvent.entries.Add(entry);
                    }
                }
            }
        }

        public bool CombineEvents(string mainEventName, List<string> eventNamesToCombine)
        {
            bool noDuplicates = true;

            // Write new event file, combining the selected events
            using (var writer = new StreamWriter(eventFilePath))
            {
                foreach (var ev in events)
                {
                    // Write all events back to file
                    writer.WriteLine(ev.fullTextString);

                    // Write all original entries back to file
                    foreach (var entry in ev.entries)
                    {
                        writer.WriteLine(entry.fullTextString);
                    }

                    // If this is the main event whose entries we want to combine, write the entries from the eventsToCombine
                    if (ev.displayName.Equals(mainEventName))
                    {
                        _mainEvent = ev;
                        _eventsToCombine = events
                            .Where(e => eventNamesToCombine.Contains(e.displayName))
                            .ToList();

                        // track lane and athlete IDs to avoid duplicates
                        var laneNumbers = new HashSet<int>();
                        var athleteNumbers = new HashSet<int>();

                        foreach (var combinedEvent in _eventsToCombine)
                        {
                            // Write the entries from the event to combine
                            foreach (var entry in combinedEvent.entries)
                            {
                                // Check if lane or athlete number exists
                                if (
                                    laneNumbers.Contains(entry.laneNumber)
                                    || (
                                        entry.athleteNumber != 0
                                        && athleteNumbers.Contains(entry.athleteNumber)
                                    )
                                )
                                {
                                    noDuplicates = false;
                                }
                                else
                                {
                                    laneNumbers.Add(entry.laneNumber);
                                    athleteNumbers.Add(entry.athleteNumber);
                                }

                                writer.WriteLine(entry.fullTextString);
                            }
                        }
                    }
                }
            }

            return noDuplicates;
        }

        public (bool, string) SplitLif()
        {
            if (_mainEvent == null)
            {
                return (false, "No events were previously combined.");
            }

            // Find LIF file corresponding to main event
            string lifFilePath = Path.Combine(
                Path.GetDirectoryName(eventFilePath) ?? "",
                $"{_mainEvent.eventNumber.ToString("D3")}-{_mainEvent.roundNumber}-{_mainEvent.heatNumber.ToString("D2")}.lif"
            );

            if (!File.Exists(lifFilePath))
            {
                return (false, $"LIF file not found: {lifFilePath}");
            }

            // Create a new LIF file for each event in _eventsToCombine
            foreach (var eventToCombine in _eventsToCombine)
            {
                string newLifFilePath = Path.Combine(
                    Path.GetDirectoryName(lifFilePath) ?? "",
                    $"{eventToCombine.eventNumber.ToString("D3")}-{eventToCombine.roundNumber}-{eventToCombine.heatNumber.ToString("D2")}.lif"
                );
                using (var reader = new StreamReader(lifFilePath))
                using (var writer = new StreamWriter(newLifFilePath))
                {
                    // Update the first line to use the new event/round/heat numbers and event name
                    var firstLine = reader.ReadLine();
                    // Replace everything before the 4th comma with the new event details
                    var newFirstLine =
                        $"{eventToCombine.eventNumber},{eventToCombine.roundNumber},{eventToCombine.heatNumber},{eventToCombine.eventName}";
                    writer.WriteLine(newFirstLine);

                    // Write the rest of the lines from the original LIF file
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        // Write the line to the new LIF file
                        writer.WriteLine(line);
                    }
                }
            }

            // Great success!
            return (true, "LIF files split successfully.");
        }
    }
}
