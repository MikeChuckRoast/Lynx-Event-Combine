using Lynx_Event_Combine;

namespace LynxEventCombineTest
{
    public class LynxEventManagerTests
    {
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