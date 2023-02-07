using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
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
        // Keep track of controls added by this instance
        // so that they can be removed by this instance.
        private readonly List<Control> _controls = new List<Control>();
        internal void Add(Control control)
        {
            _controls.Add(control);
            DataGridView.Controls.Add(control);
        }
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if ((DataGridView == null) && (_dataGridView != null))
            {
                foreach (var control in _controls)
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

    public class DataGridViewUserControlCell : DataGridViewCell
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
                    var col = DataGridView.Columns[ColumnIndex];
                    ((DataGridViewUserControlColumn)col).Add(control);
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
                    control = (Control)pi.GetValue(record);
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
