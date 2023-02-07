using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
    public class DataGridViewUserControlColumn<T> : DataGridViewColumn where T: Control
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
    }
    public class DataGridViewUserControlCell<T> : DataGridViewCell
    {
        public override Type FormattedValueType => typeof(string);
    }
}
