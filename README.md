I agree that sometimes there's a solid use case for something like "three buttons" or (in my use case) where each row has its own rolling `Chart` of real time data. Something that works for me is having a custom `DataGridViewUserControlColumn` class as coded below.  

The theory of operation is to allow the bound data class to have properties that derive from `UserControl`. The auto-generated Column in the DGV corresponding to can be swapped out. Then,  when a `DataGridViewUserControlCell` gets "painted" instead of drawing the cell what happens instead is that the control is moved (if necessary) so that its bounds coincide with the cell bounds being drawn. Since the user control is in the DataGridView.Controls collection, the UC stays on top in the z-order and paints the same as any child of any container would.

The UserControl is added to the `DataGridView.Controls` collection the first time it's drawn and removed when the cell's `DataGridView` property is set to null, such as when a row is deleted. When the `AllowUserToAddRows` options is enabled, a new row will _not_ show a control until the item editing is complete.

[![screenshot][1]][1]

***
**Typical Record class**

    class Record : INotifyPropertyChanged
    {
        public Record()
        {
            Modes.TextChanged += (sender, e) =>
                OnPropertyChanged(nameof(Description));
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
        public ButtonCell3Up Modes { get; } = new ButtonCell3Up { Visible = false }; 

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
            dataGridView.MouseDoubleClick += onMouseDoubleClick;  

            #region F O R M A T    C O L U M N S
            Records.Add(new Record()); // <- Auto-configure columns
            dataGridView.Columns[nameof(Record.Description)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView.Columns[nameof(Record.Modes)].Width = 200;
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Modes)]);
            Records.Clear();
            #endregion F O R M A T    C O L U M N S

            
            // FOR DEMO PURPOSES: Add some items.
            for (int i = 0; i < 5; i++)
            {
                Records.Add(new Record { Description = "Voltage Range" });
                Records.Add(new Record { Description = "Current Range" });
                Records.Add(new Record { Description = "Power Range" });
            }
            for (int i = 1; i <= Records.Count; i++)
                Records[i - 1].Modes.Labels = new[] { $"{i}A", $"{i}B", $"{i}C", }; 
    }

***
**Custom Cell with Paint override**

    public class DataGridViewUserControlCell : DataGridViewCell
    {
        private Control _control = null;
        private DataGridViewUserControlColumn _column;
        public override Type FormattedValueType => typeof(string);
        private DataGridView _dataGridView = null;
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if((DataGridView == null) && (_dataGridView != null))
            {
                // WILL occur on Swap() and when a row is deleted.
                if (TryGetControl(out var control))
                {
                    _column.RemoveUC(control);
                }
            }
            _dataGridView = DataGridView;
        }
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            if (DataGridView.Rows[rowIndex].IsNewRow)
            {
                graphics.FillRectangle(Brushes.Azure, cellBounds);
            }
            else
            {
                if (TryGetControl(out var control))
                {
                    control.Location = cellBounds.Location;
                    control.Size = cellBounds.Size;
                    control.Visible = true;
                }
            }
        }

        public bool TryGetControl(out Control control)
        {
            control = null;
            if (_control == null)
            {
                try
                {
                    if ((RowIndex != -1) && (RowIndex < DataGridView.Rows.Count))
                    {
                        var row = DataGridView.Rows[RowIndex];
                        _column = (DataGridViewUserControlColumn)DataGridView.Columns[ColumnIndex];
                        var record = row.DataBoundItem;
                        var type = record.GetType();
                        var pi = type.GetProperty(_column.Name);
                        control = (Control)pi.GetValue(record);
                        if (control.Parent == null)
                        {
                            DataGridView.Controls.Add(control);
                            _column.AddUC(control);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
                _control = control;
            }
            else
            {
                control = _control;
            }
            return _control != null;
        }
    }

 ***
 **Custom Column**

 
    public class DataGridViewUserControlColumn : DataGridViewColumn
    {
        public DataGridViewUserControlColumn() => CellTemplate = new DataGridViewUserControlCell();
        public static void Swap(DataGridViewColumn old)
        {
            var dataGridView = old.DataGridView;
            var indexB4 = old.Index;
            dataGridView.Columns.RemoveAt(indexB4);
            dataGridView.Columns.Insert(indexB4, new DataGridViewUserControlColumn
            {
                Name = old.Name,
                AutoSizeMode = old.AutoSizeMode,
                Width = old.Width,
            });
        }
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if ((DataGridView == null) && (_dataGridView != null))
            {
                _dataGridView.Invalidated -= (sender, e) => refresh();
                _dataGridView.Scroll -= (sender, e) => refresh();
                _dataGridView.SizeChanged -= (sender, e) => refresh();
                foreach (var control in _controls.ToArray())
                {
                    RemoveUC(control);
                }

            }
            else
            {
                DataGridView.Invalidated += (sender, e) =>refresh();
                DataGridView.Scroll += (sender, e) =>refresh();
                DataGridView.SizeChanged += (sender, e) =>refresh();
            }
            _dataGridView = DataGridView;
        }
        // Keep track of controls added by this instance
        // so that they can be removed by this instance.
        private readonly List<Control> _controls = new List<Control>();
        internal void AddUC(Control control)
        {
            _controls.Add(control);
            DataGridView.Controls.Add(control);
        }
        internal void RemoveUC(Control control)
        {
            _controls.Remove(control);
            if (_dataGridView != null)
            {
            }
        }

        int _wdtCount = 0;
        private void refresh()
        {
            var capture = ++_wdtCount;
            // Allow changes to settle.
            Task
                .Delay(TimeSpan.FromMilliseconds(10))
                .GetAwaiter()
                .OnCompleted(() => 
                {
                    foreach (var row in DataGridView.Rows.Cast<DataGridViewRow>().ToArray())
                    {
                        if (row.Cells[Index] is DataGridViewUserControlCell cell)
                        {
                            if (row.IsNewRow)
                            {   /* G T K */
                            }
                            else
                            {
                                var cellBounds = DataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                                if (cell.TryGetControl(out var control))
                                {
                                    control.Location = cellBounds.Location;
                                    control.Size = cellBounds.Size;
                                    control.Visible = !row.IsNewRow;
                                }
                            }
                        }
                    }
                });
        }
        private DataGridView _dataGridView = null;
    }


  [1]: https://i.stack.imgur.com/HqaW5.png