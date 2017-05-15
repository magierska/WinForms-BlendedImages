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
    public partial class Form1 : Form
    {
        bool[] fullpictures = new bool[2];
        PictureBox[] pictureboxes = new PictureBox[2];
        public Blender[] blenders = new Blender[2];
        public Library library;
        int imagenumber;
        public Form1()
        {
            InitializeComponent();
            SetUpWindow();
        }

        public void SetUpWindow()
        {
            imagenumber = 0;
            CreateLibrary();
            InitializingArrays();
            AddingEventHandlers();
        }

        public void CreateLibrary()
        {
            flowLayoutPanel1.AllowDrop = true;
            library = new Library(flowLayoutPanel1);
            string filepath = Application.StartupPath + "\\imgLibrary.xml";
            if (System.IO.File.Exists(filepath))
                DownloadLibrary();
        }

        public void DownloadLibrary()
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<string>));
            System.IO.Stream s = System.IO.File.Open("imgLibrary.xml", System.IO.FileMode.Open);
            List<string> filepaths = (List<string>)xs.Deserialize(s);
            s.Close();
            library.LoadPicturesFromFiles(filepaths);
        }

        public void InitializingArrays()
        {
            pictureboxes[0] = pictureBox1;
            pictureboxes[1] = pictureBox2;
            blenders[0] = new Blender(this,progressBar1);
            blenders[1] = new Blender(this,progressBar2);
            blenders[0].active = false;
            blenders[1].active = false;
        }

        public void AddingEventHandlers()
        {
            this.KeyDown += new KeyEventHandler(MyKeyDown);
            pictureboxes[0].Click += new EventHandler(pictureBox_Click);
            pictureboxes[1].Click += new EventHandler(pictureBox_Click);
        }


        private void MyKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
                TakeScreenshot();
            else if (e.KeyCode == Keys.Delete)
                library.DeleteActivePicture();
        }

        public void TakeScreenshot()
        {
            Bitmap bitmap = GettingBitmapFromScreen();
            LoadingBitmapFromScreen(bitmap);
        }

        public Bitmap GettingBitmapFromScreen()
        {
            Rectangle ScreenSize = Screen.PrimaryScreen.Bounds;
            Bitmap bitmap = new Bitmap(ScreenSize.Width, ScreenSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(ScreenSize.X, ScreenSize.Y, 0, 0, ScreenSize.Size);
            return bitmap;
        }

        public void LoadingBitmapFromScreen(Bitmap bitmap)
        {
            int whichone = 0;
            if (!fullpictures[1])
                whichone = 1;
            LoadingPictureFromBitmap(pictureboxes[whichone], bitmap);
        }


        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox picturebox = sender as PictureBox;
            foreach (PictureInLibrary p in library.pictures)
                if (p.active)
                {
                    LoadingPictureFromBitmap(picturebox, (Bitmap)p.picture.Image);
                    return;
                }
            ChoosingPicture(picturebox);
        }

        public void ChoosingPicture(PictureBox picturebox)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "image files (*.bmp; *.jpg; *.png) | *.bmp; *.jpg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadingPictureFromBitmap(picturebox, new Bitmap(ofd.FileName));
        }

        public void LoadingPictureFromBitmap(PictureBox picturebox, Bitmap bitmap)
        {
            picturebox.Image = bitmap;
            ActualizeFullPictures(picturebox);
        }

        public void ActualizeFullPictures(PictureBox picturebox)
        {
            fullpictures[Int32.Parse((string)picturebox.Tag)] = true;
            if (fullpictures[0] && fullpictures[1])
                button1.Enabled = true;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            int whichone = 0;
            if (blenders[0].active)
            {
                whichone = 1;
                button1.Enabled = false;
            }
            blenders[whichone].StartBlendingProcess(imagenumber);
            imagenumber++;
        }
    }

    public class Blender
    {
        public Form1 mainform;
        BackgroundWorker backgroundworker;
        ProgressBar progressbar;
        public Bitmap bitmap;
        public bool active;
        public int height, width;
        public double alfa;
        public int imagenumber;
        public Bitmap[] bitmaps = new Bitmap[2];

        public Blender(Form1 mainform, ProgressBar progressbar)
        {
            this.mainform = mainform;
            this.progressbar = progressbar;
            SetUpBackGroundWorker();
        }

        public void SetUpBackGroundWorker()
        {
            backgroundworker = new BackgroundWorker();
            backgroundworker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            backgroundworker.DoWork += new DoWorkEventHandler(DoWork);
            backgroundworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            backgroundworker.WorkerReportsProgress = true;
        }

        public void StartBlendingProcess(int imagenumber)
        {
            this.imagenumber = imagenumber;
            ChangingBlendingStuff(true);
            GetInfoAboutPictures();
            backgroundworker.RunWorkerAsync();
        }

        public void GetInfoAboutPictures()
        {
            alfa = (double)mainform.trackBar1.Value / 10;
            bitmaps[0] = (Bitmap)(mainform.pictureBox1.Image).Clone();
            bitmaps[1] = (Bitmap)(mainform.pictureBox2.Image).Clone();
            height = bitmaps[0].Height < bitmaps[1].Height ? bitmaps[0].Height : bitmaps[1].Height;
            width = bitmaps[0].Width < bitmaps[1].Width ? bitmaps[0].Width : bitmaps[1].Width;
            progressbar.Maximum = width;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
           Blending();
        }

        public void Blending()
        {
            CreatingResultBitmap();
            SettingColors();
        }

        public void CreatingResultBitmap()
        {
            bitmap = new Bitmap(width, height);
        }

        public void SettingColors()
        {
            Color c;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    c = GettingColor(alfa, i, j);
                    bitmap.SetPixel(i, j, c);
                }
                backgroundworker.ReportProgress(i);
            }
        }

        public Color GettingColor(double alfa, int i, int j)
        {
            int r = (int)(alfa * bitmaps[0].GetPixel(i, j).R + (1 - alfa) * bitmaps[1].GetPixel(i, j).R);
            int g = (int)(alfa * bitmaps[0].GetPixel(i, j).G + (1 - alfa) * bitmaps[1].GetPixel(i, j).G);
            int b = (int)(alfa * bitmaps[0].GetPixel(i, j).B + (1 - alfa) * bitmaps[1].GetPixel(i, j).B);
            Color c = Color.FromArgb(r, g, b);
            return c;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressbar.Value = e.ProgressPercentage;
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Form2 resultwindow = new Form2(this);
            resultwindow.Text = "Image Window";
            resultwindow.Show();
            ChangingBlendingStuff(false);
        }

        public void ChangingBlendingStuff(bool opening)
        {
            active = opening;
            progressbar.Visible = opening;
            mainform.label2.Visible = opening;
        }
    }
}
