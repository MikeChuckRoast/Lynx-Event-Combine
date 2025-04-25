namespace Lynx_Event_Combine
{
    public partial class Form1 : Form
    {
        private LynxEventManager? eventManager;

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadEventData(string eventFilePath)
        {
            eventManager = new LynxEventManager(eventFilePath);
            eventManager.removeGenderedEventName = removeGenderCheckBox.Checked;
            mainEventComboBox.Items.Clear();
            eventListBox.Items.Clear();

            if (eventManager != null && eventManager.events != null)
            {
                mainEventComboBox.Items.AddRange(eventManager.eventNames.ToArray());
                eventListBox.Items.AddRange(eventManager.eventNames.ToArray());
            }
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
            LoadEventData(databasePathText.Text);
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
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

            var eventsToCombine = eventListBox.SelectedItems.Cast<string>().ToList();
            var ok = eventManager.CombineEvents(mainEvent, eventsToCombine);

            if (ok)
            {
                MessageBox.Show("Events combined successfully.");
            }
            else
            {
                MessageBox.Show(
                    "There was a duplicate athlete ID or lane number after combining. Check the entries in your meet management software.",
                    "Duplicate ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void splitLifButton_Click(object sender, EventArgs e)
        {
            if (eventManager == null || !eventManager.hasCombinedData)
            {
                MessageBox.Show(
                    "You must combine events first.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            (var success, var message) = eventManager.SplitLif();
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

        private void removeGenderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (eventManager != null)
                eventManager.removeGenderedEventName = removeGenderCheckBox.Checked;
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
