using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
    public partial class ButtonCell3Up : UserControl
    {
        static int _id = -1;
        public ButtonCell3Up()
        {
            _id++;
            InitializeComponent();
            foreach (RadioButton radio in new Control[] { button1, button2, button3 })
            {
                radio.CheckedChanged += onRadioCheckedChanged;
                radio.Text = $"{_id}{radio.Text}";
            }
            Text = button1.Text;
        }

        private void onRadioCheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radio)
            {
                radio.ForeColor = radio.Checked ? Color.DarkCyan : Color.FromArgb(16, 16, 16);
                if(radio.Checked)
                {
                    Text = radio.Text;
                    ButtonChanged?.Invoke(radio, EventArgs.Empty);
                }
            }
        }
        public event EventHandler ButtonChanged;
    }
}
