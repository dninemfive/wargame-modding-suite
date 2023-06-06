using System;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor.ScenarioItems.PropertyPanels;

public partial class SpawnProperty : UserControl
{
    private readonly Spawn spawn;
    public SpawnProperty(Spawn s)
    {
        spawn = s;
        InitializeComponent();
    }
    public void update() => comboBox1.SelectedIndex = (int)spawn.type;//textBox1.Text = string.Format("{0}", zone.value);
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => spawn.type = (SpawnType)comboBox1.SelectedIndex;
}
