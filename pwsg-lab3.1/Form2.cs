using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pwsg_lab3._1
{
    public partial class Form2 : Form
    {
        Bitmap bitmap;
        int imagenumber;
        Library library;
        bool saved;
        public Form2(Blender blender)
        {
            imagenumber = blender.imagenumber;
            bitmap = blender.bitmap;
            library = blender.mainform.library;
            SetUpWindow();
            InitializeComponent();
        }

        public void SetUpWindow()
        {
            pictureBox1 = new PictureBox();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Click += new EventHandler(pictureBox1_Click);
            this.Controls.Add(pictureBox1);
            pictureBox1.Image = bitmap;
            saved = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            if (MouseButtons.Right == mouse.Button)
            {
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!saved)
                SaveImage();
        }

        private void addToLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!saved)
            {
                string filepath = SaveImage();
                library.AddPicture(bitmap,filepath);
            }
        }

        public string SaveImage()
        {
            saved = true;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "image files (*.bmp; *.jpg; *.png) | *.bmp; *.jpg; *.png";
            sfd.Title = "Save an Image";
            sfd.FileName = "NewImage" + imagenumber.ToString();

            if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != "")
            {
                bitmap.Save(sfd.FileName);
            }
            return System.IO.Path.GetFullPath(sfd.FileName);
        }
    }

    public class PictureInLibrary
    {
        public bool active;
        public PictureBox picture;
        public Library library;

        public PictureInLibrary(Bitmap bitmap, Library library)
        {
            active = false;
            this.library = library;
            SetUpPicture(bitmap);
        }

        public void SetUpPicture(Bitmap bitmap)
        {
            picture = new PictureBox();
            picture.Image = bitmap;
            picture.Width = picture.Height = 150;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;
            picture.Tag = Color.White;
            picture.Paint += new PaintEventHandler(pictureBoxPaint);
            picture.Click += new EventHandler(pictureBoxClick);
        }

        private void pictureBoxClick(object sender, EventArgs e)
        {
            PictureBox picturebox = (PictureBox)sender;
            library.ChangeActivePictures(this);
            picturebox.Refresh();
        }

        private void pictureBoxPaint(object sender, PaintEventArgs e)
        {
            PictureBox picturebox = (PictureBox)sender;
            Color c = Color.White;
            if (active)
                c = Color.Orange;
            ControlPaint.DrawBorder(e.Graphics, picturebox.ClientRectangle, c, ButtonBorderStyle.Solid);
        }

        public void ChangeActive()
        {
            if (active)
                active = false;
            else
                active = true;
            picture.Refresh();
        }
    }

    public class Library
    {
        FlowLayoutPanel flowlayoutpanel;
        public List<PictureInLibrary> pictures;
        public List<string> filepaths;

        public Library(FlowLayoutPanel flowlayoutpanel)
        {
            this.flowlayoutpanel = flowlayoutpanel;
            pictures = new List<PictureInLibrary>();
            filepaths = new List<string>();
            flowlayoutpanel.DragEnter += new DragEventHandler(flowLayoutPanel_DragEnter);
            flowlayoutpanel.DragDrop += new DragEventHandler(flowLayoutPanel1_DragDrop);
        }

        public void AddPicture(Bitmap bitmap,string filepath)
        {
            PictureInLibrary image = new PictureInLibrary(bitmap, this);
            pictures.Add(image);
            filepaths.Add(filepath);
            ActualizeXml();
            flowlayoutpanel.Controls.Add(image.picture);
        }

        public void DeleteActivePicture()
        {
            for (int i = 0; i < pictures.Count; i++)
                if (pictures[i].active)
                {
                    pictures.RemoveAt(i);
                    flowlayoutpanel.Controls.RemoveAt(i);
                    filepaths.RemoveAt(i);
                    ActualizeXml();
                    return;
                }
        }

        public void ActualizeXml()
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            System.IO.Stream s = System.IO.File.Create("imgLibrary.xml");
            xs.Serialize(s, filepaths);
            s.Close();
        }

        public void ChangeActivePictures(PictureInLibrary picture)
        {
            foreach (PictureInLibrary p in pictures)
                if (p == picture || p.active)
                    p.ChangeActive();
        }

        void flowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filename in filenames)
            {
                string filepath = System.IO.Path.GetFullPath(filename);
                LoadPictureFromFile(filepath);
            }
        }

        public void LoadPicturesFromFiles(List<string> tmpfilepaths)
        {
            foreach (string filepath in tmpfilepaths)
            {
                LoadPictureFromFile(filepath);
            }
        }

        public void LoadPictureFromFile(string filepath)
        {
            Image image = Image.FromFile(filepath);
            Bitmap bitmap = new Bitmap(image);
            AddPicture(bitmap, filepath);
        }

    }
}
