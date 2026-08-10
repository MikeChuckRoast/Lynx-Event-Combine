using System.Reflection;

namespace Lynx_Event_Combine
{
    public partial class Form1 : Form
    {
        private LynxEventManager? eventManager;

        public Form1()
        {
            InitializeComponent();
            Text = $"{Text} v{GetAppVersion()}";
            UpdateCombineStatus();
        }

        /// <summary>
        /// Returns the version from the assembly's informational version (the &lt;Version&gt; in the
        /// project file), with any build metadata such as "+abc123" stripped off.
        /// </summary>
        private static string GetAppVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var informational = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informational))
            {
                var metadataIndex = informational.IndexOf('+');
                return metadataIndex >= 0 ? informational[..metadataIndex] : informational;
            }

            return assembly.GetName().Version?.ToString(3) ?? "unknown";
        }

        private void LoadEventData(string eventFilePath)
        {
            try
            {
                eventManager = new LynxEventManager(eventFilePath);
                ApplyCombineOptions();
                RefreshEventLists();
            }
            catch (Exception ex)
            {
                eventManager = null;
                MessageBox.Show(
                    $"Could not load event file:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            UpdateCombineStatus();
            ShowCombineWarning();
        }

        /// <summary>
        /// Refills the event lists from the manager. Called after a combine as well as after a
        /// load, since combining rewrites the event file and can add an event or change an
        /// event's name.
        /// </summary>
        /// <param name="preserveSelection">
        /// Keeps the current selection, by position rather than by name. Only meaningful after a
        /// combine, which leaves the events in the same order but can rename the main event.
        /// </param>
        private void RefreshEventLists(bool preserveSelection = false)
        {
            var previousMainEventIndex = mainEventComboBox.SelectedIndex;
            var previousSelectedIndices = eventListBox.SelectedIndices.Cast<int>().ToArray();

            mainEventComboBox.BeginUpdate();
            eventListBox.BeginUpdate();
            try
            {
                mainEventComboBox.Items.Clear();
                eventListBox.Items.Clear();

                if (eventManager != null && eventManager.events != null)
                {
                    var names = eventManager.eventNames.ToArray();
                    mainEventComboBox.Items.AddRange(names);
                    eventListBox.Items.AddRange(names);
                }

                if (!preserveSelection)
                {
                    return;
                }

                if (
                    previousMainEventIndex >= 0
                    && previousMainEventIndex < mainEventComboBox.Items.Count
                )
                {
                    mainEventComboBox.SelectedIndex = previousMainEventIndex;
                }
                foreach (var index in previousSelectedIndices)
                {
                    if (index < eventListBox.Items.Count)
                    {
                        eventListBox.SetSelected(index, true);
                    }
                }
            }
            finally
            {
                eventListBox.EndUpdate();
                mainEventComboBox.EndUpdate();
            }
        }

        /// <summary>
        /// Keeps the status line and the buttons that act on a saved combine in step with it.
        /// </summary>
        private void UpdateCombineStatus()
        {
            bool hasCombinedData = eventManager?.hasCombinedData == true;
            splitLifButton.Enabled = hasCombinedData;
            clearCombineButton.Enabled = hasCombinedData;

            if (eventManager == null)
            {
                combineStatusLabel.Text = "No event file loaded.";
            }
            else if (!hasCombinedData)
            {
                combineStatusLabel.Text = "No combine saved for this event file.";
            }
            else
            {
                combineStatusLabel.Text =
                    $"Combined: {eventManager.combineDescription} — "
                    + (
                        eventManager.lastCombine!.splitCompleted
                            ? "results split."
                            : "results not yet split."
                    );
            }
        }

        private void ShowCombineWarning()
        {
            if (string.IsNullOrEmpty(eventManager?.lastCombineWarning))
                return;

            MessageBox.Show(
                eventManager.lastCombineWarning,
                "Saved Combine",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        /// <summary>
        /// Checks before doing something that gives up a combine whose results have not been
        /// split yet, which is the one way left to lose the information a split needs.
        /// </summary>
        private bool ConfirmDiscardCombine(string action)
        {
            if (eventManager?.hasCombinedData != true || eventManager.lastCombine!.splitCompleted)
                return true;

            return MessageBox.Show(
                    $"{eventManager.combineDescription}\r\n\r\n"
                        + $"Those results have not been split yet. {action} will discard the "
                        + "information needed to split them.\r\n\r\nContinue?",
                    "Results Not Yet Split",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                ) == DialogResult.Yes;
        }

        private void ApplyCombineOptions()
        {
            if (eventManager == null)
                return;

            eventManager.removeGenderedEventName = removeGenderCheckBox.Checked;
            eventManager.reassignLanes = reassignLanesCheckBox.Checked;
            eventManager.writeToNewEvent = newEventNumberCheckBox.Checked;
        }

        private void chooseDirButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Event Files (*.evt)|*.evt|All Files (*.*)|*.*";
                openFileDialog.Title = "Select an Event File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    databasePathText.Text = selectedFilePath;
                    LoadEventData(selectedFilePath);
                }
            }
        }

        private void databasePathText_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(databasePathText.Text))
                return;

            LoadEventData(databasePathText.Text);
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(databasePathText.Text))
                return;

            LoadEventData(databasePathText.Text);
        }

        private void combineButton_Click(object sender, EventArgs e)
        {
            if (eventManager == null)
            {
                MessageBox.Show("Please load an event file first.");
                return;
            }
            if (mainEventComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a main event.");
                return;
            }
            if (eventListBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select events to combine.");
                return;
            }

            var mainEvent = mainEventComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(mainEvent))
            {
                MessageBox.Show(
                    "The selected main event is invalid.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            if (!ConfirmDiscardCombine("Combining again"))
            {
                return;
            }

            ApplyCombineOptions();

            var eventsToCombine = eventListBox.SelectedItems.Cast<string>().ToList();
            var ok = eventManager.CombineEvents(mainEvent, eventsToCombine);

            RefreshEventLists(preserveSelection: true);
            UpdateCombineStatus();

            if (ok)
            {
                var message = "Events combined successfully.";
                if (eventManager.lastNewEventNumber.HasValue)
                {
                    message +=
                        $"\r\n\r\nThe combined entries were written to new event {eventManager.lastNewEventNumber.Value}.";
                }
                if (eventManager.reassignLanes)
                {
                    message += "\r\nLane numbers were re-assigned starting at 1.";
                }
                MessageBox.Show(
                    message,
                    "Combine Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    eventManager.reassignLanes
                        ? "There was a duplicate athlete ID after combining. Check the entries in your meet management software."
                        : "There was a duplicate athlete ID or lane number after combining. Check the entries in your meet management software.",
                    "Duplicate ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            if (!string.IsNullOrEmpty(eventManager.lastCombineWarning))
            {
                MessageBox.Show(
                    eventManager.lastCombineWarning,
                    "Schedule File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void splitLifButton_Click(object sender, EventArgs e)
        {
            if (eventManager == null || !eventManager.hasCombinedData)
            {
                return;
            }
            (var success, var message) = eventManager.SplitLif();
            UpdateCombineStatus();
            if (success)
            {
                MessageBox.Show(
                    "Events split successfully.",
                    "Split Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    $"There was an error splitting the events: {message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void clearCombineButton_Click(object sender, EventArgs e)
        {
            if (eventManager == null || !eventManager.hasCombinedData)
            {
                return;
            }
            if (!ConfirmDiscardCombine("Clearing the saved combine"))
            {
                return;
            }

            eventManager.ClearCombineState();
            UpdateCombineStatus();
        }

        private void combineOptionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyCombineOptions();
        }

        #region Speed Buttons
        private void autoConfigureForEvent(string eventName)
        {
            // Find first matching name in mainEventComboBox and select it
            for (int i = 0; i < mainEventComboBox.Items.Count; i++)
            {
                var item = mainEventComboBox.Items[i]?.ToString();
                if (item != null && item.Contains(eventName))
                {
                    mainEventComboBox.SelectedIndex = i;
                    break;
                }
            }
            // Clear all selections in eventListBox
            eventListBox.ClearSelected();

            // Find all matching names in eventListBox and select them (excluding the main event)
            for (int i = 0; i < eventListBox.Items.Count; i++)
            {
                var item = eventListBox.Items[i]?.ToString();
                if (
                    item != null
                    && item.Contains(eventName)
                    && !item.Equals(mainEventComboBox.SelectedItem?.ToString())
                )
                {
                    eventListBox.SetSelected(i, true);
                }
            }
        }

        private void relay100mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("4x100");
        }

        #endregion Speed Buttons

        private void relay200mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("4x200");
        }

        private void relay400mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("4x400");
        }

        private void relay800mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("4x800");
        }

        private void run800mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("800 Meters");
        }

        private void run1600mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("1600 Meters");
        }

        private void run3200mButton_Click(object sender, EventArgs e)
        {
            autoConfigureForEvent("3200 Meters");
        }
    }
}
