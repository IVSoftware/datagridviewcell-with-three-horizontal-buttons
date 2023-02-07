using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
            DataGridViewUserControlColumn<ButtonCell3Up>
                .Swap(dataGridView.Columns[nameof(Record.Modes)]);
            Records.Clear();

            Debug.Assert(
                dataGridView.Controls.OfType<ButtonCell3Up>().Count().Equals(0),
                "Expecting the Clear method to reset the custom controls");
            #endregion F O R M A T    C O L U M N S

#if true
            dataGridView.ControlAdded += (sender, e) =>
            {
                var count =
                    dataGridView.Controls.OfType<ButtonCell3Up>().Count();
                { }
            };
            // Add a few items
            for (int i = 0; i < 1; i++)
            {
                Records.Add(new Record { Description = "Voltage Range" });
                // AssertCount(dataGridView, 1);
                Records.Add(new Record { Description = "Current Range" });
                // AssertCount(dataGridView, 2);
                Records.Add(new Record { Description = "Power Range" });
                // AssertCount(dataGridView, 3);
            }
#else
            Records.Add(new Record { Description = "Voltage Range" });  
#endif
            { }
            for (int i = 1; i <= Records.Count; i++)
                Records[i - 1].Modes.Labels = new[] { $"{i}A", $"{i}B", $"{i}C", }; 
        }
        public static void AssertCount(DataGridView dataGridView, int expected)
        {
            var actual =
                dataGridView.Controls.OfType<ButtonCell3Up>().Count();
            Debug.Assert(expected.Equals(actual),
                $"Expected {expected}  Actual {actual}");
        }
        BindingList<Record> Records { get; } = new BindingList<Record>();
        private void onMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var b4 = dataGridView.Controls.OfType<ButtonCell3Up>().Count();
            Records.Clear();
            BeginInvoke((MethodInvoker)delegate
            {
                var ftr = dataGridView.Controls.OfType<ButtonCell3Up>().Count();
            });
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
        string _currentButton;
        string _description = string.Empty;
        public string Description
        {
            get
            {
                if(string.IsNullOrEmpty(_description))
                {
                    return _description;
                }
                else
                {
                    return $"{_description} - {_currentButton}";
                }
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
        public ButtonCell3Up Modes { get; } = new ButtonCell3Up { Visible = false }; 

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
