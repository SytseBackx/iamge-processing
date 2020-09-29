﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        /*
         * loadButton_Click: process when user clicks "Load" button
         */
        private void loadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // open file dialog
            {
                string file = openImageDialog.FileName;                     // get the file name
                imageFileName.Text = file;                                  // show file name
                if (InputImage != null) InputImage.Dispose();               // reset image
                InputImage = new Bitmap(file);                              // create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0)
                    MessageBox.Show("Error in image dimensions (have to be > 0");
                else
                    pictureBox1.Image = (Image) InputImage;                 // display input image
            }
        }


        /*
         * applyButton_Click: process when user clicks "Apply" button
         */
        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            // ====================================================================
            // =================== YOUR FUNCTION CALLS GO HERE ====================
            // Alternatively you can create buttons to invoke certain functionality
            // ====================================================================

            byte[,] workingImage = convertToGrayscale(Image);          // convert image to grayscale
            byte[,] invertedImage = invertImage(workingImage);
            byte[,] contrastedImage = adjustContrast(workingImage);
            float[,] GaussianFilter = createGaussianFilter(5, 5);
            byte[,] FilteredImage = convolveImage(workingImage, GaussianFilter);
            byte[,] MedianFilter = medianFilter(workingImage, 5);
            //byte[,] ThresholdFilter = thresholdImage(workingImage);
            //float[,] horizontalKernal = new float[3, 1] { { -0.5f }, {0 }, {0.5f}};
            //float[,] verticalKernal = new float[1, 3] { { -0.5f ,  0,  0.5f} };
            //byte[,] EdgeMagnitudeImage = edgeMagnitude(workingImage, horizontalKernal, verticalKernal) ;
            //byte[,] pipelineB = thresholdImage(edgeMagnitude(convolveImage(workingImage,GaussianFilter),horizontalKernal,verticalKernal));
            //byte[,] pipelineC = thresholdImage(edgeMagnitude(medianFilter(workingImage, 5), horizontalKernal, verticalKernal));
            byte[,] strucElem = CreateStructuringElement("plus", 5);
            byte[,] erodedImage = CloseImage(workingImage, strucElem);

            // ==================== END OF YOUR FUNCTION CALLS ====================
            // ====================================================================

            // copy array to output Bitmap
            for (int x = 0; x < erodedImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < erodedImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(erodedImage[x, y], erodedImage[x, y], erodedImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }
            
            pictureBox2.Image = (Image)OutputImage;                         // display output image
        }


        /*
         * saveButton_Click: process when user clicks "Save" button
         */
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // save the output image
        }


        /*
         * convertToGrayScale: convert a three-channel color image to a single channel grayscale image
         * input:   inputImage          three-channel (Color) image
         * output:                      single-channel (byte) image
         */
        private byte[,] convertToGrayscale(Color[,] inputImage)
        {
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    Color pixelColor = inputImage[x, y];                    // get pixel color
                    byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                    tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 1 GO HERE ==============
        // ====================================================================

        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] invertImage(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte pixelColor = inputImage[x, y];                    // get pixel color
                    byte invertedColor = (byte)(~pixelColor);
                    tempImage[x,y] = invertedColor;
                }
            return tempImage;
        }


        /*
         * adjustContrast: create an image with the full range of intensity values used
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] adjustContrast(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            byte newmin = 0; byte newmax = 255;
            List<byte> colors = new List<byte>();
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte pixelColor = inputImage[x, y];                    // get pixel color
                    colors.Add(pixelColor);
                }

            byte min = colors.Min();
            byte max = colors.Max();

            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte pixelColor = inputImage[x, y];                    // get pixel color
                    tempImage[x,y] =Convert.ToByte((pixelColor - min) * ( (newmax - newmin) / (max - min)));
                }
            return tempImage;
        }


        /*
         * createGaussianFilter: create a Gaussian filter of specific square size and with a specified sigma
         * input:   size                length and width of the Gaussian filter (only odd sizes)
         *          sigma               standard deviation of the Gaussian distribution
         * output:                      Gaussian filter
         */
        private float[,] createGaussianFilter(byte size, float sigma)
        {
            // create temporary grayscale image
            float[,] filter = new float[size, size];

            double filterSum = 0;
            int offset = (size - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * sigma * sigma);
            for (int y = -offset; y <= offset; y++)
            {
                for (int x = -offset; x <= offset; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * sigma * sigma);
                    filter[y + offset, x + offset] = Convert.ToSingle( constant * Math.Exp(-distance));
                    filterSum += filter[y + offset, x + offset];
                }
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    filter[y, x] = Convert.ToSingle(filter[y, x] * 1d / filterSum);
                }
            }


            return filter;
        }


        /*
         * convolveImage: apply linear filtering of an input image
         * input:   inputImage          single-channel (byte) image
         *          filter              linear kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] convolveImage(byte[,] inputImage, float[,] filter)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            int kRows = filter.GetLength(0);
            int kCols = filter.GetLength(1);
            int filterRadiusX = kRows / 2;
            int filterRadiusY = kCols / 2;

            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    float pixelColor = 0;
                    for (int k = 0; k < kRows; k++)
                    {
                        int xoffset = filterRadiusX - k;
                        int xx = x + (xoffset);

                        for (int l = 0; l < kCols; l++)
                        {
                            int yoffset= filterRadiusY - l;

                            
                            int yy = y + (yoffset);

                            if (xx >= 0 && xx < InputImage.Size.Width && yy >= 0 && yy < InputImage.Size.Height)
                            {
                                float colorval = inputImage[xx, yy];
                                float scalar = filter[xoffset + filterRadiusX, yoffset + filterRadiusY];
                                pixelColor += (colorval * scalar);
                            }

                        }
                    }
                    tempImage[x,y] = Convert.ToByte(Math.Abs(pixelColor));

                }
            }
            return tempImage;
        }


        /*
         * medianFilter: apply median filtering on an input image with a kernel of specified size
         * input:   inputImage          single-channel (byte) image
         *          size                length/width of the median filter kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] medianFilter(byte[,] inputImage, byte size)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            List<int> temp = new List<int>();
            int radius = (size - 1) / 2;
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {

                    for (int k = -radius; k <= radius; k++)
                    {
                       
                        for (int l = -radius; l <= radius; l++)
                        {
                           

                            int xx = x + k;
                            int yy = y + l;

                            if (xx >= 0 && xx < InputImage.Size.Width && yy >= 0 && yy < InputImage.Size.Height)
                            {
                                temp.Add(inputImage[xx, yy]);
                            }
                        }
                    }
                    temp.Sort();
                    int median = (temp.Count - 1) / 2;
                    tempImage[x, y] = Convert.ToByte(temp[median]);
                    temp.Clear();

                }
            }
            return tempImage;
        }


        /*
         * edgeMagnitude: calculate the image derivative of an input image and a provided edge kernel
         * input:   inputImage          single-channel (byte) image
         *          horizontalKernel    horizontal edge kernel
         *          virticalKernel      vertical edge kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] edgeMagnitude(byte[,] inputImage, float[,] horizontalKernel, float[,] verticalKernel)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            byte[,] Dx = convolveImage(inputImage, horizontalKernel);
            byte[,] Dy = convolveImage(inputImage, verticalKernel);

            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte Dxval = Dx[x,y];
                    byte Dyval = Dy[x,y];
                    byte Magnitude = (byte)Math.Sqrt((Dxval * Dxval) + (Dyval * Dyval));
                    tempImage[x, y] = Magnitude;
                }
                

            return tempImage;
        }


        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image with on/off values
         */
        private byte[,] thresholdImage(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            byte threshold = 17;
            byte belowThreshold = 0;
            byte aboveThreshold = 255;
            // TODO: add your functionality and checks
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte pixelColor = inputImage[x, y];                    // get pixel color
                    if (pixelColor > threshold)
                    {
                        tempImage[x, y] = aboveThreshold;
                    }
                    else
                    {
                        tempImage[x, y] = belowThreshold;
                    }
                }

            return tempImage;
        }

        
        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 2 GO HERE ==============
        // ====================================================================

        private byte[,] CreateStructuringElement(string shape, int size)
        {
            byte[,] structuringElement = new byte[size, size];
            switch (shape)
            {
                case "plus":
                    int halfSize = size / 2;
                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++) {
                            if (x == halfSize || y == halfSize)
                            {
                                structuringElement[x, y] = 1;
                            }
                        }
                    }                     
                    break;
                case "square":
                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            structuringElement[x, y] = 1;
                        }
                    }
                    break;

            }
            return structuringElement;
                
        }

        private byte[,] ErodeImage(byte[,] inputImage,byte[,] structuringElement)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            int structuringElementWidth= structuringElement.GetLength(0);
            int structuringElementheight = structuringElement.GetLength(1);
            for (int x = 0; x < InputImage.Size.Width; x++) {                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte newVal = inputImage[x, y];
                    for (int k = 0; k < structuringElementWidth; k++)
                    {
                        int xoffset = (structuringElementWidth / 2) - k;
                        int xx = x + (xoffset);

                        for (int l = 0; l < structuringElementheight; l++)
                        {
                            int yoffset = (structuringElementheight / 2) - l;

                            int yy = y + (yoffset);

                            if (xx >= 0 && xx < InputImage.Size.Width && yy >= 0 && yy < InputImage.Size.Height && structuringElement[k, l] != 0)
                            {

                                if (inputImage[xx, yy] < newVal)
                                {
                                    newVal = inputImage[xx, yy];
                                }

                            }

                        }
                    }
                    tempImage[x, y] = newVal;
                }
            }
            return tempImage;
        }

        private byte[,] DilateImage(byte[,] inputImage, byte[,] structuringElement)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            int structuringElementWidth = structuringElement.GetLength(0);
            int structuringElementheight = structuringElement.GetLength(1);
            for (int x = 0; x < InputImage.Size.Width; x++)
                    {                 // loop over columns
                        for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                        {
                            byte newVal = inputImage[x, y];
                            for (int k = 0; k < structuringElementWidth; k++)
                            {
                                int xoffset = (structuringElementWidth / 2) - k;
                                int xx = x + (xoffset);

                                for (int l = 0; l < structuringElementheight; l++)
                                {
                                    int yoffset = (structuringElementheight / 2) - l;

                                    int yy = y + (yoffset);

                                    if (xx >= 0 && xx < InputImage.Size.Width && yy >= 0 && yy < InputImage.Size.Height && structuringElement[k, l] != 0)
                                    {

                                        if (inputImage[xx, yy] > newVal)
                                        {
                                            newVal = inputImage[xx, yy];
                                        }

                                    }

                                }
                            }
                            tempImage[x, y] = newVal;
                        }
                    }
                    return tempImage;
                
        }

        private byte[,] OpenImage(byte[,] inputImage, byte[,] SE)
        {

            return DilateImage(ErodeImage(inputImage, SE), SE);
        }

        private byte[,] CloseImage(byte[,] inputImage, byte[,] SE)
        {
            return ErodeImage(DilateImage(inputImage, SE), SE);
        }

        private byte[,] AndImages(byte[,] inputImage1, byte[,] inputImage2)
        {
            int inputWidth1 = inputImage1.GetLength(0);
            int inputWidth2 = inputImage2.GetLength(0);
            int tempWidth;
            if (inputWidth1 > inputWidth2)
            {
                tempWidth = inputWidth1;
            }
            else
            {
                tempWidth = inputWidth2;
            }
            int inputHeight1 = inputImage1.GetLength(1);
            int inputHeight2 = inputImage2.GetLength(1);
            int tempHeight;
            if (inputHeight1 > inputHeight2)
            {
                tempHeight = inputHeight1;
            }
            else
            {
                tempHeight = inputHeight2;
            }

            byte[,] tempImage = new byte[tempWidth,tempHeight];
            for (int x = 0; x < tempWidth; x++)                 // loop over columns
                for (int y = 0; y <tempHeight; y++)            // loop over rows
                {
                    if (inputImage1[x,y] == inputImage2[x, y])
                    {
                        tempImage[x, y] = inputImage1[x, y];
                    }
                    else
                    {
                        tempImage[x, y] = 0;
                    }
                }
            return tempImage;
        }
        private byte[,] OrImages(byte[,] inputImage1, byte[,] inputImage2)
        {
            int inputWidth1 = inputImage1.GetLength(0);
            int inputWidth2 = inputImage2.GetLength(0);
            int tempWidth;
            if (inputWidth1 > inputWidth2) 
            {
                tempWidth = inputWidth1; 
            }
            else
            {
                tempWidth = inputWidth2;
            }
            int inputHeight1 = inputImage1.GetLength(1);
            int inputHeight2 = inputImage2.GetLength(1);
            int tempHeight;
            if (inputHeight1 > inputHeight2)
            {
                tempHeight= inputHeight1;
            }
            else
            {
                tempHeight = inputHeight2;
            }

            byte[,] tempImage = new byte[tempWidth, tempHeight];
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    if (inputImage1[x, y] != 0 ||  inputImage2[x, y] != 0)
                    {
                        tempImage[x, y] = inputImage1[x, y];
                    }
                    else
                    {
                        tempImage[x, y] = 0;
                    }
                }
            return tempImage;
        }

        struct Histogram
        {
            public int values;
            public byte[] histogram;
        }

        private Histogram CountValues(byte[,] inputImage)
        {
            Histogram hist = new Histogram();
            byte[] histogram = new byte[256];                               //create an array that is the same size as all pixel values within range
            for (int i = 0; i < 256; i++)
                histogram[i] = 0;                                           //set each value to 0. at this moment each value has occurred 0 times.

            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {

                    byte value = tempImage[x, y];
                    histogram[value] = (byte)(histogram[value] + 1);
                }

            int indistinctValues = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] == 0)
                    indistinctValues += 1;
            }

            hist.values = indistinctValues;
            hist.histogram = histogram;

            return hist;
        }

        private byte[,] TraceBoundary(byte[,] inputImage)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {

                }
            return tempImage;
        }



        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 3 GO HERE ==============
        // ====================================================================

    }
}