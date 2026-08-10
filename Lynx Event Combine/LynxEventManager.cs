using System.Text.Json;

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

        // The last combine performed on this event file. Restored from the state file that sits
        // next to it, so a reload, a restart or a crash cannot lose the ability to split.
        private CombineRecord? _lastCombine;
        public CombineRecord? lastCombine
        {
            get { return _lastCombine; }
        }

        public bool hasCombinedData
        {
            get { return _lastCombine != null; }
        }

        /// <summary>
        /// One line describing the saved combine, for the status area of the form.
        /// </summary>
        public string combineDescription
        {
            get
            {
                if (_lastCombine == null)
                {
                    return "";
                }

                int count = _lastCombine.sources.Count;

                // Where the results land is only worth saying when it is not the main event
                string target =
                    _lastCombine.resultsEvent.ToString() == _lastCombine.mainEvent.ev.ToString()
                        ? ""
                        : $" → event {_lastCombine.resultsEvent}";

                return $"{_lastCombine.mainEvent.displayName} + {count} {(count == 1 ? "event" : "events")}{target}";
            }
        }

        // FinishLynx pairs lynx.evt with lynx.sch, so the saved combine takes the same base name
        public string combineStateFilePath
        {
            get
            {
                return Path.Combine(
                    Path.GetDirectoryName(eventFilePath) ?? "",
                    Path.GetFileNameWithoutExtension(eventFilePath) + ".combine.json"
                );
            }
        }

        // Option to remove gendered event name
        public bool removeGenderedEventName { get; set; } = true;

        // Option to number the combined entries 1..n instead of keeping their original lanes
        public bool reassignLanes { get; set; } = false;

        // Option to write the combined entries to a new event instead of into the main event
        public bool writeToNewEvent { get; set; } = false;

        // Event number created by the last combine, when writeToNewEvent was set
        public int? lastNewEventNumber { get; private set; }

        // Non-fatal problem from the last combine, such as a schedule file that could not be updated
        public string? lastCombineWarning { get; private set; }

        public LynxEventManager(string eventFilePath)
        {
            this.eventFilePath = eventFilePath;
            events = new List<Event>();
            LoadEvents();
            LoadCombineState();
        }

        public void LoadEvents()
        {
            if (!File.Exists(eventFilePath))
            {
                throw new FileNotFoundException(
                    $"The file at path {eventFilePath} does not exist."
                );
            }

            events.Clear();

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
                        // Trailing columns are optional, so a line may stop at the event name
                        int roundNumber = int.TryParse(GetField(values, 1), out int round)
                            ? round
                            : 1;
                        int heatNumber = int.TryParse(GetField(values, 2), out int heat) ? heat : 1;
                        string eventName = GetField(values, 3);

                        currentEvent = new Event
                        {
                            eventNumber = eventNumber,
                            roundNumber = roundNumber,
                            heatNumber = heatNumber,
                            eventName = eventName,
                            distance = double.TryParse(GetField(values, 4), out double distance)
                                ? distance
                                : 0,
                            fullTextString = line,
                            displayName = $"{eventName} ({eventNumber},{roundNumber},{heatNumber})",
                        };
                        events.Add(currentEvent);
                    }
                    else if (currentEvent != null) // Entry line
                    {
                        var entry = new EventEntry
                        {
                            athleteNumber = GetField(values, 1),
                            laneNumber = GetField(values, 2),
                            lastName = GetField(values, 3),
                            firstName = GetField(values, 4),
                            teamName = GetField(values, 5),
                            fullTextString = line,
                        };
                        currentEvent.entries.Add(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a comma separated column, treating columns past the end of the line as empty.
        /// Lynx files leave optional trailing columns off entirely rather than padding them.
        /// </summary>
        private static string GetField(string[] values, int index)
        {
            return index < values.Length ? values[index] : "";
        }

        private static readonly JsonSerializerOptions combineStateJsonOptions =
            new JsonSerializerOptions { WriteIndented = true };

        /// <summary>
        /// Reads the combine saved next to the event file, if there is one. A state file that
        /// cannot be read, or that describes an event no longer in the file, is discarded and
        /// reported rather than thrown, since the event file itself is still perfectly usable.
        /// </summary>
        public void LoadCombineState()
        {
            _lastCombine = null;

            if (!File.Exists(combineStateFilePath))
            {
                return;
            }

            CombineRecord? record;
            try
            {
                record = JsonSerializer.Deserialize<CombineRecord>(
                    File.ReadAllText(combineStateFilePath),
                    combineStateJsonOptions
                );
            }
            catch (Exception ex)
            {
                lastCombineWarning =
                    $"The saved combine could not be read: {ex.Message}\r\n\r\n"
                    + "Combine the events again before splitting the results.";
                return;
            }

            if (record == null || record.mainEvent == null || record.resultsEvent == null)
            {
                return;
            }

            // A saved combine only makes sense against the event file it was made from. If the
            // main event has since gone, the event file was replaced and the saved lane mapping
            // can no longer be trusted to route results correctly.
            if (!events.Any(e => record.mainEvent.ev.Matches(e)))
            {
                lastCombineWarning =
                    $"The saved combine refers to event {record.mainEvent.ev}, which is no longer "
                    + "in this event file.\r\n\r\nIt has been discarded. Combine the events again "
                    + "before splitting the results.";
                ClearCombineState();
                return;
            }

            _lastCombine = record;
        }

        private void SaveCombineState()
        {
            try
            {
                if (_lastCombine == null)
                {
                    if (File.Exists(combineStateFilePath))
                    {
                        File.Delete(combineStateFilePath);
                    }
                    return;
                }

                File.WriteAllText(
                    combineStateFilePath,
                    JsonSerializer.Serialize(_lastCombine, combineStateJsonOptions)
                );
            }
            catch (Exception ex)
            {
                lastCombineWarning =
                    $"The combine could not be saved to {combineStateFilePath}: {ex.Message}\r\n\r\n"
                    + "Split the results before closing the program or loading another file.";
            }
        }

        /// <summary>
        /// Forgets the saved combine. The event file is left as it is; only the ability to
        /// split its results is given up.
        /// </summary>
        public void ClearCombineState()
        {
            _lastCombine = null;
            SaveCombineState();
        }

        public bool CombineEvents(string mainEventName, List<string> eventNamesToCombine)
        {
            bool noDuplicates = true;
            lastCombineWarning = null;
            lastNewEventNumber = null;

            // Make sure eventNamesToCombine doesn't include mainEventName
            eventNamesToCombine = eventNamesToCombine.Where(e => !e.Equals(mainEventName)).ToList();

            var mainEvent = events.FirstOrDefault(e => e.displayName.Equals(mainEventName));
            Event? newEvent = null;
            var eventsToCombine = new List<Event>();

            if (mainEvent != null)
            {
                eventsToCombine = events
                    .Where(e => eventNamesToCombine.Contains(e.displayName))
                    .ToList();

                noDuplicates = AssignCombinedLanes(mainEvent, eventsToCombine);

                if (writeToNewEvent)
                {
                    newEvent = BuildNewCombinedEvent(mainEvent, eventsToCombine);
                }
            }

            // Backup the event file
            BackupFile(eventFilePath);

            // Write new event file, combining the selected events
            using (var writer = new StreamWriter(eventFilePath))
            {
                foreach (var ev in events)
                {
                    bool isMainEvent = ev == mainEvent;

                    // The main event only becomes the combined event when we write in place
                    bool isCombinedEvent = isMainEvent && !writeToNewEvent;

                    // Write all events back to file
                    if (isCombinedEvent && removeGenderedEventName)
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
                        writer.WriteLine(
                            isCombinedEvent ? CombinedEntryLine(entry) : entry.fullTextString
                        );
                    }

                    // If this is the main event whose entries we want to combine, write the entries from the eventsToCombine
                    if (isCombinedEvent)
                    {
                        WriteCombinedEntries(writer, eventsToCombine);
                    }
                }

                // Otherwise the combine goes to a brand new event at the end of the file
                if (newEvent != null)
                {
                    writer.WriteLine(newEvent.fullTextString);
                    foreach (var entry in newEvent.entries)
                    {
                        writer.WriteLine(entry.fullTextString);
                    }
                }
            }

            if (mainEvent != null)
            {
                if (newEvent != null)
                {
                    UpdateScheduleFile(mainEvent, newEvent);
                    lastNewEventNumber = newEvent.eventNumber;
                }

                _lastCombine = BuildCombineRecord(mainEvent, eventsToCombine, newEvent);
                SaveCombineState();
            }

            // The file now holds the combined entries, so re-read it. Without this the in-memory
            // events still describe the file as it was before the combine, and the next combine
            // would write that stale picture back over this one.
            LoadEvents();

            return noDuplicates;
        }

        /// <summary>
        /// Captures what a split needs to know: which events took part, and which lane each
        /// entry was seeded in against the lane it will actually run in.
        /// </summary>
        private CombineRecord BuildCombineRecord(
            Event mainEvent,
            List<Event> eventsToCombine,
            Event? newEvent
        )
        {
            return new CombineRecord
            {
                mainEvent = BuildCombineSource(mainEvent),
                sources = eventsToCombine.Select(BuildCombineSource).ToList(),
                resultsEvent = new EventKey(newEvent ?? mainEvent),
                removeGenderedEventName = removeGenderedEventName,
                reassignLanes = reassignLanes,
                writeToNewEvent = writeToNewEvent,
                combinedAt = DateTime.Now,
                splitCompleted = false,
            };
        }

        private static CombineSource BuildCombineSource(Event ev)
        {
            return new CombineSource
            {
                ev = new EventKey(ev),
                eventName = ev.eventName,
                displayName = ev.displayName,
                entries = ev
                    .entries.Select(entry => new CombinedEntry
                    {
                        athleteNumber = entry.athleteNumber,
                        seededLane = entry.laneNumber,
                        runLane = string.IsNullOrEmpty(entry.assignedLaneNumber)
                            ? entry.laneNumber
                            : entry.assignedLaneNumber,
                    })
                    .ToList(),
            };
        }

        /// <summary>
        /// Works out the lane each entry will run in once the events are combined, and reports
        /// whether that produced any duplicates. When reassignLanes is set the combined entries
        /// are numbered 1..n so lanes cannot collide, and only athlete numbers are checked.
        /// </summary>
        private bool AssignCombinedLanes(Event mainEvent, List<Event> eventsToCombine)
        {
            bool noDuplicates = true;

            // Drop any assignment left over from a previous combine of these same events
            foreach (var ev in eventsToCombine.Prepend(mainEvent))
            {
                foreach (var entry in ev.entries)
                {
                    entry.assignedLaneNumber = "";
                }
            }

            // track lane and athlete IDs to avoid duplicates
            var laneNumbers = new HashSet<string>();
            var athleteNumbers = new HashSet<string>();
            int nextLaneNumber = 1;

            // Add all lane and athlete numbers from main event
            foreach (var entry in mainEvent.entries)
            {
                if (reassignLanes)
                {
                    entry.assignedLaneNumber = (nextLaneNumber++).ToString();
                }
                laneNumbers.Add(entry.laneNumber);
                athleteNumbers.Add(entry.athleteNumber);
            }

            foreach (var combinedEvent in eventsToCombine)
            {
                foreach (var entry in combinedEvent.entries)
                {
                    if (reassignLanes)
                    {
                        entry.assignedLaneNumber = (nextLaneNumber++).ToString();
                    }

                    // Check if lane or athlete number exists
                    if (
                        (!reassignLanes && laneNumbers.Contains(entry.laneNumber))
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
                }
            }

            return noDuplicates;
        }

        /// <summary>
        /// Builds the event that will hold the combined entries, using the next unused event number.
        /// </summary>
        private Event BuildNewCombinedEvent(Event mainEvent, List<Event> eventsToCombine)
        {
            int newEventNumber = events.Count > 0 ? events.Max(e => e.eventNumber) + 1 : 1;

            var headerLine = removeGenderedEventName
                ? StripGenderedEventName(mainEvent.fullTextString)
                : mainEvent.fullTextString;

            var parts = headerLine.Split(',');
            if (parts.Length > 2)
            {
                parts[0] = newEventNumber.ToString();
                parts[1] = "1";
                parts[2] = "1";
                headerLine = string.Join(",", parts);
            }
            var eventName = parts.Length > 3 ? parts[3] : mainEvent.eventName;

            return new Event
            {
                eventNumber = newEventNumber,
                roundNumber = 1,
                heatNumber = 1,
                eventName = eventName,
                distance = mainEvent.distance,
                fullTextString = headerLine,
                displayName = $"{eventName} ({newEventNumber},1,1)",
                // Snapshot the entries rather than sharing them, so this event keeps the lanes
                // it was written with even if the source events are combined again later
                entries = mainEvent
                    .entries.Concat(eventsToCombine.SelectMany(e => e.entries))
                    .Select(entry => new EventEntry
                    {
                        athleteNumber = entry.athleteNumber,
                        laneNumber = string.IsNullOrEmpty(entry.assignedLaneNumber)
                            ? entry.laneNumber
                            : entry.assignedLaneNumber,
                        lastName = entry.lastName,
                        firstName = entry.firstName,
                        teamName = entry.teamName,
                        fullTextString = CombinedEntryLine(entry),
                    })
                    .ToList(),
            };
        }

        /// <summary>
        /// Adds the new event to the schedule file, immediately before the main event it was
        /// combined from. A schedule that cannot be updated is reported through lastCombineWarning
        /// rather than failing the combine, since the event file has already been written.
        /// </summary>
        private void UpdateScheduleFile(Event mainEvent, Event newEvent)
        {
            // FinishLynx pairs lynx.evt with lynx.sch, so the schedule takes the event file's name
            string scheduleFilePath = Path.Combine(
                Path.GetDirectoryName(eventFilePath) ?? "",
                Path.GetFileNameWithoutExtension(eventFilePath) + ".sch"
            );

            if (!File.Exists(scheduleFilePath))
            {
                lastCombineWarning =
                    $"Schedule file not found: {scheduleFilePath}\r\n\r\n"
                    + $"Event {newEvent.eventNumber} was added to the event file only.";
                return;
            }

            try
            {
                BackupFile(scheduleFilePath);

                var scheduleLines = File.ReadAllLines(scheduleFilePath).ToList();
                var newScheduleLine =
                    $"{newEvent.eventNumber},{newEvent.roundNumber},{newEvent.heatNumber}";

                int insertIndex = scheduleLines.FindIndex(line =>
                    ScheduleLineMatchesEvent(line, mainEvent)
                );
                if (insertIndex < 0)
                {
                    scheduleLines.Add(newScheduleLine);
                    lastCombineWarning =
                        $"Event {mainEvent.eventNumber},{mainEvent.roundNumber},{mainEvent.heatNumber} "
                        + $"was not found in {Path.GetFileName(scheduleFilePath)}.\r\n\r\n"
                        + $"Event {newEvent.eventNumber} was added to the end of the schedule instead.";
                }
                else
                {
                    scheduleLines.Insert(insertIndex, newScheduleLine);
                }

                File.WriteAllLines(scheduleFilePath, scheduleLines);
            }
            catch (Exception ex)
            {
                lastCombineWarning = $"Could not update the schedule file: {ex.Message}";
            }
        }

        private static bool ScheduleLineMatchesEvent(string scheduleLine, Event ev)
        {
            var parts = scheduleLine.Split(',');
            return parts.Length >= 3
                && int.TryParse(parts[0], out int eventNumber)
                && eventNumber == ev.eventNumber
                && int.TryParse(parts[1], out int roundNumber)
                && roundNumber == ev.roundNumber
                && int.TryParse(parts[2], out int heatNumber)
                && heatNumber == ev.heatNumber;
        }

        private static void WriteCombinedEntries(StreamWriter writer, List<Event> eventsToWrite)
        {
            foreach (var ev in eventsToWrite)
            {
                foreach (var entry in ev.entries)
                {
                    writer.WriteLine(CombinedEntryLine(entry));
                }
            }
        }

        private static string CombinedEntryLine(EventEntry entry)
        {
            return string.IsNullOrEmpty(entry.assignedLaneNumber)
                ? entry.fullTextString
                : ReplaceLaneNumber(entry.fullTextString, entry.assignedLaneNumber);
        }

        private static void BackupFile(string filePath)
        {
            string backupFilePath =
                filePath + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
            File.Copy(filePath, backupFilePath);
        }

        /// <summary>
        /// Replaces the lane field, which is the third column of both entry lines and LIF result lines.
        /// </summary>
        private static string ReplaceLaneNumber(string line, string laneNumber)
        {
            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                return line;
            }
            parts[2] = laneNumber;
            return string.Join(",", parts);
        }

        public (bool, string) SplitLif()
        {
            if (_lastCombine == null)
            {
                return (false, "No events were previously combined.");
            }

            // Find LIF file corresponding to the event the combined entries were written to
            string lifFilePath = Path.Combine(
                Path.GetDirectoryName(eventFilePath) ?? "",
                _lastCombine.resultsEvent.lifFileName
            );
            if (!File.Exists(lifFilePath))
            {
                return (false, $"LIF file not found: {lifFilePath}");
            }

            // Read the original LIF file into an array of strings
            var lifFileLines = File.ReadAllLines(lifFilePath);

            // Create a new LIF file for the main event and for every event added to it
            foreach (var source in _lastCombine.allEvents)
            {
                if (!WriteFilteredLif(source, lifFileLines, lifFilePath))
                {
                    return (false, $"Failed to write LIF file for event: {source.displayName}");
                }
            }

            _lastCombine.splitCompleted = true;
            SaveCombineState();

            // Great success!
            return (true, "LIF files split successfully.");
        }

        private static bool WriteFilteredLif(
            CombineSource eventToCombine,
            string[] lifFileLines,
            string lifFilePath
        )
        {
            try
            {
                string newLifFilePath = Path.Combine(
                    Path.GetDirectoryName(lifFilePath) ?? "",
                    eventToCombine.ev.lifFileName
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
                            $"{eventToCombine.ev.number},{eventToCombine.ev.round},{eventToCombine.ev.heat},{eventToCombine.eventName},{restOfLine}";
                        writer.WriteLine(newFirstLine);
                    }

                    // Process the rest of the lines
                    foreach (var line in lifFileLines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Write the line to the new LIF file if it matches the original entries
                        var matchingEntry = FindResultInOriginalEntries(line, eventToCombine);
                        if (matchingEntry == null)
                            continue;

                        // Hand the result back with the lane the athlete was originally seeded in,
                        // which is what the meet management software is expecting
                        writer.WriteLine(
                            matchingEntry.runLane.Equals(matchingEntry.seededLane)
                                ? line
                                : ReplaceLaneNumber(line, matchingEntry.seededLane)
                        );
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

        private static CombinedEntry? FindResultInOriginalEntries(
            string lifLine,
            CombineSource originalEvent
        )
        {
            var splitLine = lifLine.Split(',');
            if (splitLine.Length < 3)
            {
                return null;
            }
            var laneNumber = splitLine[2];
            var athleteNumber = splitLine[1];
            // Check if the lane number and athlete number exist in the original event's entries
            foreach (var entry in originalEvent.entries)
            {
                // The result carries the lane the athlete actually ran in, which is the
                // re-assigned lane whenever the combine renumbered them
                if (
                    entry.runLane.Equals(laneNumber)
                    && (
                        String.IsNullOrEmpty(entry.athleteNumber)
                        || entry.athleteNumber.Equals(athleteNumber)
                    )
                )
                {
                    return entry;
                }
            }
            return null;
        }

        public static string StripGenderedEventName(string? eventFileLine)
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
