using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Bicubic_Interpolation
{
    public class Bicubic
    {
        public Mat PixelMatrix;
        public Mat XDerivative;
        public Mat YDerivative;
        public Mat XYDerivative;
        public Mat Full_Matrix;
        public Mat Coefficients;
        public Bicubic()
        {
            double[,] a = new double[5, 5] { { 117, 112, 115, 114, 116},
                                            { 118, 111, 109, 110, 113 },
                                            { 120, 119, 117, 112, 110 },
                                            { 119, 120, 118, 116, 115 },
                                            {117, 116, 115, 116, 114 }};

            PixelMatrix = new Mat(5, 5, MatType.CV_64FC1, a);

            for (int i = 0; i < PixelMatrix.Cols; i++)
            {
                for (int j = 0; j < PixelMatrix.Rows; j++)
                {
                    Debug.Write(PixelMatrix.At<double>(i, j));
                    Debug.Write(" ");

                }
                Debug.Write(Environment.NewLine);
            }

            FindXDerivative();
            FindYDerivative();
            FindXYDerivative();
            //Concatenate(1,1);
            //Interpolate(0.5, 0.5);
            Enlarge(PixelMatrix, 2);

        }

        public Mat FindXDerivative()
        {
            XDerivative = new Mat(PixelMatrix.Size(), MatType.CV_64FC1);
            Mat kernel = new Mat(1, 3, MatType.CV_64FC1, new double[3] { 0.5, 0, -0.5 });
            Cv2.Filter2D(PixelMatrix, XDerivative, MatType.CV_64FC1, kernel);

            /// agjsdojgnkalsdngjsdkjgnasdljkngj

            //for (int i = 0; i < XDerivative.Cols; i++)
            //{
            //    for (int j = 0; j < XDerivative.Rows; j++)
            //    {
            //        Debug.Write(XDerivative.At<double>(i, j));
            //        Debug.Write(" ");
            //    }
            //    Debug.Write(Environment.NewLine);

            //}

            return XDerivative;
        }

        public Mat FindYDerivative()
        {
            YDerivative = new Mat(PixelMatrix.Size(), MatType.CV_64FC1);
            Mat kernel = new Mat(3, 1, MatType.CV_64FC1, new double[3] { 0.5, 0, -0.5 });
            Cv2.Filter2D(PixelMatrix, YDerivative, MatType.CV_64FC1, kernel);



            //for (int i = 0; i < YDerivative.Cols; i++)
            //{
            //    for (int j = 0; j < YDerivative.Rows; j++)
            //    {
            //        Debug.Write(YDerivative.At<double>(i, j));
            //        Debug.Write(" ");
            //    }
            //    Debug.Write(Environment.NewLine);

            //}

            return YDerivative;
        }

        public Mat FindXYDerivative()
        {
            XYDerivative = new Mat(PixelMatrix.Size(), MatType.CV_64FC1);
            Mat kernel = new Mat(1, 3, MatType.CV_64FC1, new double[3] { 0.5, 0, -0.5 });
            Cv2.Filter2D(YDerivative, XYDerivative, MatType.CV_64FC1, kernel);

            //for (int i = 0; i < XYDerivative.Cols; i++)
            //{
            //    for (int j = 0; j < XYDerivative.Rows; j++)
            //    {
            //        Debug.Write(XYDerivative.At<double>(i, j));
            //        Debug.Write(" ");
            //    }
            //    Debug.Write(Environment.NewLine);

            //}


            return XYDerivative;
        }

        public void Concatenate(int x,int y)
        {
            double[,] b = new double[4, 4] { { PixelMatrix.At<double>(x,y), PixelMatrix.At<double>(x,y+1), YDerivative.At<double>(x,y), YDerivative.At<double>(x,y+1)},
                                            { PixelMatrix.At<double>(x+1,y), PixelMatrix.At<double>(x+1,y+1), YDerivative.At<double>(x+1,y), YDerivative.At<double>(x+1,y+1) },
                                            { XDerivative.At<double>(x,y), XDerivative.At<double>(x,y+1), XYDerivative.At<double>(x,y), XYDerivative.At<double>(x,y+1) },
                                            {YDerivative.At<double>(x+1,y), YDerivative.At<double>(x+1,y+1), XYDerivative.At<double>(x+1,y+1), XYDerivative.At<double>(x+1,y+1)}};

            Full_Matrix = new Mat(4, 4, MatType.CV_64FC1, b);
            //for (int i = 0; i < Full_Matrix.Cols; i++)
            //{
            //    for (int j = 0; j < Full_Matrix.Rows; j++)
            //    {
            //        Debug.Write(Full_Matrix.At<double>(i, j));
            //        Debug.Write(" ");
            //    }
            //    Debug.Write(Environment.NewLine);

            //}
            double[,] c = new double[4, 4] { { 1, 0, 0, 0},
                                            { 0, 0, 1, 0},
                                            { -3, 3, -2, -1 },
                                            {2, -2, 1, 1}};

            Mat c_matrix = new Mat(4, 4, MatType.CV_64FC1, c);

            Coefficients = c_matrix * Full_Matrix * c_matrix.Transpose();

            Debug.Write(Environment.NewLine);
            //for (int i = 0; i < Coefficients.Cols; i++)
            //{

            //    for (int j = 0; j < Coefficients.Rows; j++)
            //    {
            //        Debug.Write(Coefficients.At<double>(i, j));
            //        Debug.Write(" ");
            //    }
            //    Debug.Write(Environment.NewLine);

            //}
        }
        public double Interpolate(double x, double y)
        {
            Mat x_matrix = new Mat(1, 4, MatType.CV_64FC1, new double[] { 1, x, Math.Pow(x, 2), Math.Pow(x, 3) });
            Mat y_matrix = new Mat(4, 1, MatType.CV_64FC1, new double[] { 1, y, Math.Pow(y, 2), Math.Pow(y, 3) });

            var p = (x_matrix * Coefficients * y_matrix);
            //Debug.Write(p.ToMat().At<double>(0,0));

            return p.ToMat().At<double>(0,0);
        }

        public Mat Enlarge(Mat startMatrix, int factor)
        {
            Mat resultantMatrix = new Mat(startMatrix.Size().Width * factor, startMatrix.Size().Height *factor, MatType.CV_64FC1);
            for (int i = 0; i < startMatrix.Cols - 1 * factor; i++)
            {
                for (int j = 0; j < startMatrix.Rows - 1 * factor; j++)
                {
                    if (i % factor == 0 && j % factor == 0) resultantMatrix.At<double>(i, j) = PixelMatrix.At<double>(i / factor, j / factor);
                    else
                    {
                        Concatenate((int)Math.Floor((double)(i/factor)), (int)Math.Floor((double)(j/factor)));
                        resultantMatrix.At<double>(i, j) = Interpolate(i/factor, j/factor);
                    }
                }
            }

            for (int i = 0; i < resultantMatrix.Cols; i++)
            {
                for (int j = 0; j < resultantMatrix.Rows; j++)
                {
                    Debug.Write(resultantMatrix.At<double>(i, j));
                    Debug.Write(" ");
                }
                Debug.Write(Environment.NewLine);

            }

            return resultantMatrix;



        }

    }
}
