using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ConsoleApp2
{
    public class Node
    {
        public int x,y;
        public double weight;
        public Node parent;

        public Node(int x, int y, double weight, Node parent)
        {
            this.x = x;
            this.y = y;

            this.weight = weight;
            this.parent = parent;
        }
    }
    internal class priorityQueue
    {
        // implement priority queue
        private List<Node> nodeQueue;
        private List<double> weights;
        public priorityQueue()
        {
            nodeQueue = new List<Node>();
            weights = new List<double>();
        }

        public int count
        {
            get
            {
                return nodeQueue.Count;
            }
        }

        public void Add(Node pix, double weight)
        {
                nodeQueue.Add(pix);
                weights.Add(weight);
                SiftUp(count - 1);
        }
        public Node Dequeue()
        {
            if (nodeQueue.Count == 0)
            {
                return null;
            }
            //Node
            Node n = nodeQueue[0];
            nodeQueue[0] = nodeQueue[nodeQueue.Count - 1];
            nodeQueue.RemoveAt(nodeQueue.Count - 1);
            //Weight
            weights[0] = weights[weights.Count - 1];
            weights.RemoveAt(weights.Count - 1);
            heapify(0);
            return n;
        }
        public void SiftUp(int i)
        {
            int parent = (i - 1) / 2;
            if ( nodeQueue[i].weight >= nodeQueue[parent].weight || i == 0)
                return;
            //Node
            Node temp = nodeQueue[i];
            nodeQueue[i] = nodeQueue[parent];
            nodeQueue[parent] = temp;
            //Weight
            double temp2 = weights[i];
            weights[i] = weights[parent];
            weights[parent] = temp2;
            SiftUp(parent);
        }

        void heapify(int i)
        {
            int l = 2 * i + 1;
            int r = 2 * i + 2;
            int min = i;

            if (l < nodeQueue.Count && weights[l] < weights[i])
            {
                min = l;
            }
            if (r < nodeQueue.Count && weights[r] < weights[min])
            {
                min = r;
            }

            if (min != i)
            {
                //Node
                Node temp1 = nodeQueue[i];
                nodeQueue[i] = nodeQueue[min];
                nodeQueue[min] = temp1;
                //Weight
                double temp2 = weights[i];
                weights[i] = weights[min];
                weights[min] = temp2;
                heapify(min);
            }
        }
    }
}