Sometimes there's a solid use case for something like "three buttons" or (in my use case) where each row has its own rolling chart of data. So here's a way to show a UserControl in a cell that I've used a lot and has worked for me.  Basically, the class that's bound to the DataGridView has a member that is the UserControl. The _first_ time the cell gets painted, the user control associated with the row is added to the Controls collection of the DGV and _every_ time the cell is painted the `Location` and `Size` of the control are verified to be at the cell bounds. As far as disposing the controls in a dynamic scenario, when the `ListChanged` event of the binding list fires there's a cleanup that removes any user controls that don't have a corresponding row item.

![Screenshot]

***
**Record class**

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

***
**Configure DGV**

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
            Records.Clear();
            #endregion F O R M A T    C O L U M N S

            // Add a few items
            Records.Add(new Record { Description = "Voltage Range"});
            Records.Add(new Record { Description = "Current Range"});
            Records.Add(new Record { Description = "Power Range"});
        }
        BindingList<Record> Records { get; } = new BindingList<Record>();
        .
        .
        .
    }

***
**Paint cell**

    private void onCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
        if (sender is DataGridView dataGridView)
        {
            if (
                    (e.RowIndex != -1) && 
                    (e.RowIndex < dataGridView.Rows.Count) &&
                    e.ColumnIndex.Equals(dataGridView.Columns[nameof(Record.Control)].Index)
                )
            {  
                // Don't assign a control in the "extra" row that's
                // present when "AllowUserToAddRows" is enabled.
                if (!dataGridView.Rows[e.RowIndex].IsNewRow) // <- check this
                {
                    var record = Records[e.RowIndex];
                    if(record.Control.Parent == null)
                    {
                        dataGridView.Controls.Add(record.Control);
                    }
                    record.Control.Location = e.CellBounds.Location;
                    record.Control.Size = e.CellBounds.Size;
                }
            }
        }
    }

 ***
 **Disposal**

 _This isn't the most efficient way but should be fine for infrequent changes to a few dozen controls.

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