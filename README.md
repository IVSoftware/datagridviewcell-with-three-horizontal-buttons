I agree that sometimes there's a solid use case to display a `UserControl` whether it's "three buttons" or each row having its own rolling `Chart` of real time data or whatever! One approach that long-term has worked for me, tried and true, is having a  `DataGridViewUserControlColumn` class similar to the one coded below that can host a control in the cell bounds instead of just drawing one.  

The theory of operation is to allow the bound data class to have properties that derive from `UserControl`. The auto-generated Column in the DGV corresponding to can be swapped out. Then,  when a `DataGridViewUserControlCell` gets "painted" instead of drawing the cell what happens instead is that the control is moved (if necessary) so that its bounds coincide with the cell bounds being drawn. Since the user control is in the DataGridView.Controls collection, the UC stays on top in the z-order and paints the same as any child of any container would.

[![screenshot][1]][1]

The item's UserControl is added to the `DataGridView.Controls` collection the first time it's drawn and removed when the cell's `DataGridView` property is set to null (e.g. when user deletes a row). When the `AllowUserToAddRows` options is enabled, the "new row" list item doesn't show a control until the item editing is complete.


***
**Typical Record class**

    class Record : INotifyPropertyChanged
    {
        public Record()
        {
            Modes.TextChanged += (sender, e) =>
                OnPropertyChanged(nameof(Description));
            Actions.Click += (sender, e) =>
                { _ = execTask(); };
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
            Actions.Value = 0;
            while(Actions.Value < Actions.Maximum)
            {
                await Task.Delay(250);
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
            dataGridView.Columns[nameof(Record.Actions)].Width = 200;
            dataGridView.Columns[nameof(Record.Actions)].DefaultCellStyle.Padding = new Padding(5);
            DataGridViewUserControlColumn.Swap(dataGridView.Columns[nameof(Record.Actions)]);
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
            using (var brush = new SolidBrush(getBackColor(@default: Color.Azure)))
            {
                graphics.FillRectangle(brush, cellBounds);
            }
            if (DataGridView.Rows[rowIndex].IsNewRow)
            {   /* G T K */
            }
            else
            {
                if (TryGetControl(out var control))
                {
                    SetLocationAndSize(cellBounds, control);
                }
            }
            Color getBackColor(Color @default)
            {
                if((_column != null) && (_column.DefaultCellStyle != null))
                {
                    Style = _column.DefaultCellStyle;
                }
                return Style.BackColor.A == 0 ? @default : Style.BackColor;
            }
        }
        public void SetLocationAndSize(Rectangle cellBounds, Control control, bool visible = true)
        {
            control.Location = new Point(
                cellBounds.Location.X +
                Style.Padding.Left,
                cellBounds.Location.Y + Style.Padding.Top);
            control.Size = new Size(
                cellBounds.Size.Width - (Style.Padding.Left + Style.Padding.Right),
                cellBounds.Height - (Style.Padding.Top + Style.Padding.Bottom));
            control.Visible = visible;
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
                catch (Exception ex) {
                    Debug.Assert(false, ex.Message);
                }
                _control = control;
            }
            else control = _control;
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
                DefaultCellStyle = old.DefaultCellStyle,
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
                DataGridView.Parent.SizeChanged += (sender, e) =>
                {
                   // refresh();
                };
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
                _dataGridView.Controls.Remove(control);
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
                    if (DataGridView != null)
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
                                        cell.SetLocationAndSize(cellBounds, control, visible: !row.IsNewRow);
                                    }
                                }
                            }
                        }
                    }
                });
        }
        private DataGridView _dataGridView = null;
    }


  [1]: https://i.stack.imgur.com/nXsE1.png