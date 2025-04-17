namespace Lynx_Event_Combine
{
    class LynxEventManager
    {
        public string eventFilePath { get; set; }
        public List<Event> events { get; set; }
        public List<string> eventNames
        {
            get
            {
                HashSet<string> names = new HashSet<string>();
                foreach (var ev in events)
                {
                    names.Add(ev.eventName);
                }
                return names.ToList();
            }
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
                Event currentEvent = null;

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

        public bool CombineEvents(string mainEvent, List<string> eventsToCombine)
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
                    if (ev.eventName.Equals(mainEvent))
                    {
                        // track lane and athlete IDs to avoid duplicates
                        var laneNumbers = new HashSet<int>();
                        var athleteNumbers = new HashSet<int>();

                        // Find the events to combine
                        foreach (var eventInnterLoop in events)
                        {
                            if (eventsToCombine.Contains(eventInnterLoop.eventName))
                            {
                                // Write the entries from the event to combine
                                foreach (var entry in eventInnterLoop.entries)
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
            }

            return noDuplicates;
        }
    }
}
