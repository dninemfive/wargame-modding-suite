using System;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.ScenarioItems.PropertyPanels;

public partial class ZoneProperty : UserControl
{
    private readonly Zone zone;
    public ZoneProperty(Zone z)
    {
        InitializeComponent();
        zone = z;

    }
    public void update()
    {
        comboBox1.SelectedIndex = (int)zone.possession;
        textBox1.Text = string.Format("{0}", zone.value);
    }
    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => zone.possession = (Possession)comboBox1.SelectedIndex;

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(textBox1.Text, out int temp))
        {
            zone.value = temp;
        }
        textBox1.Text = string.Format("{0}", zone.value);

    }
}
