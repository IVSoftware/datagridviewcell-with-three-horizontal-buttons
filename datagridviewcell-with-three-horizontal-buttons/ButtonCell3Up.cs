using System;
using System.Drawing;
using System.Windows.Forms;

namespace datagridviewcell_with_three_horizontal_buttons
{
    public partial class ButtonCell3Up : UserControl
    {
        public ButtonCell3Up()
        {
            InitializeComponent();
            foreach (RadioButton radio in new Control[] { button1, button2, button3 })
            {
                radio.CheckedChanged += onRadioCheckedChanged;
            }
            Text = button1.Text;
        }

        public string[] Labels
        {
            set
            {
                var buttons = new RadioButton[] { button1, button2, button3 };
                for (int i = 0; i < 3; i++)
                {
                    buttons[i].Text = value[i];
                    if(buttons[i].Checked)
                    {
                        Text = buttons[i].Text;
                    }
                }
            }
        }

        public new event EventHandler TextChanged
        {
            add => base.TextChanged += value;
            remove => base.TextChanged += value;
        }

        public new string Text
        {
            get => base.Text;
            set
            {
                if (!Equals(base.Text, value))
                {
                    base.Text = value;
                    OnTextChanged(EventArgs.Empty);
                }
            }
        }

        private void onRadioCheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radio)
            {
                radio.ForeColor = radio.Checked ? Color.DarkCyan : Color.FromArgb(16, 16, 16);
                if(radio.Checked)
                {
                    Text = radio.Text;
                }
            }
        }
    }
}
