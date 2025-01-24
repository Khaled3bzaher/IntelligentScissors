using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ConsoleApp2;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        Boolean isLoaded = false; // Cannot Do Any Thing without Loading Picture
        Boolean isFinished = false; // To Draw Closed Path
        int isClicked = 0; // To Check There al least 2 points


        // Hold All Graph
        Dictionary<int, List<Node>> wightedGraph;
        // Hold All Mouse Clicked
        List<Tuple<int, int>> points = new List<Tuple<int, int>>();
        // Hold All Pathes Drawed
        List<List<Tuple<int, int>>> pathes = new List<List<Tuple<int, int>>>();
        int width;
        int height;
        // Graph & Load Picture
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

            width = Convert.ToInt32(txtWidth.Text);
            height = Convert.ToInt32(txtHeight.Text);

            // Start of the code
            //Graph
            //j=x-axis & no of columns  ---- i=y-axis & no of rows 
            Stopwatch graphTime = new Stopwatch();
            wightedGraph = new Dictionary<int, List<Node>>();
            graphTime.Start();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * width + j; // Pixel Index
                    wightedGraph[index] = new List<Node>(); // Hold 4 Connectivity
                 
                    Vector2D RightDown = ImageOperations.CalculatePixelEnergies(j, i, ImageMatrix);
                    if (j != width - 1) // Can Calc Right ?
                    {
                        wightedGraph[index].Add(new Node(i, j + 1, double.IsInfinity(1 / RightDown.X) ? 1e16 : 1 / RightDown.X, null));
                    }
                    if (i != height - 1) // Can Calc Down ?
                    {
                        wightedGraph[index].Add(new Node(i + 1, j, double.IsInfinity(1 / RightDown.Y) ? 1e16 : 1 / RightDown.Y, null));
                    }
                    if (i != 0) // Can Calc Up ?
                    {
                        double weightUp = 1 / ImageOperations.CalculatePixelEnergies(j, i - 1, ImageMatrix).Y;
                        wightedGraph[index].Add(new Node(i - 1, j, double.IsInfinity(weightUp) ? 1e16 : weightUp, null));
                    }
                    if (j != 0) // Can Calc Left ?
                    {
                        double weightLeft = 1 / ImageOperations.CalculatePixelEnergies(j - 1, i, ImageMatrix).X;
                        wightedGraph[index].Add(new Node(i, j - 1, double.IsInfinity(weightLeft) ? 1e16 : weightLeft, null));
                    }  
                }
            }
            //Calculate Time
            graphTime.Stop();
            TimeSpan GT = graphTime.Elapsed;
            Console.WriteLine("Graph Created in Time {0:00}:{1:00}:{2:00}.{3}",
                      GT.Hours, GT.Minutes, GT.Seconds, GT.Milliseconds);
            oneGenerate = 0; // To Generate One Sample File
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isLoaded || isFinished)
                return;

            isClicked++;

            Console.WriteLine("X: " + e.X + " Y: " + e.Y);

            // Remove Last Anchor
            if (e.Button == MouseButtons.Right && points.Count != 0)
            {
                isClicked--;
                if(points.Count - 1 != 0) { 
                    points.RemoveAt(points.Count - 1);
                    pathes.RemoveAt(pathes.Count - 1);
                }
            }
            else if (isClicked >= 1) // Draw Dijkstra between 2 Points
            {
                Tuple<int, int> click = new Tuple<int, int>(e.Y, e.X);
                //item 1 in dist Y item 2 in dists 
                points.Add(click);
                if (isClicked > 1) {
                    Stopwatch shortPathTime = new Stopwatch();
                    shortPathTime.Start();
                    var lastPath = Dijkstra(points[points.Count - 2], points[points.Count - 1]);
                    shortPathTime.Stop();
                    TimeSpan GT = shortPathTime.Elapsed;
                    Console.WriteLine("Path construction took: {0:00}:{1:00}:{2:00}.{3}",
                    GT.Hours, GT.Minutes, GT.Seconds, GT.Milliseconds);
                    pathes.Add(lastPath);

                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isLoaded || isFinished)
                return;

            textBox1.Text = e.X.ToString();
            textBox2.Text = e.Y.ToString();

            Pen p = new Pen(Color.Red, 1.0f);
            if (isClicked >= 1)
            {
                using (Graphics g = pictureBox1.CreateGraphics())
                {
                    Refresh();
                    //Anchors
                    foreach (var node in points)
                        g.DrawEllipse(new Pen(Color.Red, 4), node.Item2, node.Item1, 1, 1);
                    //Pathes
                    foreach (var currentPath in pathes)
                    {
                        foreach (var node in currentPath)
                        {
                            g.DrawEllipse(p, node.Item2, node.Item1, 1, 1);
                        }
                    }
                    //Dijkstra and Current Mouse Position
                    if (points.Count > 0)
                    {
                        List<Tuple<int, int>> ayAsm = Dijkstra(points[points.Count - 1], new Tuple<int, int>(e.Y, e.X));
                        foreach (var node in ayAsm)
                        {
                            g.DrawEllipse(p, node.Item2, node.Item1, 1, 1);
                        }
                    }
                }
            }
        }

        public List<Tuple<int, int>> Dijkstra(Tuple<int, int> src, Tuple<int, int> dist)
        {
            //Console.WriteLine("Src" + src + "Dist" + dist);
            // Item 1 -> Y    Item 2  -> X              IN SRC & DIST

            int Source = src.Item2 + src.Item1 * width;
            int Destination = dist.Item2 + dist.Item1 * width;

            priorityQueue prQueue = new priorityQueue();

            Dictionary<int, double> distance = new Dictionary<int, double>();
            Dictionary<int, int> parent = new Dictionary<int, int>();
            bool[] visted = new bool[width * height];

            List<Tuple<int, int>> path = new List<Tuple<int, int>>();

            prQueue.Add(new Node(src.Item1, src.Item2, 0, null), 0);
            distance[Source] = 0;
            parent[Source] = -1;

            while (prQueue.count != 0)
            {
                Node node = prQueue.Dequeue();
                int vertex = node.y + node.x * width;
                visted[vertex] = true;

                if (vertex == Destination)
                {
                    int temp = vertex;
                    while (parent[temp] != -1)
                    {
                        path.Add(new Tuple<int, int>(temp / width, temp % width));
                        temp = parent[temp];
                    }
                    path.Add(new Tuple<int, int>(temp / width, temp % width));
                    break;
                }
                //Node.X == Y && Node.Y == X
                foreach (var neighboor in wightedGraph[vertex])
                {
                    int Neigh = neighboor.y + neighboor.x * width;
                    if (!visted[Neigh])
                    {
                        if (!distance.ContainsKey(Neigh))
                        {
                            distance.Add(Neigh, distance[vertex] + neighboor.weight);
                            parent[Neigh] = vertex;
                            prQueue.Add(neighboor, distance[Neigh]);
                        }
                        if (distance[vertex] + neighboor.weight < distance[Neigh])
                        {
                            distance[Neigh] = distance[vertex] + neighboor.weight;
                            parent[Neigh] = vertex;
                            prQueue.Add(neighboor, distance[Neigh]);
                        }
                    }
                }
            }
            return path;
        }
        //Close The Path
        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isFinished = true;
                Pen p = new Pen(Color.Blue, 1.0f);
                using (Graphics g = pictureBox1.CreateGraphics())
                {
                    Refresh();
                    //To Draw Normal Line
                    List<Tuple<int, int>> ayAsm = Dijkstra(points[points.Count - 1], points[0]);
                    pathes.Add(ayAsm);
                    // All Pathes Drawing
                    int pixelCount = 0;
                    foreach (var path in pathes)
                        foreach (var node in path)
                        {
                            g.DrawEllipse(p, node.Item2, node.Item1, 1, 1);
                            pixelCount++;
                        }
                    Console.WriteLine("Full Path Pixels Count : " + pixelCount);
                    // To Draw Last Path Using Dijkstra
                    //g.DrawLine(p, points[points.Count - 1].Item2, points[points.Count - 1].Item1, points[0].Item2, points[0].Item1);
                }
            }
        }

        //Sample Output
        int oneGenerate = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!isLoaded || oneGenerate != 0)
                return;
            oneGenerate++;


            using (StreamWriter streamWriter = File.CreateText(@"G://FCIS//Sixth Semester//Algorithms//Project//13-5//IntelligentScissors//sampleGraphOutPut.txt"))
            {
                streamWriter.WriteLine("The constructed graph");
                streamWriter.WriteLine("");
                foreach (var pixel in wightedGraph.Keys) //0 1 2 3 4 5 6 7
                {
                    streamWriter.WriteLine(" The  index node" + pixel.ToString());
                    streamWriter.WriteLine("Edges");
                    foreach (var node in wightedGraph[pixel])
                    {
                        streamWriter.WriteLine("edge from   " + pixel.ToString() + "  To  " + (((node.x) * width) + (node.y)).ToString() + "  With Weights  " + node.weight.ToString());
                    }
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine("");
                }
            }
        }   
    }
}