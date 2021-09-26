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

        private Process process;
        private MemoryScanner scanner;
        private ushort value;

        public MainForm() {
            InitializeComponent();
            process = new Process((uint)System.Diagnostics.Process.GetProcessesByName("Growtopia").Single().Id);
            scanner = new MemoryScanner(process);
        }

        private unsafe bool IsValue(Span<byte> buffer) =>
            Unsafe.Read<ushort>(Unsafe.AsPointer(ref buffer.GetPinnableReference())) == value;

        private void button1_Click(object sender, EventArgs e) {
            if(!ushort.TryParse(textBox1.Text, out value)) {
                MessageBox.Show("Invalid value.");
                return;
            }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            scanner.Scan(sizeof(ushort), IsValue);
            sw.Stop();

            int resultCount = scanner.GetAddresses().Count();

            label2.Text = $"Scan time: {sw.ElapsedMilliseconds} ms" + Environment.NewLine +
                $"Results found: {resultCount}";

            if (resultCount < 50) {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(scanner.GetAddresses().Take(Math.Min(resultCount, 50)).Select(x => x.ToString("X")).ToArray());
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            scanner.Reset();
            label2.Text = "Results reset!";
        }
    }
}
