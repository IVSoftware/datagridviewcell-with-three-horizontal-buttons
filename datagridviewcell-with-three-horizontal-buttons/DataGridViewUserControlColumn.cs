using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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
            if (DataGridView == null)
            {
            }
            else
            {
                DataGridView.Invalidated += (sender, e) =>refresh();
                DataGridView.Scroll += (sender, e) =>refresh();
                DataGridView.SizeChanged += (sender, e) =>refresh();
            }
            _dataGridView = DataGridView;
        }

        private void refresh()
        {
            foreach (var row in DataGridView.Rows.Cast<DataGridViewRow>().ToArray())
            {
                if(row.Cells[Index] is DataGridViewUserControlCell<T> cell)
                {
                    var cellBounds = DataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                    try
                    {
#if false
                        var record = row.DataBoundItem;
                        var type = record.GetType();
                        var pi = type.GetProperty(Name);
                        var control = (T)pi.GetValue(record);
                        control.Location = cellBounds.Location;
                        control.Size = cellBounds.Size;
#endif
                    }
                    catch (Exception ex)
                    {
                        // Debug.Assert(false, ex.Message);
                    }
                }
            }
        }

        private DataGridView _dataGridView = null;
    }
    public class DataGridViewUserControlCell<T> : DataGridViewCell where T: Control, new()
    {
        public DataGridViewUserControlCell() 
        { }
        public override Type FormattedValueType => typeof(string);

#if false
        public T Control
        {
            get
            {
                if(DataGridView == null)
                {
                    return default(T);
                }
                var row = DataGridView.Rows[RowIndex];
                if (row.IsNewRow)
                {
                    return default(T);
                }
                else
                {
                    var record = row.DataBoundItem;
                    if (record == null)
                    {
                        return default(T);
                    }
                    var name = DataGridView.Columns[ColumnIndex].Name;
                    var pi = record.GetType().GetProperty(name);
                    if (pi == null)
                    {
                        return default(T);
                    }
                    else
                    {
                        var control = (T)pi.GetValue(record);
                        DataGridView.Controls.Add(control);
                        var count = DataGridView.Controls.OfType<ButtonCell3Up>().Count();
                        var distinct = DataGridView.Controls.OfType<ButtonCell3Up>().Distinct().Count();
                        Debug.Assert(count.Equals(distinct), $"Not expecting duplicates");
                        return control;
                    }
                }
            }
        }
#endif
        private DataGridView _dataGridView = null;
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if((DataGridView == null) && (_dataGridView != null))
            {
                //// WILL occur on Swap()
                //_dataGridView.Controls.Remove(Control);
                //var count = _dataGridView.Controls.OfType<ButtonCell3Up>().Count();
                //{ }
                //Control.Dispose();
            }
            else
            {
                //DataGridView.Controls.Add(Control);
                //var count = DataGridView.Controls.OfType<ButtonCell3Up>().Count();
                //var distinct = DataGridView.Controls.OfType<ButtonCell3Up>().Distinct().Count();
                //{ }
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
            try
            {
                var row = DataGridView.Rows[rowIndex];
                var column = DataGridView.Columns[ColumnIndex];
                var record =row.DataBoundItem;
                var type = record.GetType();
                var pi = type.GetProperty(column.Name);
                var control = (T)pi.GetValue(record);
                control.Location = cellBounds.Location;
                control.Size = cellBounds.Size;
                if (control.Parent == null)
                {
                    DataGridView.Controls.Add(control);
                }
                control.Visible = true;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }
    }
}
