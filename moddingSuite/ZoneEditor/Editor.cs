using moddingSuite.ZoneEditor.Markers;
using moddingSuite.ZoneEditor.ScenarioItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace moddingSuite.ZoneEditor;

public partial class Editor : Form
{
    public Point LeftClickPoint;
    private System.Drawing.Image image;

    private Redraw redraw => new(delegate ()
                                      {

                                          foreach (object c in pictureBox1.Controls)
                                          {
                                              if (c is Marker)
                                              {
                                                  Marker a = c as Marker;
                                                  a.UpdateMarker();
                                              }
                                          }
                                          pictureBox1.Select();
                                          pictureBox1.Refresh();
                                      });

    private readonly ZoneEditorData zoneData;

    public Editor(ZoneEditorData ze, string path)
    {
        InitializeComponent();
        zoneData = ze;
        buildImage(path);
        Name = "ZoneDrawer";
        Text = "ZoneDrawer";

        Graphics g = CreateGraphics();
        float zoom = pictureBox1.Width / (float)image.Width *
                (image.HorizontalResolution / g.DpiX);
        PanAndZoom.setZoom(zoom);

        pictureBox1.Paint += new PaintEventHandler(OnPaint);
        pictureBox1.MouseDown += PanAndZoom.MouseDown;
        pictureBox1.MouseMove += PanAndZoom.MouseMove;

        pictureBox1.MouseUp += PanAndZoom.MouseUp;
        pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_Click);
        pictureBox1.MouseWheel += PanAndZoom.MouseWheel;

        pictureBox1.Select();
        //contextMenuStrip1
        PanAndZoom.redraw = redraw;
        contextMenuStrip1.Items[1].Click += ze.AddZone;
        ToolStripMenuItem spawns = contextMenuStrip1.Items[2] as ToolStripMenuItem;
        spawns.DropDown.Items[0].Click += ze.AddLandSpawn;
        spawns.DropDown.Items[1].Click += ze.AddAirSpawn;
        spawns.DropDown.Items[2].Click += ze.AddSeaSpawn;
        ToolStripMenuItem positions = contextMenuStrip1.Items[3] as ToolStripMenuItem;
        positions.DropDown.Items[0].Click += ze.AddCV;
        positions.DropDown.Items[1].Click += ze.AddFOB;

        button1.Click += new System.EventHandler(ze.deleteItem);

        //outline = new Outline(pictureBox1);
        //pictureBox1.Paint += new PaintEventHandler(outline.paint);

    }
    public void addScenarioItem(ScenarioItem item, bool select = false)
    {
        item.attachTo(pictureBox1);
        _ = listBox1.Items.Add(item.ToString());
        if (select)
        {
            _ = zoneData.setSelectedItem(item.ToString());
            listBox1.SelectedItem = item.ToString();
        }
        redraw();
    }
    private void stripClicked(object obj, EventArgs e) => Console.WriteLine("hello");
    private void pictureBox1_Click(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            LeftClickPoint = e.Location;
            contextMenuStrip1.Show(pictureBox1, e.Location);
        }
        //var scalingFactor = 5100;
        //textBox1.Text = String.Format("{0}", e.X * scalingFactor);
        //textBox2.Text = String.Format("{0}", e.Y * scalingFactor);
        //Console.WriteLine(e.Location);
    }

    private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e) => Console.WriteLine(e.ClickedItem.Name);

    private void groupBox1_Enter(object sender, EventArgs e)
    {

    }
    private void buildImage(string path)
    {
        string mapName = getMapName(path);

        if (mapName == null)
        {
            image = new Bitmap(500, 500);
            return;
        }

        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.IO.Stream imgStream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".ZoneEditor.Images." + mapName + ".png");
        //Console.WriteLine(assembly.GetName().Name);

        image = new Bitmap(imgStream);

    }
    private string getMapName(string path)
    {
        //map\scenario\_2x2_port_wonsan_terrestre_destruction\zonebluff\leveldesign.kdt	0 B

        List<string> maps = new(){
            "_2x2_port_wonsan",
            "_2x3_anbyon",
            "_2x3_esashi",
            "_2x3_hwaseong",
            "_2x3_montagne_2",
            "_2x3_tohoku",
            "_3x2_sangju",
            "_3x3_chongju",
            "_3x3_gangjin",
            "_3x3_pyeongtaek"
        };
        foreach (string m in maps)
        {
            //var p=path.Substring(13,m.Length);
            if (path.Contains(m))
                return m;
        }
        return null;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        PanAndZoom.Transform(e);
        e.Graphics.DrawImage(image, 0, 0);
    }
    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {

    }
    private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
    {

    }
    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (zoneData.selectedItem != null)
        {
            panel1.Controls.Remove(zoneData.selectedItem.propertypanel);
        }
        _ = zoneData.setSelectedItem((string)listBox1.SelectedItem);
        panel1.Controls.Add(zoneData.selectedItem.propertypanel);

    }
    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {

    }

    private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }

    private void splitContainer2_Panel2_Paint_1(object sender, PaintEventArgs e)
    {

    }

    private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
    {

    }

    public void deleteItem(ScenarioItem item)
    {
        item.detachFrom(pictureBox1);
        listBox1.Items.Remove(item.ToString());

        redraw();
    }
}
