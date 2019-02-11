using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ConsoleApp1
{
    class Program
    {
        public static DepthType DepthTypeCv { get; private set; }
        public static object Openfile { get; private set; }

        
        static void Main(string[] args)
        {
            String image_path = "D:/Work/correlation/Rtest/test (6).png";
            Image<Gray, byte> img = new Image<Gray, byte>(image_path);



            int height = img.Height;
            int width = img.Width;


            List<int> indices_x = new List<int>();
            List<int> indices_y = new List<int>();
            int intersectionX, intersectionY;
            int edge_flag = 1;

            XoringImage(image_path, out indices_x, out indices_y);
            CenterAnalysis(indices_x, indices_y, width, height, out intersectionX, out intersectionY);

            if (intersectionX == -1 && intersectionY == -1)
            {
                Console.WriteLine("No repeat Pattern Detected");
                //EdgeXoringImage(image_path, out indices_x, out indices_y);
                //CenterAnalysis(indices_x, indices_y, width, height, out intersectionX, out intersectionY);

                //XoringImage(image_path, out indices_x, out indices_y);
                //CenterAnalysis(indices_x, indices_y, width, height, out intersectionX, out intersectionY);

            }
            Console.WriteLine("Final Point for Repeated Pattern");

            Console.WriteLine(intersectionX);
            Console.WriteLine(intersectionY);

            Console.ReadLine();


        }

        //analysis the list returned
        static public void CenterAnalysis(List<int> darkInX, List<int> darkInY, int width, int height, out int intersectionX, out int intersectionY)
        {
            intersectionX = 0;
            intersectionY = 0;

            if (darkInX.Count != 0 && darkInY.Count != 0)
            {
                if (darkInX.Count >= (width / 2))
                {
                    Console.WriteLine("Horizontal Lining Pattern");

                    intersectionX = (width / 2);
                    intersectionY = darkInY[0];
                }

                else if (darkInY.Count >= (height / 2))
                {
                    Console.WriteLine("Vertical Lining Pattern");
                    intersectionX = darkInX[0];
                    intersectionY = height / 2;
                }

                else
                {
                    //Detect false negative patterns
                    if (((darkInX[0] == 1 || darkInX[0] == 2) && (darkInY[0] == 1 || darkInY[0] == 2)) || ((darkInX[0] == width - 1) && darkInY[0] == height - 1))
                    {
                        Console.WriteLine("Returning for Edge Detection");
                        intersectionX = -1;
                        intersectionY = -1;
                    }

                    //logic for complex horizontal pattern
                    else if ((darkInX[0] == 1) && (darkInY[0] != 1) && darkInY[0] != height - 1)
                    {
                        intersectionX = width;
                        intersectionY = darkInY[0];
                    }

                    //logic for complex vertical pattern
                    else if ((darkInY[0] == 1) && (darkInX[0] != 1) && (darkInX[0] != width - 1))
                    {
                        intersectionX = darkInX[0];
                        intersectionY = height;
                    }

                    //logic for simple pattern
                    else
                    {
                        intersectionX = darkInX[0];
                        intersectionY = darkInY[0];
                    }
                }

            }
            else
                Console.WriteLine("No repeated pattern detected at Level 1");
        }

        static public void XoringImage(String path, out List<int> indices_horizontal, out List<int> indices_vertical)
        {

            indices_horizontal = new List<int>();
            indices_vertical = new List<int>();


            Image<Gray, byte> image = new Image<Gray, byte>(path);
            Image<Gray, byte> image2 = new Image<Gray, byte>(path);
            Image<Gray, byte> original = new Image<Gray, byte>(path);




            Dictionary<int, double> horizontal_values = new Dictionary<int, double>();
            Dictionary<int, double> vertical_values = new Dictionary<int, double>();
            Dictionary<int, int> trial = new Dictionary<int, int>();


            int height = image.Height;
            int width = image.Width;

            int i = 1;
            for (i = 1; i < height; i++)
            {
                Image<Gray, byte> row = new Image<Gray, byte>(1, image.Width);
                CvInvoke.cvGetRow(image, row, 0);
                Image<Gray, byte> xor_image = new Image<Gray, byte>(image.Height, image.Width);

                Rectangle rect = new Rectangle(0, 1, image.Width, image.Height);
                image.ROI = rect;


                Image<Gray, byte> gray = image.Convert<Gray, byte>();
                Image<Gray, byte> gray_row = row.Convert<Gray, byte>();


                image = gray.ConcateVertical(gray_row);





                CvInvoke.BitwiseXor(original, image, xor_image);
                Gray gray_sum = xor_image.GetSum();
                double intensity = gray_sum.Intensity;



                vertical_values.Add(i, intensity);
            }

            for (i = 1; i < width; i++)
            {
                Image<Gray, byte> column = new Image<Gray, byte>(image2.Height, 1);
                CvInvoke.cvGetCol(image2, column, 0);
                Image<Gray, byte> xor_image2 = new Image<Gray, byte>(image2.Height, image2.Width);

                Rectangle rect2 = new Rectangle(1, 0, image2.Width, image2.Height);
                image2.ROI = rect2;



                Image<Gray, byte> gray2 = image2.Convert<Gray, byte>();
                Image<Gray, byte> gray_col = column.Convert<Gray, byte>();


                image2 = gray2.ConcateHorizontal(gray_col);
                CvInvoke.BitwiseXor(original, image2, xor_image2);
                Gray gray_sum2 = xor_image2.GetSum();
                double intensity2 = gray_sum2.Intensity;

                horizontal_values.Add(i, intensity2);
            }
            var min_vertical = vertical_values.Values.Min();
            var min_horizontal = horizontal_values.Values.Min();
            if (min_horizontal != 0)
            {
                Console.WriteLine("No complete Dark in Horizontal Direction");
            }
            if (min_vertical != 0)
            {
                Console.WriteLine("No complete Dark in Vertical Direction");
            }

            var dictval = from x in horizontal_values
                          where x.Value == min_horizontal
                          select x;

            foreach (KeyValuePair<int, double> values in dictval)
            {
                int key = values.Key;
                indices_horizontal.Add(key);
            }


            var dictval2 = from x in vertical_values
                           where x.Value == min_vertical
                           select x;

            foreach (KeyValuePair<int, double> values in dictval2)
            {
                int key = values.Key;
                indices_vertical.Add(key);
            }

        }


        static public void EdgeXoringImage(String path, out List<int> indices_horizontal, out List<int> indices_vertical)
        {

            indices_horizontal = new List<int>();
            indices_vertical = new List<int>();


            Image<Gray, byte> eimage = new Image<Gray, byte>(path);
            int height = eimage.Height;
            int width = eimage.Width;



            Image<Gray, byte> edge_image = new Image<Gray, byte>(eimage.Height, eimage.Width);
            CvInvoke.Canny(eimage, edge_image, 50, 100);
            eimage = edge_image;


            Image<Gray, byte> image2 = new Image<Gray, byte>(path);
            Image<Gray, byte> edge_image2 = new Image<Gray, byte>(image2.Height, image2.Width);
            CvInvoke.Canny(image2, edge_image2, 50, 100);
            image2 = edge_image2;

            Image<Gray, byte> original = new Image<Gray, byte>(path);
            Image<Gray, byte> edge_original = new Image<Gray, byte>(original.Height, original.Width);
            CvInvoke.Canny(original, edge_original, 50, 100);
            original = edge_original;
            




            Dictionary<int, double> horizontal_values = new Dictionary<int, double>();
            Dictionary<int, double> vertical_values = new Dictionary<int, double>();
            Dictionary<int, int> trial = new Dictionary<int, int>();




            int i = 1;
            for (i = 1; i < height; i++)
            {
                Image<Gray, byte> row = new Image<Gray, byte>(1, eimage.Width);
                CvInvoke.cvGetRow(eimage, row, 0);

                Image<Gray, byte> xor_image = new Image<Gray, byte>(eimage.Height, eimage.Width);

                Rectangle rect = new Rectangle(0, 1, eimage.Width, eimage.Height);
                eimage.ROI = rect;


                Image<Gray, byte> gray = eimage.Convert<Gray, byte>();
                Image<Gray, byte> gray_row = row.Convert<Gray, byte>();

                Console.Write(eimage.Height);
                Console.Write(eimage.Width);

                eimage = gray.ConcateVertical(gray_row);

                Console.WriteLine(eimage.Height);
                Console.WriteLine(eimage.Width);

                CvInvoke.BitwiseXor(original, eimage, xor_image);
                Gray gray_sum = xor_image.GetSum();
                double intensity = gray_sum.Intensity;



                vertical_values.Add(i, intensity);
            }

            for (i = 1; i < width; i++)
            {
                Image<Gray, byte> column = new Image<Gray, byte>(image2.Height, 1);
                CvInvoke.cvGetCol(image2, column, 0);
                Image<Gray, byte> xor_image2 = new Image<Gray, byte>(image2.Height, image2.Width);

                Rectangle rect2 = new Rectangle(1, 0, image2.Width, image2.Height);
                image2.ROI = rect2;



                Image<Gray, byte> gray2 = image2.Convert<Gray, byte>();
                Image<Gray, byte> gray_col = column.Convert<Gray, byte>();


                image2 = gray2.ConcateHorizontal(gray_col);
                CvInvoke.BitwiseXor(original, image2, xor_image2);
                Gray gray_sum2 = xor_image2.GetSum();
                double intensity2 = gray_sum2.Intensity;

                horizontal_values.Add(i, intensity2);
            }
            var min_vertical = vertical_values.Values.Min();
            var min_horizontal = horizontal_values.Values.Min();
            if (min_horizontal != 0)
            {
                Console.WriteLine("No complete Dark in Horizontal Direction");
            }
            if (min_vertical != 0)
            {
                Console.WriteLine("No complete Dark in Vertical Direction");
            }

            var dictval = from x in horizontal_values
                          where x.Value == min_horizontal
                          select x;

            foreach (KeyValuePair<int, double> values in dictval)
            {
                int key = values.Key;
                indices_horizontal.Add(key);
            }


            var dictval2 = from x in vertical_values
                           where x.Value == min_vertical
                           select x;

            foreach (KeyValuePair<int, double> values in dictval2)
            {
                int key = values.Key;
                indices_vertical.Add(key);
            }

        }
    }
}
