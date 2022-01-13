using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using TheLeftExit.Trickster.Memory;

namespace TheLeftExit.Trickster.UI {
    public partial class MainForm : Form {
        #region static void Main()
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        #endregion

        public MainForm() {
            InitializeComponent();
            _ui = new();
            openButtonClick();
        }

        private TricksterUI _ui;

        private void openButtonClick(object sender = null, EventArgs e = null) {
            statusLabel.Text = _ui.OpenProcess(openTextBox.Text);

            openTextBox.Text = _ui.Process?.ProcessName ?? string.Empty;
            openTextBox.Enabled = _ui.Trickster is null;
            openButton.Text = _ui.Trickster is null ? "Open" : "Close";
            getTypesButton.Enabled = _ui.Trickster is not null;
            getTypesComboBox.Enabled = false;
            getTypesComboBox.Items.Clear();
            readButton.Enabled = _ui.Trickster is not null;
            scanButton.Enabled = false;
            this.scanListBox.Items.Clear();
        }

        private void getTypesButtonClick(object sender, EventArgs e) {
            statusLabel.Text = "Scanning types...";
            Update();
            var result = _ui.GetTypes();
            statusLabel.Text = result.Item1;
            foreach (var t in result.Item2) getTypesComboBox.Items.Add(t);
            getTypesComboBox.Enabled = true;
            statusLabel.Text = $"Types found: {result.Item2.Length}";

            if (_ui.Trickster.Regions is not null)
                scanButton.Enabled = true;
        }

        private void readButton_Click(object sender, EventArgs e) {
            statusLabel.Text = $"Reading process memory...";
            Update();
            statusLabel.Text = _ui.Read();
            if (_ui.Trickster.ScannedTypes is not null)
                scanButton.Enabled = true;
        }

        private void scanButton_Click(object sender, EventArgs e) {
            statusLabel.Text = "Scanning memory for type references...";
            Update();
            string[] result = _ui.Scan((TypeInfo)getTypesComboBox.SelectedItem);
            statusLabel.Text = $"Structures found: {result.Length}";
            scanListBox.Items.Clear();
            foreach (string address in result)
                scanListBox.Items.Add(address);
            scanListBox.Enabled = true;
        }

        private void scanListBoxMouseDown(object sender, MouseEventArgs e) {
            string item = scanListBox.SelectedItem.ToString();
            if (item != null && e.Button.HasFlag(MouseButtons.Right)) {
                Clipboard.SetText(item);
                statusLabel.Text = $"Address {item} copied to clipboard!";
            }
        }

        private void openTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData == Keys.Enter)
                openButton.PerformClick();
        }
    }
}
