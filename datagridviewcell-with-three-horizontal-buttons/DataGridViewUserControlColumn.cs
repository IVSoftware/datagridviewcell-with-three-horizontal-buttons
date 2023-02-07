using System;
using System.Collections.Generic;
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
            foreach (DataGridViewRow row in DataGridView.Rows)
            {
                if(row.Cells[Index] is DataGridViewUserControlCell<T> cell)
                {
                    var cellBounds = DataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                    cell.Control.Location = cellBounds.Location;
                    cell.Control.Size = cellBounds.Size;
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
        public T Control { get; } = new T() { Visible = false };

        private DataGridView _dataGridView = null;
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if((DataGridView == null) && (_dataGridView != null))
            {
                // WILL occur on Swap()
                _dataGridView.Controls.Remove(Control);
                var count = _dataGridView.Controls.OfType<ButtonCell3Up>().Count();
                { }
                Control.Dispose();
            }
            else
            {
                DataGridView.Controls.Add(Control);
                var count = DataGridView.Controls.OfType<ButtonCell3Up>().Count();
                var distinct = DataGridView.Controls.OfType<ButtonCell3Up>().Distinct().Count();
                { }
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
            Control.Location = cellBounds.Location;
            Control.Size = cellBounds.Size;
        }
    }
}
