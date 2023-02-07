using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView.DataSource = Records;
            dataGridView.RowTemplate.Height = 50;
            dataGridView.MouseDoubleClick += onMouseDoubleClick;  

            #region F O R M A T    C O L U M N S
            Records.Add(new Record()); // <- Auto-configure columns
            dataGridView.Columns[nameof(Record.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[nameof(Record.Modes)].Width = 200;
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Modes)]);
            dataGridView.Columns[nameof(Record.Actions)].Width = 200;
            dataGridView.Columns[nameof(Record.Actions)].DefaultCellStyle.Padding = new Padding(5);
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Actions)]);
            Records.Clear();

            Debug.Assert(
                dataGridView.Controls.OfType<ButtonCell3Up>().Count().Equals(0),
                "Expecting the Clear method to reset the custom controls");
            #endregion F O R M A T    C O L U M N S

            dataGridView.ControlAdded += (sender, e) =>
            {
                var count =
                    dataGridView.Controls.OfType<ButtonCell3Up>().Count();
                { }
            };
            // Add 15 items
            for (int i = 0; i < 5; i++)
            {
                Records.Add(new Record { Description = "Voltage Range" });
                Records.Add(new Record { Description = "Current Range" });
                Records.Add(new Record { Description = "Power Range" });
            }
            for (int i = 1; i <= Records.Count; i++)
                Records[i - 1].Modes.Labels = new[] { $"{i}A", $"{i}B", $"{i}C", }; 
        }
        BindingList<Record> Records { get; } = new BindingList<Record>();
        private void onMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var client = dataGridView.PointToClient(MousePosition);
            var hittest = dataGridView.HitTest(client.X, client.Y);
            if (hittest.Type.Equals(DataGridViewHitTestType.None))
            {
                Records.Clear();
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_COMPOSITED = 0x02000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_COMPOSITED;
                return cp;
            }
        }
    }
    class Record : INotifyPropertyChanged
    {
        public Record()
        {
            Modes.TextChanged += (sender, e) =>
                OnPropertyChanged(nameof(Description));
            Actions.Click += (sender, e) =>
            {
                _ = execTask();
            };
        }

        private async Task execTask()
        {
            Actions.Value = 0;
            while(Actions.Value < Actions.Maximum)
            {
                await Task.Delay(250);
                Actions.Value++;
            }
        }

        private void onModesTextChanged(object sender, EventArgs e) =>
            OnPropertyChanged(nameof(Description));

        string _description = string.Empty;
        public string Description
        {
            get
            {
                return $"{Modes.Text} : {_description}";
            }
            set
            {
                if (!Equals(_description, value))
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        // This can be any type of Control.
        public ButtonCell3Up Modes { get; } = new ButtonCell3Up(); 
        public ProgressBar Actions { get; } = new ProgressBar { Value = 1 }; 

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
