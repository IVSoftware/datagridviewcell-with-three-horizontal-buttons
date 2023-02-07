using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
    public class DataGridViewUserControlColumn<T> : DataGridViewColumn where T: Control, new()
    {
        public DataGridViewUserControlColumn()
        {
            CellTemplate = new DataGridViewUserControlCell<T>();
        }
        public static void Swap(DataGridViewColumn old)
        {
            var dataGridView = old.DataGridView;
            var indexB4 = old.Index;
            dataGridView.Columns.RemoveAt(indexB4);
            dataGridView.Columns.Insert(indexB4, new DataGridViewUserControlColumn<T>
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
                var controls = _dataGridView.Controls.OfType<T>().ToArray();
                foreach (var control in controls)
                {
                    _dataGridView.Controls.Remove(control);
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

        int _wdtCount = 0;
        private void refresh()
        {
            var capture = ++_wdtCount;
            Task
                .Delay(TimeSpan.FromMilliseconds(10))
                .GetAwaiter()
                .OnCompleted(() => 
                {
                    foreach (var row in DataGridView.Rows.Cast<DataGridViewRow>().ToArray())
                    {
                        if (row.Cells[Index] is DataGridViewUserControlCell<T> cell)
                        {
                            var cellBounds = DataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                            if (cell.TryGetControl(out var control))
                            {
                                control.Location = cellBounds.Location;
                                control.Size = cellBounds.Size;
                            }
                        }
                    }
                });
        }
        private DataGridView _dataGridView = null;
    }

    public class DataGridViewUserControlCell<T> : DataGridViewCell where T: Control, new()
    {
        public DataGridViewUserControlCell() 
        { }
        public override Type FormattedValueType => typeof(string);

        private DataGridView _dataGridView = null;
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if((DataGridView == null) && (_dataGridView != null))
            {
                // WILL occur on Swap()
                if (TryGetControl(out var control))
                {
                    _dataGridView.Controls.Remove(control);
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
            if (TryGetControl(out var control))
            {
                control.Location = cellBounds.Location;
                control.Size = cellBounds.Size;
                if (control.Parent == null)
                {
                    DataGridView.Controls.Add(control);
                    var count = DataGridView.Controls.OfType<ButtonCell3Up>().Count();
                    var distinct = DataGridView.Controls.OfType<ButtonCell3Up>().Distinct().Count();
                    { }
                }
                control.Visible = true;
            }
        }

        public bool TryGetControl(out Control control)
        {
            try
            {
                if ((RowIndex != -1) && (RowIndex < DataGridView.Rows.Count))
                {
                    var row = DataGridView.Rows[RowIndex];
                    var column = DataGridView.Columns[ColumnIndex];
                    var record = row.DataBoundItem;
                    var type = record.GetType();
                    var pi = type.GetProperty(column.Name);
                    control = (T)pi.GetValue(record);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            control = null;
            return false;
        }
    }
}
