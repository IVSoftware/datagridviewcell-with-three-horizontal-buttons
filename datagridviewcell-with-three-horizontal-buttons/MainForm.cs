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
            dataGridView.CellPainting += onCellPainting;
            Records.ListChanged += onRecordsChanged;
            dataGridView.MouseDoubleClick += onMouseDoubleClick;

            #region F O R M A T    C O L U M N S
            Records.Add(new Record()); // <- Auto-configure columns
            dataGridView.Columns[nameof(Record.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[nameof(Record.Control)].Width = 200;
            DataGridViewUserControlColumn<ButtonCell3Up>
                .Swap(dataGridView.Columns[nameof(Record.Control)]);
            Records.Clear();
            #endregion F O R M A T    C O L U M N S

            // Add a few items
            for (int i = 0; i < 5; i++)
            {
                Records.Add(new Record { Description = "Voltage Range" });
                Records.Add(new Record { Description = "Current Range" });
                Records.Add(new Record { Description = "Power Range" });
            }
            for (int i = 1; i <= Records.Count; i++)
                Records[i - 1].Control.Labels = new[] { $"{i}A", $"{i}B", $"{i}C", }; 
        }
        BindingList<Record> Records { get; } = new BindingList<Record>();
        private void onRecordsChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                case ListChangedType.ItemDeleted:
                    var controlsB4 =
                        dataGridView.Controls.OfType<ButtonCell3Up>().ToArray();
                    if (controlsB4.Length != 0)
                    {
                        foreach (var buttonCell3Up in controlsB4)
                        {
                            if (!Records.Any(_ => _.Control.Equals(buttonCell3Up)))
                            {
                                buttonCell3Up.Dispose();
                                dataGridView.Controls.Remove(buttonCell3Up);
                            }
                        }
                    }
                    break;
            }
        }
        private void onMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var b4 = dataGridView.Controls.OfType<ButtonCell3Up>().Count();
            Records.Clear();
            BeginInvoke((MethodInvoker)delegate
            {
                var ftr = dataGridView.Controls.OfType<ButtonCell3Up>().Count();
            });
        }
        private void onCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is DataGridView dataGridView)
            {
                if (
                        (e.RowIndex != -1) && 
                        (e.RowIndex < dataGridView.Rows.Count)
                    )
                {
                    if (!dataGridView.Rows[e.RowIndex].IsNewRow)
                    {
                        hideIfLocationChanged();
                        if (e.ColumnIndex.Equals(dataGridView.Columns[nameof(Record.Control)].Index))
                        {
                            var record = Records[e.RowIndex];
                            if (record.Control.Parent == null)
                            {
                                dataGridView.Controls.Add(record.Control);
                            }
                            record.Control.Location = e.CellBounds.Location;
                            record.Control.Size = e.CellBounds.Size;
                            record.Control.Visible = true;
                        }
                    }
                }
            }
            void hideIfLocationChanged()
            {
                var cIndex = dataGridView.Columns[nameof(Record.Control)].Index;
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    if (!dataGridView.Rows[i].IsNewRow)
                    {
                        var control = Records[i].Control;
                        var sbLoc = dataGridView.GetCellDisplayRectangle(cIndex, i, true).Location;
                        if (control.Location != sbLoc)
                        {
                            control.Visible = false;
                        }
                    }
                }
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
            _currentButton = Control.Text;
            Control.ButtonChanged += onButtonChanged;
        }
        private void onButtonChanged(object sender, EventArgs e)
        {
            _currentButton = Control.Text;
            OnPropertyChanged(nameof(Description));
        }
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
        [DisplayName("Modes")]
        public ButtonCell3Up Control { get; } = new ButtonCell3Up(); 

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
