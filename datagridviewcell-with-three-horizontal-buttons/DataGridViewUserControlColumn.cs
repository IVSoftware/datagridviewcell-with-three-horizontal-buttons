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
#if DEBUG
            var count = DataGridView.Controls.OfType<ButtonCell3Up>().Count();
            if (Form.ActiveForm != null)
            {
                Form.ActiveForm.Text = $"[{count}] {nameof(ButtonCell3Up)} Controls";
            }
#endif
        }
        internal void RemoveUC(Control control)
        {
            _controls.Remove(control);
            if (_dataGridView != null)
            {
                _dataGridView.Controls.Remove(control);
#if DEBUG
                var count = _dataGridView.Controls.OfType<ButtonCell3Up>().Count();
                if (Form.ActiveForm != null)
                {
                    Form.ActiveForm.Text = $"[{count}] {nameof(ButtonCell3Up)} Controls";
                }
#endif
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
}
