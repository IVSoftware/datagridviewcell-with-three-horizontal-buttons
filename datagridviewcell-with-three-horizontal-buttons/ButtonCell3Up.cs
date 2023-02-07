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
        public string[] Labels
        {
            set
            {
                var buttons = new Control[] { button1, button2, button3 };
                for (int i = 0; i < 3; i++)
                {
                    buttons[i].Text = value[i];
                }
            }
        }

        public ButtonCell3Up()
        {
            InitializeComponent();
            foreach (RadioButton radio in new Control[] { button1, button2, button3 })
            {
                radio.CheckedChanged += onRadioCheckedChanged;
                radio.TextChanged += (sender, e) =>
                {
                };
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
