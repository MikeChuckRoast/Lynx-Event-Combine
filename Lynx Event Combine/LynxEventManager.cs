namespace Lynx_Event_Combine
{
    public class LynxEventManager
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

        // Option to remove gendered event name
        public bool removeGenderedEventName { get; set; } = true;

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
                            athleteNumber = values[1],
                            laneNumber = values[2],
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

            // Make sure eventNamesToCombine doesn't include mainEventName
            eventNamesToCombine = eventNamesToCombine.Where(e => !e.Equals(mainEventName)).ToList();

            // Backup the event file
            string backupFilePath =
                eventFilePath + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
            File.Copy(eventFilePath, backupFilePath);

            // Write new event file, combining the selected events
            using (var writer = new StreamWriter(eventFilePath))
            {
                foreach (var ev in events)
                {
                    // Write all events back to file
                    if (ev.displayName.Equals(mainEventName) && removeGenderedEventName)
                    {
                        writer.WriteLine(StripGenderedEventName(ev.fullTextString));
                    }
                    else
                    {
                        writer.WriteLine(ev.fullTextString);
                    }

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
                        var laneNumbers = new HashSet<string>();
                        var athleteNumbers = new HashSet<string>();
                        // Add all lane and athlete numbers from main event
                        foreach (var entry in _mainEvent.entries)
                        {
                            laneNumbers.Add(entry.laneNumber);
                            athleteNumbers.Add(entry.athleteNumber);
                        }

                        foreach (var combinedEvent in _eventsToCombine)
                        {
                            // Write the entries from the event to combine
                            foreach (var entry in combinedEvent.entries)
                            {
                                // Check if lane or athlete number exists
                                if (
                                    laneNumbers.Contains(entry.laneNumber)
                                    || (
                                        !String.IsNullOrEmpty(entry.athleteNumber)
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

            // Read the original LIF file into an array of strings
            var lifFileLines = File.ReadAllLines(lifFilePath);

            // Create a new LIF file for each event in _eventsToCombine
            foreach (var eventToCombine in _eventsToCombine)
            {
                if (!WriteFilteredLif(eventToCombine, lifFileLines, lifFilePath))
                {
                    return (
                        false,
                        $"Failed to write LIF file for event: {eventToCombine.displayName}"
                    );
                }
            }
            // Create a new LIF file for the main event
            if (!WriteFilteredLif(_mainEvent, lifFileLines, lifFilePath))
            {
                return (false, $"Failed to write LIF file for event: {_mainEvent.displayName}");
            }

            // Great success!
            return (true, "LIF files split successfully.");
        }

        private bool WriteFilteredLif(
            Event eventToCombine,
            string[] lifFileLines,
            string lifFilePath
        )
        {
            try
            {
                string newLifFilePath = Path.Combine(
                    Path.GetDirectoryName(lifFilePath) ?? "",
                    $"{eventToCombine.eventNumber.ToString("D3")}-{eventToCombine.roundNumber}-{eventToCombine.heatNumber.ToString("D2")}.lif"
                );

                using (var writer = new StreamWriter(newLifFilePath))
                {
                    // Update the first line to use the new event/round/heat numbers and event name
                    var firstLine = lifFileLines.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(firstLine))
                    {
                        var firstLineSplit = firstLine.Split(',');
                        var restOfLine = string.Join(",", firstLineSplit.Skip(4));
                        var newFirstLine =
                            $"{eventToCombine.eventNumber},{eventToCombine.roundNumber},{eventToCombine.heatNumber},{eventToCombine.eventName},{restOfLine}";
                        writer.WriteLine(newFirstLine);
                    }

                    // Process the rest of the lines
                    foreach (var line in lifFileLines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Write the line to the new LIF file if it matches the original entries
                        if (CheckResultInOriginalEntries(line, eventToCombine))
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing LIF file: {ex.Message}");
                return false;
            }
        }

        private bool CheckResultInOriginalEntries(string lifLine, Event originalEvent)
        {
            var splitLine = lifLine.Split(',');
            if (splitLine.Length < 3)
            {
                return false;
            }
            var laneNumber = splitLine[2];
            var athleteNumber = splitLine[1];
            // Check if the lane number and athlete number exist in the original event's entries
            foreach (var entry in originalEvent.entries)
            {
                if (
                    entry.laneNumber.Equals(laneNumber)
                    && (
                        String.IsNullOrEmpty(entry.athleteNumber)
                        || entry.athleteNumber.Equals(athleteNumber)
                    )
                )
                {
                    return true;
                }
            }
            return false;
        }

        public static string StripGenderedEventName(string eventFileLine)
        {
            if (eventFileLine == null)
            {
                return String.Empty;
            }

            // Split the eventFileLine into parts
            var parts = eventFileLine.Split(',');

            // Ensure the line has enough parts to include an event name
            if (parts.Length > 3)
            {
                // Get the event name
                var eventName = parts[3];

                // Check if the event name starts with a gendered prefix
                var nameParts = eventName.Split(' ');
                if (
                    nameParts.Length > 1
                    && (
                        nameParts[0].StartsWith("Girl", StringComparison.OrdinalIgnoreCase)
                        || nameParts[0].StartsWith("Boy", StringComparison.OrdinalIgnoreCase)
                        || nameParts[0].StartsWith("Men", StringComparison.OrdinalIgnoreCase)
                        || nameParts[0].StartsWith("Women", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    // Replace the event name with the new name (without the gendered prefix)
                    parts[3] = string.Join(" ", nameParts.Skip(1));
                }
            }

            // Reconstruct and return the full eventFileLine
            return string.Join(",", parts);
        }
    }
}
