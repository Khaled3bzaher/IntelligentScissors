using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        Boolean isLoaded = false;
        int isClicked = 0;
        int mouseX = 0;
        int mouseY = 0;
        Dictionary<Vector2D, List<double>> wightedGraph;
        List<Point> points = new List<Point>();

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                isLoaded = true;
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

            // Start of the code
            //Graph
            wightedGraph = new Dictionary<Vector2D, List<double>>();
            for (int i = 0; i < Convert.ToInt32(txtWidth.Text); i++)
            {
                for (int j = 0; j < Convert.ToInt32(txtHeight.Text); j++)
                {

                    Vector2D vertex = new Vector2D();
                    vertex.X = i;
                    vertex.Y = j;
                    wightedGraph[vertex] = new List<double>();
                    if (j != Convert.ToInt32(txtWidth.Text) - 1)
                    {
                        double wieghtRight = ImageOperations.CalculatePixelEnergies(i, j, ImageMatrix).X;
                        wightedGraph[vertex].Add(wieghtRight);
                    }
                    if (i != Convert.ToInt32(txtHeight.Text) - 1)
                    {
                        double wieghtBottom = ImageOperations.CalculatePixelEnergies(i, j, ImageMatrix).Y;
                        wightedGraph[vertex].Add(wieghtBottom);
                    }
                    if (j != 0)
                    {
                        double wieghtLeft = ImageOperations.CalculatePixelEnergies(i, j - 1, ImageMatrix).X;
                        wightedGraph[vertex].Add(wieghtLeft);

                    }
                    if (i != 0)
                    {
                        double wieghtUp = ImageOperations.CalculatePixelEnergies(i - 1, j, ImageMatrix).Y;
                        wightedGraph[vertex].Add(wieghtUp);

                    }



                }
            }

        }
        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        //}




        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isLoaded)
                return;
            
            if(e.Button == MouseButtons.Right)
            {
                
                points.RemoveAt(points.Count - 1);
                return;

            }
            Vector2D vector2 = new Vector2D();
            vector2.X = e.X;
            vector2.Y = e.Y;

            textBox3.Text = wightedGraph[vector2][0].ToString();
            textBox4.Text = wightedGraph[vector2][1].ToString();
            textBox5.Text = wightedGraph[vector2][2].ToString();
            textBox6.Text = wightedGraph[vector2][3].ToString();
            

           
            
            isClicked++;
            
            Point point = new Point(e.X, e.Y);
            points.Add(point);

            mouseX = e.X;
            mouseY = e.Y;


        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isLoaded)
                return;

            textBox1.Text = e.X.ToString();
            textBox2.Text = e.Y.ToString();

            Pen p = new Pen(Color.Red, 1.0f);
            if (isClicked >= 1)
            {
                using (Graphics g = pictureBox1.CreateGraphics())
                {
                    Refresh();
                    for (int i = 0; i < points.Count - 1; i++)
                        g.DrawLine(p, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                    g.DrawLine(p, points[points.Count -1].X, points[points.Count - 1].Y, e.X, e.Y);


                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!isLoaded)
                return;

        }



    }
}