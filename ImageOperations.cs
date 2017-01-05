using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;

///Algorithms Project
///Intelligent Scissors

namespace ImageQuantization
{
    
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public int red, green, blue;
    }
    public struct edge
    {
        public RGBPixelD first_node,second_node;
        public double weight;
        
    }
    
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public static double weight(RGBPixelD one,RGBPixelD two){
            double temp_weight;
            double R = one.red - two.red;
            double G = one.green - two.green;
            double B = one.blue - two.blue;
            temp_weight = Math.Sqrt((R*R) + (G*G) + (B*B));
            return temp_weight;
        }
        public static RGBPixelD setnodenull()
        {
            RGBPixelD mini;
            mini.red = -1;
            mini.green = -1;
            mini.blue = -1;
            return mini;
        }
       
        public static void InitalizeDis(ref double[, ,] dist)
        {
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    for (int k = 0; k < 256; k++)
                    {
                        dist[i, j, k] = double.MaxValue;
                    }
                }
            }
            
        }
       
        public static bool[, ,] visited = new bool[256, 256, 256];

        public static Dictionary<RGBPixelD, List<RGBPixelD>> palette = new Dictionary<RGBPixelD, List<RGBPixelD>>();

        public static RGBPixelD avg = new RGBPixelD();
        public static int counter = 0;

        public static List<RGBPixelD> collect_colors = new List<RGBPixelD>();
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }
        public static void dfs(RGBPixelD vertex)
        {
            counter++;
            collect_colors.Add(vertex);
            avg.red += vertex.red;
            avg.green += vertex.green;
            avg.blue += vertex.blue;
            visited[vertex.red, vertex.green, vertex.blue] = true;
            for (int i = 0; i < palette[vertex].Count; i++)
            {
                if (!visited[palette[vertex][i].red, palette[vertex][i].green, palette[vertex][i].blue])
                {
                    dfs(palette[vertex][i]);
                    
                }
            }
        }
        
        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        public static RGBPixel[,] quantization(RGBPixel[,] ImageMatrix,double n_clusters)
        {
            String Startt = DateTime.Now.ToString("h:mm:ss tt");
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            RGBPixelD[,]DimageMatrix=new RGBPixelD[Height,Width];
            List<RGBPixelD> distnict = new List<RGBPixelD>();

            bool[, ,] freq = new bool[256, 256, 256];
            //construct double 2d array of the Image
            // cast ImageMatrix to type (RGPPixelD)
            for (int i = 0; i < Height; i++)
            {
                for(int j = 0 ;j < Width ; j++){
                    DimageMatrix[i,j].red=(int)ImageMatrix[i,j].red;
                    DimageMatrix[i,j].green=(int)ImageMatrix[i,j].green;
                    DimageMatrix[i, j].blue = (int)ImageMatrix[i, j].blue;
                    if (!freq[DimageMatrix[i, j].red, DimageMatrix[i, j].green,DimageMatrix[i, j].blue])
                    {
                        freq[DimageMatrix[i, j].red, DimageMatrix[i, j].green, DimageMatrix[i, j].blue] = true;
                        distnict.Add(DimageMatrix[i, j]);
                    }
                }
            }


            RGBPixelD[] distnict_colors = new RGBPixelD[distnict.Count];
            distnict.CopyTo(distnict_colors);          

            //----------------------------------------------------------------------------------------------------------------------------------
            // Prim's Algorithm
            double  mstCost = 0,mind=0;
            RGBPixelD curNode = distnict_colors[0], mini;
            int size=distnict.Count;
            bool[, ,] visi = new bool[256, 256, 256];
            RGBPixelD[, ,] previ = new RGBPixelD[256, 256, 256];
            double[, ,] disti = new double[256, 256, 256];
            InitalizeDis(ref disti);
            List<edge> edges = new List<edge>();
            

            for (int k = 0; k < size - 1; k++)
            {
                visi[curNode.red, curNode.green,curNode.blue] = true;
                mind = Double.MaxValue;
                mini=setnodenull();
                //function set mini to -1;

                for (int j = 0; j < size; j++)
                {
                    if (!visi[distnict_colors[j].red, distnict_colors[j].green, distnict_colors[j].blue])
                    {
                        double temp_weight;
                        temp_weight = weight(curNode,distnict_colors[j]);
                        if (temp_weight < disti[distnict_colors[j].red, distnict_colors[j].green, distnict_colors[j].blue])
                        {
                            disti[distnict_colors[j].red, distnict_colors[j].green, distnict_colors[j].blue] = temp_weight;
                            previ[distnict_colors[j].red, distnict_colors[j].green,distnict_colors[j].blue] = curNode;
                        }
                        if (disti[distnict_colors[j].red, distnict_colors[j].green, distnict_colors[j].blue] < mind)
                        {
                            mind = disti[distnict_colors[j].red, distnict_colors[j].green, distnict_colors[j].blue];
                            mini.red = distnict_colors[j].red;
                            mini.green = distnict_colors[j].green;
                            mini.blue = distnict_colors[j].blue;
                        }
                    }
                }
                double wei = weight(previ[mini.red, mini.green, mini.blue], mini);
                edge temp_edge;
                temp_edge.first_node = previ[mini.red, mini.green, mini.blue];
                temp_edge.second_node = mini;
                temp_edge.weight = wei;
                edges.Add(temp_edge);
                curNode=mini;
                mstCost += disti[curNode.red, curNode.green, curNode.blue];
            }
            

            
            //-------------------------------------------------------MileStone 2--------------------------------------------
            
            int max=0;
            double maxWieght = -1;
            n_clusters--;
            for (int i = 0; i < n_clusters; i++)
            {
                for (int j = 0; j < edges.Count; j++)
                {
                    if (edges[j].weight > maxWieght)
                    {
                        max = j;
                        maxWieght = edges[j].weight;
                    }
                }
                maxWieght = -1;
                edges.RemoveAt(max);
            }  

            //clustring .. Construct Adjcancey list
            for (int i = 0; i < distnict_colors.Count(); i++)
            {
                palette.Add(distnict_colors[i], new List<RGBPixelD>());
                palette[distnict_colors[i]].Add(distnict_colors[i]);
            }

            for (int i = 0; i < edges.Count; i++)
            {
                    palette[edges[i].second_node].Add(edges[i].first_node);
                    palette[edges[i].first_node].Add(edges[i].second_node);                
            }

            foreach (var item in palette)
            {
                if (!visited[item.Key.red, item.Key.green,item.Key.blue])
                {  
                    avg.red = 0;
                    avg.green = 0;
                    avg.blue = 0;
                    counter = 0;
                    collect_colors.Clear();
                    dfs(item.Key);
                    avg.red = avg.red / counter;
                    avg.green = avg.green / counter;
                    avg.blue = avg.blue / counter ;
                    palette[item.Key].Clear();
                    palette[item.Key].Add(avg);
                    for (int i = 0; i < collect_colors.Count; i++)
                    {
                        palette[collect_colors[i]].Clear();
                        palette[collect_colors[i]].Add(avg);
                    }
                }
            }

            //String Finish = DateTime.Now.ToString("h:mm:ss tt");
            //MessageBox.Show(Startt + " " + Finish + "   mst = " + mstCost);
            for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                       
                        DimageMatrix[i, j] = palette[DimageMatrix[i, j]][0];
                        ImageMatrix[i, j].red = (byte)DimageMatrix[i, j].red;
                        ImageMatrix[i, j].green = (byte)DimageMatrix[i, j].green;
                        ImageMatrix[i, j].blue = (byte)DimageMatrix[i, j].blue;
                   }
                }

            String Finish = DateTime.Now.ToString("h:mm:ss tt");
            MessageBox.Show(Startt + " " + Finish + "   mst = " + mstCost);
            //sw.Stop();
            return ImageMatrix;
        }
    }
}
/*
            double mean = mstCost / edges.Count;
            MessageBox.Show("Mean = " + mean);
            double summation = 0;//summation of (Xi - mean)^2
            for (int i = 0; i < edges.Count; i++)
            {
                summation += ((edges[i].weight - mean) * (edges[i].weight - mean));
            }
            // standard deviation = sqrt (summation/N)
            double standard_deviation = Math.Sqrt(summation / edges.Count);
            MessageBox.Show("standard deviation = " + standard_deviation);
            */