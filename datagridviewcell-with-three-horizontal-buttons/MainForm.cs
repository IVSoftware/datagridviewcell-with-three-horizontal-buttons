﻿using System;
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
            Point clientTopLeft = PointToScreen(ClientRectangle.Location);
            NCOffset = new Point(
                clientTopLeft.X - Location.X,
                clientTopLeft.Y - Location.Y);
            Debug.WriteLine($"NC Rect: {NCOffset}");

            // Add 15 items
            for (int i = 0; i < 5; i++)
            {
                Records.Add(new Record { Description = "Voltage Range" });
                Records.Add(new Record { Description = "Current Range" });
                Records.Add(new Record { Description = "Power Range" });
            }
            for (int i = 1; i <= Records.Count; i++)
                Records[i - 1].Modes.Labels = new[] { $"{i}A", $"{i}B", $"{i}C", };

            dataGridView.RowTemplate.Height = 50;
            dataGridView.DataSource = Records;
            dataGridView.MouseDoubleClick += onMouseDoubleClick;  

            #region F O R M A T    C O L U M N S
            dataGridView.Columns[nameof(Record.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[nameof(Record.Modes)].Width = 200;
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Modes)]);
            dataGridView.Columns[nameof(Record.Actions)].Width = 200;
            dataGridView.Columns[nameof(Record.Actions)].DefaultCellStyle.Padding = new Padding(5);
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Actions)]);

            Debug.Assert(
                dataGridView.Controls.OfType<ButtonCell3Up>().Count().Equals(0),
                "Expecting the Clear method to reset the custom controls");
            #endregion F O R M A T    C O L U M N S

            Record.TooltipRequired += (sender, e) =>
            {
                if (sender is Record record)
                {
                    if (dataGridView
                            .Rows
                            .OfType<DataGridViewRow>()
                            .FirstOrDefault(_ => Equals(_.DataBoundItem, sender))
                            is
                            DataGridViewRow row)
                    {
                        var cellRectangle =
                            dataGridView
                            .GetCellDisplayRectangle(row.Cells["Description"]
                            .ColumnIndex,
                            row.Index, true);

                        descriptionToolTip.Show(
                            $"New value {record.Modes.Text}",
                            this,
                            cellRectangle.X + NCOffset.X + 40,
                            cellRectangle.Y + NCOffset.Y + 10,
                            1000);
                    }
                }
            };
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

        // Make app startup drawing more even.
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
        private ToolTip descriptionToolTip = new ToolTip
        {
            IsBalloon = false, 
            AutoPopDelay = 10, 
            InitialDelay = 10, 
            ReshowDelay = 200
        };
        private Point NCOffset { get; set; }
    }
    class Record : INotifyPropertyChanged
    {
        public Record()
        {
            Modes.TextChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(Description));
            };
            Modes.SelectionChanged += (sender, e) =>
            {
                _ = execTask(); 
                TooltipRequired?.Invoke(this, EventArgs.Empty);
            };
            Actions.Click += (sender, e) =>
            { 
                _ = execTask(); 
            };
        }

        public string Description
        {
            get => $"{Modes.Text} : {_description}";
            set
            {
                if (!Equals(_description, value))
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        string _description = string.Empty;
        #region B O U N D    C O N T R O L S    o f    A N Y    T Y P E   
        public ButtonCell3Up Modes { get; } = new ButtonCell3Up();
        public ProgressBar Actions { get; } = new ProgressBar { Value = 1 };  
        #endregion B O U N D    C O N T R O L S    o f    A N Y    T Y P E   

        private async Task execTask()
        {
            Actions.Value = Actions.Maximum / 10;
            while(Actions.Value < Actions.Maximum)
            {
                await Task.Delay(50);
                Actions.Value++;
            }
        }
        private void onModesTextChanged(object sender, EventArgs e) =>
            OnPropertyChanged(nameof(Description));

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static event EventHandler TooltipRequired;
    }
    static partial class Extensions
    {
        public static ButtonCell3Up WithSelectionChangedHandler(this ButtonCell3Up button, EventHandler handler)
        {
            button.SelectionChanged += handler;
            return button;
        }
    }
}
