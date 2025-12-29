using System.Windows.Forms;

namespace ImageEdit;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void Button1_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.ico";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            pictureBox1.Image = Image.FromFile(dialog.FileName);
            pictureBox1.Image = Image.FromFile(dialog.FileName);
        }
    }
}
