using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NumSharp.Core;


namespace ConsoleApp1
{
    class Program
    {
        public static DepthType DepthTypeCv { get; private set; }
        public static object Openfile { get; private set; }
        
        static void Main(string[] args)
        {

            Image<Gray, byte> image = new Image<Gray, byte>("D:/Work/correlation/Rtest/test (12).png");
            Image<Gray, byte> image2 = new Image<Gray, byte>("D:/Work/correlation/Rtest/test (12).png");
            Image<Gray, byte> original = new Image<Gray, byte>("D:/Work/correlation/Rtest/test (12).png");

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

                //String filename = "D:/Work/dither/images/Cs/";
                //String format = ".png";
                //filename = String.Concat(filename, i);
                //filename = String.Concat(filename, format);

                //CvInvoke.Imwrite(filename, xor_image);

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

                //String filename = "D:/Work/dither/images/Cs/v";
                //String format = ".png";
                //filename = String.Concat(filename, i);
                //filename = String.Concat(filename, format);

                //CvInvoke.Imwrite(filename, xor_image2);
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
            List<int> indices_horizontal = new List<int>();

            foreach (KeyValuePair<int, double> values in dictval)
            {
                int key = values.Key;
                indices_horizontal.Add(key);
            }

            Console.WriteLine("Horizontal Pattern");
            for (int j = 0; j < indices_horizontal.Count; j++)
                Console.WriteLine(indices_horizontal[j]);

            var dictval2 = from x in vertical_values
                          where x.Value == min_vertical
                          select x;
            List<int> indices_vertical = new List<int>();

            foreach (KeyValuePair<int, double> values in dictval2)
            {
                int key = values.Key;
                indices_vertical.Add(key);
            }
            Console.WriteLine("Vertical Pattern");
            for (int j = 0; j < indices_vertical.Count; j++)
                Console.WriteLine(indices_vertical[j]);


            Console.ReadLine();

        }

        //analysis the list returned
        private void CenterAnalysis(int darkInX, int darkInY, int width, int height, out int intersectionX, out int intersectionY)
        {
            intersectionX = 0;
            intersectionY = 0;

            if (darkInX != 0 && darkInY != 0)
            {
                if (darkInX >= (width / 2) - 2)
                {
                    Console.WriteLine("Horizontal Lining Pattern");

                    intersectionX = width / 2;
                    intersectionY = darkInY;
                }

                else if (darkInY >= (height / 2) - 2)
                {
                    Console.WriteLine("Vertical Lining Pattern");
                    intersectionX = darkInX;
                    intersectionY = height / 2;
                }

                else
                {
                    //Detect false negative patterns
                    if (((darkInX == 1 || darkInY == 2) && (darkInY == 1 || darkInY == 2)) || ((darkInX == width - 1) && darkInY == height - 1))
                    {
                        Console.WriteLine("Returning for Edge Detection");
                    }

                    //logic for complex horizontal pattern
                    else if ((darkInX == 1) && (darkInY != 1) && darkInY != height - 1)
                    {
                        intersectionX = width;
                        intersectionY = darkInY;
                    }

                    //logic for complex vertical pattern
                    else if ((darkInY == 1) && (darkInX != 1) && (darkInX != width - 1))
                    {
                        intersectionX = darkInX;
                        intersectionY = height;
                    }

                    //logic for simple pattern
                    else
                    {
                        intersectionX = darkInX;
                        intersectionY = darkInY;
                    }
                }

            }
            else
                Console.WriteLine("No repeated pattern detected at Level 1");
        }

    }
}
