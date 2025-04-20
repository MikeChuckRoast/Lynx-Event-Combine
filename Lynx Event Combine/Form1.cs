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
                MessageBox.Show("The selected main event is invalid.");
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
                    "There was a duplicate athlete ID or lane number after combining. Check the entries in your meet management software."
                );
            }
        }

        private void splitLifButton_Click(object sender, EventArgs e)
        {
            if (eventManager == null || !eventManager.hasCombinedData)
            {
                MessageBox.Show("You must combine events first.");
                return;
            }
            (var success, var message) = eventManager.SplitLif();
            if (success)
            {
                MessageBox.Show("Events split successfully.");
            }
            else
            {
                MessageBox.Show($"There was an error splitting the events: {message}");
            }
        }
    }
}
