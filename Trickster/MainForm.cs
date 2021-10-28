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

using static TheLeftExit.Memory.ObjectModel.ObjectModelExtensions;

namespace TheLeftExit.Trickster {
    public partial class MainForm : Form {
        #region static void Main()
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        #endregion

        public MainForm() {
            InitializeComponent();
            OnProcessOpened();
        }

        private TypeScanner scanner;
        private ulong? value;

        private void OnProcessOpened(string procName = null) {
            bool opened = procName is not null;
            openButton.Text = opened ? "Close" : "Open...";
            openTextBox.Enabled = !opened;
            openTextBox.Text = procName;
            statusLabel.Text = opened ? $"Process: {procName} [{scanner.Process.Id}]" : "Type in the process ID or part of its name.";
            getTypesButton.Enabled = opened;
            getTypesComboBox.Enabled = false;
            getTypesComboBox.Items.Clear();
            value = null;
            readButton.Enabled = opened;
            scanButton.Enabled = false;
            scanListBox.Enabled = false;
            scanListBox.Items.Clear();
        }

        private void openButtonClick(object sender, EventArgs e) {
            if(scanner == null) {
                if (int.TryParse(openTextBox.Text, out int processId)) {
                    try {
                        Process processById = Process.GetProcessById(processId);
                        scanner = new TypeScanner(processById);
                        OnProcessOpened(processById.ProcessName);
                        return;
                    } catch (ArgumentException) { }
                }
                Process[] result = Process.GetProcesses().Where(x => x.ProcessName.ToLower().Contains(openTextBox.Text.ToLower())).ToArray();
                switch (result.Length) {
                    case 0:
                        statusLabel.Text = "No processes found matching this ID or name.";
                        return;
                    case 1:
                        scanner = new TypeScanner(result[0]);
                        OnProcessOpened(result[0].ProcessName);
                        return;
                    default:
                        statusLabel.Text = "More than one process found matching this name.";
                        return;
                }
            } else {
                scanner?.Dispose();
                scanner = null;
                OnProcessOpened();
            }
        }

        private void getTypesButtonClick(object sender, EventArgs e) {
            statusLabel.Text = "Scanning for type information..."; Update();
            scanner.InitTypes();
            getTypesComboBox.Items.AddRange(scanner.Types.Select(x => $"{x.Names[0]} - {x.Offset:X}").ToArray());
            getTypesComboBox.Enabled = true;
            statusLabel.Text = $"Types found: {scanner.Types.Length}";
        }

        private void getTypesComboBoxSelectedIndexChanged(object sender, EventArgs e) {
            if (!getTypesComboBox.Enabled) return;
            value = ApplyOffset(scanner.MainModuleBaseAddress, scanner.Types[getTypesComboBox.SelectedIndex].Offset);
            if (scanner.Regions != null)
                scanButton.Enabled = true;
        }

        private void readButton_Click(object sender, EventArgs e) {
            statusLabel.Text = $"Reading process memory..."; Update();
            scanner.InitRegions();
            scanner.ReadRegions();
            statusLabel.Text = $"Regions read: {scanner.Regions.Where(x => x != null).Count()}";
            if (value != null)
                scanButton.Enabled = true;
        }

        private void scanButton_Click(object sender, EventArgs e) {
            statusLabel.Text = "Scanning..."; Update();
            ulong[] result = scanner.ScanRegions(value.Value);
            statusLabel.Text = $"Structures found: {result.Length}";
            scanListBox.Items.Clear();
            foreach (ulong address in result)
                scanListBox.Items.Add(address.ToString("X"));
            scanListBox.Enabled = true;
        }

        private void scanListBoxMouseDown(object sender, MouseEventArgs e) {
            string item = scanListBox.SelectedItem as string;
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
