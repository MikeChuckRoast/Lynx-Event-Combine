using Lynx_Event_Combine;

namespace LynxEventCombineTest
{
    public class LynxEventManagerTests : IDisposable
    {
        // Combining writes a state file next to the event file. The temp names these tests use
        // come from Path.GetTempFileName and get recycled once deleted, so a state file left
        // behind by one run could attach itself to an unrelated event file in the next one.
        public void Dispose()
        {
            foreach (var stateFile in Directory.GetFiles(Path.GetTempPath(), "tmp*.combine.json"))
            {
                try
                {
                    File.Delete(stateFile);
                }
                catch (IOException) { }
            }
        }

        #region StripGenderedEventName Tests
        [Fact]
        public void StripGenderedEventName_RemovesGenderPrefix()
        {
            // Arrange
            string input = "1,1,1,Girl 100m,0.0";
            string expected = "1,1,1,100m,0.0";

            // Act
            string result = LynxEventManager.StripGenderedEventName(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StripGenderedEventName_NoGenderPrefix_ReturnsOriginal()
        {
            // Arrange
            string input = "1,1,1,100m,0.0";

            // Act
            string result = LynxEventManager.StripGenderedEventName(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void StripGenderedEventName_HandlesEmptyString()
        {
            // Arrange
            string input = "";
            string expected = "";

            // Act
            string result = LynxEventManager.StripGenderedEventName(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StripGenderedEventName_HandlesNullInput()
        {
            // Arrange
            string? input = null;
            string expected = "";

            // Act
            string result = LynxEventManager.StripGenderedEventName(input);

            // Assert
            Assert.Equal(expected, result);
        }
        #endregion StripGenderedEventName Tests

        #region CombineEvents Tests
        [Fact]
        public void CombineEvents_CombinesEntriesCorrectly_WithProvidedFile()
        {
            // Arrange
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            string testFilePath = Path.Combine(projectDirectory, "Resources", "lynx.evt"); string tempFilePath = Path.GetTempFileName();
            File.Copy(testFilePath, tempFilePath, overwrite: true);

            var manager = new LynxEventManager(tempFilePath);

            // Act
            bool result = manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Assert
            Assert.True(result, "CombineEvents should return true when there are no duplicate entries.");

            // Verify the combined data
            string combinedFileContent = File.ReadAllText(tempFilePath);
            Assert.Contains("39,1,1,4x400 Relay Varsity", combinedFileContent);

            // Count the number of entries for the "4x400 Relay Varsity" event
            var lines = combinedFileContent.Split('\n');
            int entryCount = 0;
            bool isInTargetEvent = false;

            foreach (var line in lines)
            {
                if (line.Contains("39,1,1,4x400 Relay Varsity"))
                {
                    isInTargetEvent = true;
                }
                else if (isInTargetEvent && line.StartsWith(","))
                {
                    entryCount++;
                }
                else if (!line.StartsWith(",") && !string.IsNullOrWhiteSpace(line))
                {
                    isInTargetEvent = false;
                }
            }

            Assert.Equal(8, entryCount);

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void CombineEvents_LaneConflict()
        {
            // Arrange
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            string testFilePath = Path.Combine(projectDirectory, "Resources", "lynx_w_conflicts.evt"); string tempFilePath = Path.GetTempFileName();
            File.Copy(testFilePath, tempFilePath, overwrite: true);

            var manager = new LynxEventManager(tempFilePath);

            // Act
            bool result = manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Assert
            Assert.False(result, "Should identify that not all combined lane numbers are unique");

            // Verify the combined data
            string combinedFileContent = File.ReadAllText(tempFilePath);
            Assert.Contains("39,1,1,4x400 Relay Varsity", combinedFileContent);

            // Count the number of entries for the "4x400 Relay Varsity" event
            var lines = combinedFileContent.Split('\n');
            int entryCount = 0;
            bool isInTargetEvent = false;

            foreach (var line in lines)
            {
                if (line.Contains("39,1,1,4x400 Relay Varsity"))
                {
                    isInTargetEvent = true;
                }
                else if (isInTargetEvent && line.StartsWith(","))
                {
                    entryCount++;
                }
                else if (!line.StartsWith(",") && !string.IsNullOrWhiteSpace(line))
                {
                    isInTargetEvent = false;
                }
            }

            Assert.Equal(8, entryCount);

            // Cleanup
            File.Delete(tempFilePath);
        }
        #endregion CombineEvents Tests

        #region Lane Re-assignment Tests
        [Fact]
        public void CombineEvents_ReassignLanes_ResolvesLaneConflicts()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx_w_conflicts.evt");

            var manager = new LynxEventManager(tempFilePath) { reassignLanes = true };

            // Act
            bool result = manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Assert
            Assert.True(result, "Re-assigned lanes cannot collide, so the combine should report no duplicates.");

            // The combined event is numbered straight through from 1
            var combinedLanes = GetEntryLanes(tempFilePath, "39,1,1,");
            Assert.Equal(new[] { "1", "2", "3", "4", "5", "6", "7", "8" }, combinedLanes);

            // The events that were added keep the lanes they were seeded in
            Assert.Equal(new[] { "1", "2" }, GetEntryLanes(tempFilePath, "41,1,1,"));
            Assert.Equal(new[] { "1", "2" }, GetEntryLanes(tempFilePath, "42,1,1,"));
            Assert.Equal(new[] { "7", "8" }, GetEntryLanes(tempFilePath, "40,1,1,"));

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void CombineEvents_ReassignLanes_StillReportsDuplicateAthleteIds()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx_w_athlete_conflicts.evt");

            var manager = new LynxEventManager(tempFilePath) { reassignLanes = true };

            // Act
            bool result = manager.CombineEvents("Boys 800 Meters Varsity (1,1,1)", new List<string>
            {
                "Boys 800 Meters JV (2,1,1)"
            });

            // Assert
            Assert.False(result, "Re-assigning lanes should not hide a duplicate athlete ID.");

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void SplitLif_RestoresOriginalLanesAfterReassignment()
        {
            // Arrange
            string tempEventFilePath = CopyResourceToTempFile("lynx.evt");
            string tempLifFilePath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");

            var manager = new LynxEventManager(tempEventFilePath) { reassignLanes = true };
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // FinishLynx records results against the lanes the athletes actually ran in,
            // which after re-assignment are 1 through 8
            var lifLines = new List<string> { "39,1,1,4x400 Relay Varsity,,,,,,1600,18:47:26.4265" };
            for (int lane = 1; lane <= 8; lane++)
            {
                lifLines.Add($"{lane},,{lane},Milan,,MILA  A,4:0{lane}.000");
            }
            File.WriteAllLines(tempLifFilePath, lifLines);

            // Act
            var (success, message) = manager.SplitLif();

            // Assert
            Assert.True(success, message);

            // Each event gets its results back in the lanes it was seeded in:
            // 39 ran 5,6  41 ran 1,2  42 ran 3,4  40 ran 7,8
            Assert.Equal(new[] { "5", "6" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "039-1-01.lif")));
            Assert.Equal(new[] { "1", "2" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "041-1-01.lif")));
            Assert.Equal(new[] { "3", "4" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "042-1-01.lif")));
            Assert.Equal(new[] { "7", "8" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "040-1-01.lif")));

            // Cleanup
            File.Delete(tempEventFilePath);
            foreach (var eventNumber in new[] { "039", "040", "041", "042" })
            {
                File.Delete(Path.Combine(Path.GetTempPath(), $"{eventNumber}-1-01.lif"));
            }
        }
        #endregion Lane Re-assignment Tests

        #region New Event Number Tests
        [Fact]
        public void CombineEvents_NewEventNumber_AppendsEventAndLeavesMainEventAlone()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");

            var manager = new LynxEventManager(tempFilePath) { writeToNewEvent = true };

            // Act
            bool result = manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Assert
            Assert.True(result, "CombineEvents should return true when there are no duplicate entries.");

            // 42 is the highest event number in the file, so the combine goes to 43
            Assert.Equal(43, manager.lastNewEventNumber);

            string combinedFileContent = File.ReadAllText(tempFilePath);
            Assert.Contains("43,1,1,4x400 Relay Varsity,,,,,,1600", combinedFileContent);
            Assert.Equal(8, GetEntryLanes(tempFilePath, "43,1,1,").Length);

            // The main event is untouched: it keeps its gendered name and only its own entries
            Assert.Contains("39,1,1,Boys 4x400 Relay Varsity", combinedFileContent);
            Assert.Equal(new[] { "5", "6" }, GetEntryLanes(tempFilePath, "39,1,1,"));

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void CombineEvents_NewEventNumber_InsertsIntoScheduleBeforeMainEvent()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");
            string tempSchedulePath = Path.ChangeExtension(tempFilePath, ".sch");
            File.Copy(GetResourcePath("lynx.sch"), tempSchedulePath, overwrite: true);

            var manager = new LynxEventManager(tempFilePath) { writeToNewEvent = true };

            // Act
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)"
            });

            // Assert
            Assert.Null(manager.lastCombineWarning);

            var scheduleLines = File.ReadAllLines(tempSchedulePath);
            int newEventIndex = Array.IndexOf(scheduleLines, "43,1,1");
            int mainEventIndex = Array.IndexOf(scheduleLines, "39,1,1");

            Assert.True(newEventIndex >= 0, "The new event was not added to the schedule.");
            Assert.Equal(mainEventIndex - 1, newEventIndex);

            // The schedule is backed up before it is rewritten
            var scheduleBackups = Directory.GetFiles(
                Path.GetTempPath(),
                Path.GetFileName(tempSchedulePath) + "_*.bak"
            );
            Assert.NotEmpty(scheduleBackups);

            // Cleanup
            File.Delete(tempFilePath);
            File.Delete(tempSchedulePath);
            foreach (var backup in scheduleBackups)
            {
                File.Delete(backup);
            }
        }

        [Fact]
        public void CombineEvents_NewEventNumber_WarnsWhenScheduleFileIsMissing()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");
            string tempSchedulePath = Path.ChangeExtension(tempFilePath, ".sch");
            if (File.Exists(tempSchedulePath))
            {
                File.Delete(tempSchedulePath);
            }

            var manager = new LynxEventManager(tempFilePath) { writeToNewEvent = true };

            // Act
            bool result = manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)"
            });

            // Assert: the event file still gets written, the schedule problem is only reported
            Assert.True(result);
            Assert.Equal(43, manager.lastNewEventNumber);
            Assert.Contains("Schedule file not found", manager.lastCombineWarning);
            Assert.Contains("43,1,1,4x400 Relay Varsity", File.ReadAllText(tempFilePath));

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void SplitLif_UsesTheNewEventsLifFile()
        {
            // Arrange
            string tempEventFilePath = CopyResourceToTempFile("lynx.evt");
            string newEventLifPath = Path.Combine(Path.GetTempPath(), "043-1-01.lif");

            var manager = new LynxEventManager(tempEventFilePath) { writeToNewEvent = true };
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Results come back against event 43, in the lanes the entries were seeded in
            var lifLines = new List<string> { "43,1,1,4x400 Relay Varsity,,,,,,1600,18:47:26.4265" };
            for (int lane = 1; lane <= 8; lane++)
            {
                lifLines.Add($"{lane},,{lane},Milan,,MILA  A,4:0{lane}.000");
            }
            File.WriteAllLines(newEventLifPath, lifLines);

            // Act
            var (success, message) = manager.SplitLif();

            // Assert
            Assert.True(success, message);
            Assert.Equal(new[] { "5", "6" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "039-1-01.lif")));
            Assert.Equal(new[] { "7", "8" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "040-1-01.lif")));
            Assert.Equal(new[] { "1", "2" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "041-1-01.lif")));
            Assert.Equal(new[] { "3", "4" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "042-1-01.lif")));

            // Cleanup
            File.Delete(tempEventFilePath);
            File.Delete(newEventLifPath);
            foreach (var eventNumber in new[] { "039", "040", "041", "042" })
            {
                File.Delete(Path.Combine(Path.GetTempPath(), $"{eventNumber}-1-01.lif"));
            }
        }
        #endregion New Event Number Tests

        #region LoadEvents Tests
        [Fact]
        public void LoadEvents_HandlesLinesWithMissingTrailingFields()
        {
            // Arrange: an event file with no distance column and an entry with no team,
            // which is what MeetUploader writes
            string tempFilePath = CopyResourceToTempFile("lynx_short_fields.evt");

            // Act
            var manager = new LynxEventManager(tempFilePath);

            // Assert
            Assert.Equal(2, manager.events.Count);
            Assert.Equal("Boys 100m (1,1,1)", manager.events[0].displayName);
            Assert.Equal(0, manager.events[0].distance);
            Assert.Equal(2, manager.events[0].entries.Count);

            // The entry that stops after the first name still loads, with an empty team
            var entryWithoutTeam = manager.events[0].entries[1];
            Assert.Equal("2", entryWithoutTeam.laneNumber);
            Assert.Equal("Jones", entryWithoutTeam.lastName);
            Assert.Equal("", entryWithoutTeam.teamName);

            // Cleanup
            File.Delete(tempFilePath);
        }
        #endregion LoadEvents Tests

        #region Saved Combine Tests
        [Fact]
        public void CombineEvents_SecondCombine_KeepsTheFirstCombineInTheFile()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");

            var manager = new LynxEventManager(tempFilePath);
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Act: a second, unrelated combine rewrites the whole event file
            bool result = manager.CombineEvents("Boys 4x800 Relay Varsity (1,1,1)", new List<string>
            {
                "Boys 4x800 Relay JV (2,1,1)"
            });

            // Assert
            Assert.True(result);

            // The 4x800 events were merged
            Assert.Equal(new[] { "3", "4", "5", "6", "7", "8" }, GetEntryLanes(tempFilePath, "1,1,1,"));

            // ...and the earlier 4x400 combine is still there, name and all
            Assert.Contains("39,1,1,4x400 Relay Varsity", File.ReadAllText(tempFilePath));
            Assert.Equal(8, GetEntryLanes(tempFilePath, "39,1,1,").Length);

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void CombineEvents_SavesTheCombineNextToTheEventFile()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");
            string expectedStatePath = CombineStateFilePathFor(tempFilePath);

            var manager = new LynxEventManager(tempFilePath) { reassignLanes = true };

            // Act
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Girls 4x400 Relay Varsity (41,1,1)"
            });

            // Assert
            Assert.True(File.Exists(expectedStatePath), $"No combine was saved at {expectedStatePath}.");
            Assert.Equal(expectedStatePath, manager.combineStateFilePath);

            // A manager built fresh from the same file reads the combine back
            var reloaded = new LynxEventManager(tempFilePath);
            Assert.True(reloaded.hasCombinedData);
            Assert.Null(reloaded.lastCombineWarning);

            var record = reloaded.lastCombine!;
            Assert.Equal("39,1,1", record.mainEvent.ev.ToString());
            Assert.Equal("39,1,1", record.resultsEvent.ToString());
            Assert.True(record.reassignLanes);
            Assert.False(record.splitCompleted);

            // The main event ran in lanes 1 and 2 but was seeded in 5 and 6
            Assert.Equal(new[] { "5", "6" }, record.mainEvent.entries.Select(e => e.seededLane).ToArray());
            Assert.Equal(new[] { "1", "2" }, record.mainEvent.entries.Select(e => e.runLane).ToArray());

            // Cleanup
            File.Delete(tempFilePath);
            File.Delete(expectedStatePath);
        }

        [Fact]
        public void SplitLif_WorksWithAManagerReloadedFromDisk()
        {
            // Arrange
            string tempEventFilePath = CopyResourceToTempFile("lynx.evt");
            string tempLifFilePath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");
            File.Copy(GetResourcePath("039-1-01.lif"), tempLifFilePath, overwrite: true);

            var manager = new LynxEventManager(tempEventFilePath);
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Act: stand in for closing the program, or reloading, between the combine and the race
            var reloaded = new LynxEventManager(tempEventFilePath);
            var (success, message) = reloaded.SplitLif();

            // Assert: a real LIF is in finish order, so compare the lanes as a set
            Assert.True(success, message);
            Assert.Equal(new[] { "5", "6" }, GetSortedLifLanes("039-1-01.lif"));
            Assert.Equal(new[] { "7", "8" }, GetSortedLifLanes("040-1-01.lif"));
            Assert.Equal(new[] { "1", "2" }, GetSortedLifLanes("041-1-01.lif"));
            Assert.Equal(new[] { "3", "4" }, GetSortedLifLanes("042-1-01.lif"));

            // Cleanup
            File.Delete(tempEventFilePath);
            File.Delete(CombineStateFilePathFor(tempEventFilePath));
            foreach (var eventNumber in new[] { "039", "040", "041", "042" })
            {
                File.Delete(Path.Combine(Path.GetTempPath(), $"{eventNumber}-1-01.lif"));
            }
        }

        [Fact]
        public void SplitLif_ReassignedLanes_SurviveAReload()
        {
            // Arrange
            string tempEventFilePath = CopyResourceToTempFile("lynx.evt");
            string tempLifFilePath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");

            var manager = new LynxEventManager(tempEventFilePath) { reassignLanes = true };
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Results come back against the lanes the athletes ran in, which are 1 through 8
            var lifLines = new List<string> { "39,1,1,4x400 Relay Varsity,,,,,,1600,18:47:26.4265" };
            for (int lane = 1; lane <= 8; lane++)
            {
                lifLines.Add($"{lane},,{lane},Milan,,MILA  A,4:0{lane}.000");
            }
            File.WriteAllLines(tempLifFilePath, lifLines);

            // Act
            var reloaded = new LynxEventManager(tempEventFilePath);
            var (success, message) = reloaded.SplitLif();

            // Assert: the seeded lanes are recovered from the saved combine, not from memory
            Assert.True(success, message);
            Assert.Equal(new[] { "5", "6" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "039-1-01.lif")));
            Assert.Equal(new[] { "7", "8" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "040-1-01.lif")));
            Assert.Equal(new[] { "1", "2" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "041-1-01.lif")));
            Assert.Equal(new[] { "3", "4" }, GetLifLanes(Path.Combine(Path.GetTempPath(), "042-1-01.lif")));

            // Cleanup
            File.Delete(tempEventFilePath);
            File.Delete(CombineStateFilePathFor(tempEventFilePath));
            foreach (var eventNumber in new[] { "039", "040", "041", "042" })
            {
                File.Delete(Path.Combine(Path.GetTempPath(), $"{eventNumber}-1-01.lif"));
            }
        }

        [Fact]
        public void SplitLif_MarksTheSavedCombineAsSplit()
        {
            // Arrange
            string tempEventFilePath = CopyResourceToTempFile("lynx.evt");
            string tempLifFilePath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");
            File.Copy(GetResourcePath("039-1-01.lif"), tempLifFilePath, overwrite: true);

            var manager = new LynxEventManager(tempEventFilePath);
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)"
            });
            Assert.False(manager.lastCombine!.splitCompleted);

            // Act
            var (success, message) = manager.SplitLif();

            // Assert: the form uses this to say whether results are still outstanding
            Assert.True(success, message);
            Assert.True(manager.lastCombine!.splitCompleted);
            Assert.True(new LynxEventManager(tempEventFilePath).lastCombine!.splitCompleted);

            // Cleanup
            File.Delete(tempEventFilePath);
            File.Delete(CombineStateFilePathFor(tempEventFilePath));
            foreach (var eventNumber in new[] { "039", "040" })
            {
                File.Delete(Path.Combine(Path.GetTempPath(), $"{eventNumber}-1-01.lif"));
            }
        }

        [Fact]
        public void LoadCombineState_DiscardsACombineForADifferentEventFile()
        {
            // Arrange: combine, then replace the event file with one that has no event 39
            string tempFilePath = CopyResourceToTempFile("lynx.evt");

            var manager = new LynxEventManager(tempFilePath);
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)"
            });

            File.Copy(GetResourcePath("lynx_short_fields.evt"), tempFilePath, overwrite: true);

            // Act
            var reloaded = new LynxEventManager(tempFilePath);

            // Assert: a saved lane mapping for events that are gone cannot route results
            Assert.False(reloaded.hasCombinedData);
            Assert.Contains("no longer", reloaded.lastCombineWarning);
            Assert.False(File.Exists(CombineStateFilePathFor(tempFilePath)));

            // Cleanup
            File.Delete(tempFilePath);
        }

        [Fact]
        public void LoadCombineState_ReportsAStateFileItCannotRead()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");
            string statePath = CombineStateFilePathFor(tempFilePath);
            File.WriteAllText(statePath, "this is not json");

            // Act
            var manager = new LynxEventManager(tempFilePath);

            // Assert: the event file is still perfectly usable, only the combine is lost
            Assert.False(manager.hasCombinedData);
            Assert.Contains("could not be read", manager.lastCombineWarning);
            Assert.Equal(35, manager.events.Count);

            // Cleanup
            File.Delete(tempFilePath);
            File.Delete(statePath);
        }

        [Fact]
        public void ClearCombineState_RemovesTheSavedCombine()
        {
            // Arrange
            string tempFilePath = CopyResourceToTempFile("lynx.evt");

            var manager = new LynxEventManager(tempFilePath);
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)"
            });

            // Act
            manager.ClearCombineState();

            // Assert
            Assert.False(manager.hasCombinedData);
            Assert.False(File.Exists(CombineStateFilePathFor(tempFilePath)));
            Assert.False(new LynxEventManager(tempFilePath).hasCombinedData);

            // The event file keeps the combine that was already written to it
            Assert.Contains("39,1,1,4x400 Relay Varsity", File.ReadAllText(tempFilePath));

            // Cleanup
            File.Delete(tempFilePath);
        }
        #endregion Saved Combine Tests

        #region Helpers
        private static string CombineStateFilePathFor(string eventFilePath)
        {
            return Path.Combine(
                Path.GetDirectoryName(eventFilePath) ?? "",
                Path.GetFileNameWithoutExtension(eventFilePath) + ".combine.json"
            );
        }

        private static string GetResourcePath(string fileName)
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            return Path.Combine(projectDirectory, "Resources", fileName);
        }

        private static string CopyResourceToTempFile(string fileName)
        {
            string tempFilePath = Path.GetTempFileName();
            File.Copy(GetResourcePath(fileName), tempFilePath, overwrite: true);
            return tempFilePath;
        }

        // Lane numbers of the entries belonging to the event whose header starts with eventHeaderPrefix
        private static string[] GetEntryLanes(string eventFilePath, string eventHeaderPrefix)
        {
            var lanes = new List<string>();
            bool inTargetEvent = false;

            foreach (var line in File.ReadAllLines(eventFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Entry lines start with a comma, event headers start with the event number
                if (!line.StartsWith(","))
                {
                    inTargetEvent = line.StartsWith(eventHeaderPrefix);
                }
                else if (inTargetEvent)
                {
                    lanes.Add(line.Split(',')[2]);
                }
            }

            return lanes.ToArray();
        }

        // Lanes of a split LIF file in the temp directory, sorted so finish order does not matter
        private static string[] GetSortedLifLanes(string lifFileName)
        {
            return GetLifLanes(Path.Combine(Path.GetTempPath(), lifFileName)).Order().ToArray();
        }

        private static string[] GetLifLanes(string lifFilePath)
        {
            return File.ReadAllLines(lifFilePath)
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(',')[2])
                .ToArray();
        }
        #endregion Helpers

        #region SplitLif Tests
        [Fact]
        public void SplitLif_SuccessfullySplitsLifFiles()
        {
            // Arrange
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            string eventFilePath = Path.Combine(projectDirectory, "Resources", "lynx.evt");
            string lifFilePath = Path.Combine(projectDirectory, "Resources", "039-1-01.lif");
            string tempEventFilePath = Path.GetTempFileName();
            string tempLifFilePath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");

            File.Copy(eventFilePath, tempEventFilePath, overwrite: true);
            File.Copy(lifFilePath, tempLifFilePath, overwrite: true);

            var manager = new LynxEventManager(tempEventFilePath);

            // Combine events to set up the state for splitting
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
    {
        "Boys 4x400 Relay JV (40,1,1)",
        "Girls 4x400 Relay Varsity (41,1,1)",
        "Girls 4x400 Relay JV (42,1,1)"
    });

            // Act
            var (success, message) = manager.SplitLif();

            // Assert
            Assert.True(success, message);

            // Verify that the LIF files for the main event and combined events were created
            string mainEventLifPath = Path.Combine(Path.GetTempPath(), "039-1-01.lif");
            string combinedEvent1LifPath = Path.Combine(Path.GetTempPath(), "040-1-01.lif");
            string combinedEvent2LifPath = Path.Combine(Path.GetTempPath(), "041-1-01.lif");
            string combinedEvent3LifPath = Path.Combine(Path.GetTempPath(), "042-1-01.lif");

            Assert.True(File.Exists(mainEventLifPath), "Main event LIF file was not created.");
            Assert.True(File.Exists(combinedEvent1LifPath), "Combined event 1 LIF file was not created.");
            Assert.True(File.Exists(combinedEvent2LifPath), "Combined event 2 LIF file was not created.");
            Assert.True(File.Exists(combinedEvent3LifPath), "Combined event 3 LIF file was not created.");

            // Verify that each LIF file has exactly 2 result lines
            Assert.Equal(2, CountResultLines(mainEventLifPath));
            Assert.Equal(2, CountResultLines(combinedEvent1LifPath));
            Assert.Equal(2, CountResultLines(combinedEvent2LifPath));
            Assert.Equal(2, CountResultLines(combinedEvent3LifPath));

            // Cleanup
            File.Delete(tempEventFilePath);
            File.Delete(tempLifFilePath);
            File.Delete(mainEventLifPath);
            File.Delete(combinedEvent1LifPath);
            File.Delete(combinedEvent2LifPath);
            File.Delete(combinedEvent3LifPath);
        }

        // Helper method to count result lines in a LIF file
        private int CountResultLines(string lifFilePath)
        {
            var lines = File.ReadAllLines(lifFilePath);
            // Skip the header line and count non-empty lines
            return lines.Skip(1).Count(line => !string.IsNullOrWhiteSpace(line));
        }

        [Fact]
        public void SplitLif_FailsWhenNoEventsWereCombined()
        {
            // Arrange
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            string eventFilePath = Path.Combine(projectDirectory, "Resources", "lynx.evt");
            string tempEventFilePath = Path.GetTempFileName();

            File.Copy(eventFilePath, tempEventFilePath, overwrite: true);

            var manager = new LynxEventManager(tempEventFilePath);

            // Act
            var (success, message) = manager.SplitLif();

            // Assert
            Assert.False(success, "SplitLif should fail when no events were combined.");
            Assert.Equal("No events were previously combined.", message);

            // Cleanup
            File.Delete(tempEventFilePath);
        }

        [Fact]
        public void SplitLif_FailsWhenLifFileIsMissing()
        {
            // Arrange
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? string.Empty;
            string eventFilePath = Path.Combine(projectDirectory, "Resources", "lynx.evt");
            string tempEventFilePath = Path.GetTempFileName();

            File.Copy(eventFilePath, tempEventFilePath, overwrite: true);

            var manager = new LynxEventManager(tempEventFilePath);

            // Combine events to set up the state for splitting
            manager.CombineEvents("Boys 4x400 Relay Varsity (39,1,1)", new List<string>
            {
                "Boys 4x400 Relay JV (40,1,1)",
                "Girls 4x400 Relay Varsity (41,1,1)",
                "Girls 4x400 Relay JV (42,1,1)"
            });

            // Act
            var (success, message) = manager.SplitLif();

            // Assert
            Assert.False(success, "SplitLif should fail when the LIF file is missing.");
            Assert.Contains("LIF file not found", message);

            // Cleanup
            File.Delete(tempEventFilePath);
        }

        #endregion SplitLif Tests
    }
}