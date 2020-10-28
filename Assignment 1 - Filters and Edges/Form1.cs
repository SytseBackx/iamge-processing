using System;
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
        private Bitmap Diamond;
        private Bitmap Club;
        private Bitmap Heart;
        private Bitmap Spade;
        private int Dir = 0;

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
                    pictureBox1.Image = (Image)InputImage;                 // display input image
            }
        }


        /*
         * applyButton_Click: process when user clicks "Apply" button
         */
        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image


            Image club = System.Drawing.Image.FromFile("..\\..\\suit_symbols\\Club.png");
            Image diamond = System.Drawing.Image.FromFile("..\\..\\suit_symbols\\Diamond.png");
            Image heart = System.Drawing.Image.FromFile("..\\..\\suit_symbols\\Heart.png");
            Image spade = System.Drawing.Image.FromFile("..\\..\\suit_symbols\\Spade.png");

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
            //byte[,] invertedImage = invertImage(workingImage);
            //byte[,] contrastedImage = adjustContrast(workingImage);
            float[,] GaussianFilter = createGaussianFilter(9, 1);
            //byte[,] FilteredImage = convolveImage(workingImage, GaussianFilter);
            //byte[,] MedianFilter = medianFilter(workingImage, 5);
            //byte[,] ThresholdFilter = thresholdImage(workingImage);
            float[,] horizontalKernal = new float[3, 1] { { -0.5f }, { 0 }, { 0.5f } };
            float[,] verticalKernal = new float[1, 3] { { -0.5f, 0, 0.5f } };
            //byte[,] EdgeMagnitudeImage = edgeMagnitude(workingImage, horizontalKernal, verticalKernal) ;
            //byte[,] pipelineB = thresholdImage(edgeMagnitude(convolveImage(workingImage,GaussianFilter),horizontalKernal,verticalKernal));
            //byte[,] pipelineC = thresholdImage(edgeMagnitude(medianFilter(workingImage, 5), horizontalKernal, verticalKernal));
            //byte[,] strucElem = CreateStructuringElement("plus", 3);
            //byte[,] dilatedImage = DilateImage(workingImage, strucElem);
            //byte[,] erodedImage = CloseImage(workingImage, strucElem);
            byte[,] binaryImage = CreateBinary(invertImage(workingImage));
            //List<Point> points = TraceBoundary(binaryImage,strucElem);
            //byte[,] bound = FillImageFromList(workingImage,points);
            //byte[,] openImage = invertImage(OpenImage(invertImage(workingImage), strucElem));
            //int[,] test = HoughPeakFinding(rthetaImage, 10);
            List<Point> points = HoughPeakFinding(HoughTransform(workingImage), 1185);//1195);
            for (int i = 0; i < points.Count; i++)
                Console.WriteLine(points[i]);
            //int[,] rthetaImage = HoughTransform(FillImageFromList(workingImage, points));
            Bitmap hough = InputImage;
            for (int i = 0; i < points.Count; i++)
            {
                List<Point> p = HoughLineDetection(binaryImage, points[i], 0, 2000, 2500, "binary");
                hough = HoughVisualisation(hough, p);
            }

            //byte[,] cornersDetected = cornerDetection(workingImage,verticalKernal,horizontalKernal);
            //byte[,] output = SIFT(workingImage);
            //Histogram values = CountValues(workingImage);
            //Color[,] hImage = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)
            //for (int x = 0; x < hough.Size.Width; x++)                 // loop over columns
            //    for (int y = 0; y < hough.Size.Height; y++)            // loop over rows
            //        hImage[x, y] = hough.GetPixel(x, y);                // set pixel color in array at (x,y)
            //byte[,] output = //convertToGrayscale(hImage);

            // ==================== END OF YOUR FUNCTION CALLS ====================
            // ====================================================================

            OutputImage = (Bitmap)heart;//new Bitmap(output.GetLength(0), output.GetLength(1)); // create new output image
            /*/ copy array to output Bitmap
            for (int x = 0; x < output.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < output.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(output[x, y], output[x, y], output[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }*/

            pictureBox2.Image = OutputImage;                         // display output image

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
                    tempImage[x, y] = invertedColor;
                }
            return tempImage;
        }

        private byte[,] visualiseBinary(byte[,] inputImage)
        {
            byte[,] test = inputImage;
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    if (test[x, y] == 1)
                        test[x, y] = 255;
                    else
                        test[x, y] = 0;
                }
            return test;
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
                    tempImage[x, y] = Convert.ToByte((pixelColor - min) * ((newmax - newmin) / (max - min)));
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
                    filter[y + offset, x + offset] = Convert.ToSingle(constant * Math.Exp(-distance));
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
                            int yoffset = filterRadiusY - l;


                            int yy = y + (yoffset);

                            if (xx >= 0 && xx < inputImage.GetLength(0) && yy >= 0 && yy < inputImage.GetLength(1))
                            {
                                float colorval = inputImage[xx, yy];
                                float scalar = filter[xoffset + filterRadiusX, yoffset + filterRadiusY];
                                pixelColor += (colorval * scalar);
                            }

                        }
                    }
                    tempImage[x, y] = Convert.ToByte(Math.Abs(pixelColor));

                }
            }
            return tempImage;
        }

        private int[,] convolveImageInt(int[,] inputImage, float[,] filter)
        {
            // create temporary grayscale image
            int[,] tempImage = new int[inputImage.GetLength(0), inputImage.GetLength(1)];

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
                            int yoffset = filterRadiusY - l;


                            int yy = y + (yoffset);

                            if (xx >= 0 && xx < inputImage.GetLength(0) && yy >= 0 && yy < inputImage.GetLength(1))
                            {
                                float colorval = inputImage[xx, yy];
                                float scalar = filter[xoffset + filterRadiusX, yoffset + filterRadiusY];
                                pixelColor += (colorval * scalar);
                            }

                        }
                    }
                    tempImage[x, y] = Convert.ToByte(Math.Abs(pixelColor));

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
                    byte Dxval = Dx[x, y];
                    byte Dyval = Dy[x, y];
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
        private byte[,] thresholdImage(byte[,] inputImage, byte threshold)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            byte belowThreshold = 0;
            byte aboveThreshold = 255;
            // TODO: add your functionality and checks
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
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
                        for (int y = 0; y < size; y++)
                        {
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

        private byte[,] ErodeImage(byte[,] inputImage, byte[,] structuringElement)
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

            byte[,] tempImage = new byte[tempWidth, tempHeight];
            for (int x = 0; x < tempWidth; x++)                 // loop over columns
                for (int y = 0; y < tempHeight; y++)            // loop over rows
                {
                    if (inputImage1[x, y] == inputImage2[x, y])
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
                tempHeight = inputHeight1;
            }
            else
            {
                tempHeight = inputHeight2;
            }

            byte[,] tempImage = new byte[tempWidth, tempHeight];
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    if (inputImage1[x, y] != 0 || inputImage2[x, y] != 0)
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

            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {

                    byte value = inputImage[x, y];
                    histogram[value] = (byte)(histogram[value] + 1);
                }

            int indistinctValues = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] != 0)
                    indistinctValues += 1;
            }

            hist.values = indistinctValues;
            hist.histogram = histogram;
            Console.WriteLine(hist.values);

            return hist;
        }

        private byte[,] CreateBinary(byte[,] inputImage)
        {
            byte[,] tempImage = inputImage;
            for (int x = 0; x < InputImage.Size.Width; x++)
            {                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)
                {             // loop over rows
                    if (inputImage[x, y] > 127)
                    {
                        tempImage[x, y] = 1;

                    }
                    else
                    {
                        tempImage[x, y] = 0;
                    }
                }
            }
            return tempImage;
        }

        private List<Point> TraceBoundary(byte[,] inputImage, byte[,] strucElem)

        {
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if (inputImage[x, y] == 0 || inputImage[x, y] == 1)
                    {

                    }
                    else
                    {
                        Console.WriteLine("non binary image in the traceboundary");
                    }
                }
            }
            List<Point> emptyList = new List<Point>();
            List<Point> boundary = emptyList;
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {             // loop over rows
                    int value = inputImage[x, y];
                    if (value == 1)
                    {
                        boundary = WalkBoundary(inputImage, x, y, strucElem);
                        break;
                    }
                }
                if (boundary != emptyList)
                {
                    break;
                }
            }
            return boundary;
        }
        private List<Point> WalkBoundary(byte[,] inputImage, int xS, int yS, byte[,] strucElem)
        {
            List<Point> boundary = new List<Point>();
            int xT, yT;//T= successor of starting point (xS,yS)
            int xP, yP;//P= previous contour point
            int xC, yC;//C= current contour point

            Point pt = new Point(xS, yS);
            boundary.Add(pt);
            Point ptN = findNextPoint(inputImage, pt, strucElem);
            int xN = ptN.X, yN = ptN.Y; //N = new contour point
            xC = xT = xN;
            yC = yT = yN;
            Boolean done = (pt == ptN); // true if isolated pixel
            while (!done)
            {
                pt = new Point(xC, yC);
                Dir += 6;
                Dir %= 8;
                ptN = findNextPoint(inputImage, pt, strucElem);

                xP = xC; yP = yC;
                xC = ptN.X; yC = ptN.Y;
                // are we back at the starting position?
                done = (xP == xS && yP == yS && xC == xT && yC == yT);
                if (!done)
                {
                    boundary.Add(pt);
                }
            }
            return boundary;

        }
        Point findNextPoint(byte[,] inputImage, Point pt, byte[,] strucElem)
        {
            int structuringElementWidth = strucElem.GetLength(0);
            int structuringElementheight = strucElem.GetLength(1);

            int[,] delta = new int[,] {
                { 1,0}, { 1, 1}, {0, 1}, {-1, 1},
                {-1,0}, {-1,-1}, {0,-1}, { 1,-1}};


            for (int i = 0; i < 7; i++)
            {
                int x = pt.X + delta[Dir, 0];
                int y = pt.Y + delta[Dir, 1];
                if (x >= 0 && x < InputImage.Size.Width && y >= 0 && y < InputImage.Size.Height)
                {
                    if (inputImage[x, y] == 0)
                    {
                        Dir = (Dir + 1) % 8;
                    }
                    else
                    {// found a nonbackground pixel

                        for (int k = 0; k < structuringElementWidth; k++)
                        {
                            int xoffset = (structuringElementWidth / 2) - k;
                            int xx = x + (xoffset);

                            for (int l = 0; l < structuringElementheight; l++)
                            {
                                int yoffset = (structuringElementheight / 2) - l;

                                int yy = y + (yoffset);

                                if (xx >= 0 && xx < InputImage.Size.Width && yy >= 0 && yy < InputImage.Size.Height && strucElem[k, l] != 0)
                                {
                                    if (inputImage[xx, yy] == 0)//the non background pixel has a neighbour which is background, therefore it is an egde pixel
                                    {
                                        return new Point(x, y);
                                    }

                                }


                            }
                        }
                        Dir = (Dir + 1) % 8;
                    }
                }

            }
            return pt;
        }

        private byte[,] FillImageFromList(byte[,] inputImage, List<Point> points) //used to visualise the outline obtained from TraceBoundary
        {
            byte[,] newImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            foreach (Point p in points)
            {
                newImage[p.X, p.Y] = 255;
            }
            return newImage;
        }



        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 3 GO HERE ==============
        // ====================================================================

        //The input image should be a binary image in which the edge values are 255
        private int[,] HoughTransform(byte[,] inputImage)
        {
            int thetaMax = 360; // maximum value of theta in degrees
            int diagonalSize = (int)Math.Sqrt((inputImage.GetLength(0) * inputImage.GetLength(0)) + (inputImage.GetLength(1) * inputImage.GetLength(1)));
            int[,] rThetaImage = new int[thetaMax, diagonalSize * 2];

            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
            {
                for (int y = 0; y < InputImage.Size.Height; y++) // loop over rows
                {
                    //checks if the pixel is an edge pixel
                    if (inputImage[x, y] == 255)
                    {
                        for (int theta = 0; theta < thetaMax; theta++) // loop over angle values
                        {
                            float thetaRadians = theta * (float)Math.PI / 180;
                            int r = (int)((x * Math.Cos(thetaRadians)) + ((inputImage.GetLength(1) - y) * Math.Sin(thetaRadians)));
                            rThetaImage[theta, r + (diagonalSize)] += 1;

                        }

                    }

                }
            }
            return rThetaImage;
        }

        //the implementation is option B
        private List<Point> HoughPeakFinding(int[,] rthetaImage, int threshold)
        {
            int[,] temp = rthetaImage;
            List<Point> peaks = new List<Point>();
            for (int x = 0; x < rthetaImage.GetLength(0); x++) // loop over columns
            {
                for (int y = 0; y < rthetaImage.GetLength(1); y++) // loop over rows
                {
                    int v = rthetaImage[x, y];
                    for (int k = 0; k < 0; k++)
                    {
                        int xx = x + k - 1;
                        if (!(xx < 0 || xx >= rthetaImage.GetLength(0))) {
                            for (int l = 0; l < 0; l++)
                            {
                                int yy = y + l - 1;
                                if (!(yy < 0 || yy >= rthetaImage.GetLength(0)))
                                {
                                    if (rthetaImage[xx, yy] > v)
                                    {
                                        temp[x, y] = 0;
                                    }
                                }
                            }
                        }
                    }

                    if (temp[x, y] > threshold)
                    {
                        peaks.Add(new Point(x, y));
                    }
                }

            }

            return peaks;
        }

        private List<Point> HoughLineDetection(byte[,] inputImage, Point punt, byte minIntensityThresh, int minLength, int maxGap, string type) //voeg pair toe ipv floats
        {
            float theta = punt.X;
            float r = punt.Y;
            List<Point> coordinates = new List<Point>();
            List<Point> line = new List<Point>();
            List<Point> segments = new List<Point>();
            int threshold;
            //voor binary is de threshold 1, voor grayscale is het de gegeven threshold
            if (type == "binary")
                threshold = 1;
            else
                threshold = minIntensityThresh;

            //voeg alleen de pixels toe die aan zijn
            for (int x = 0; x < inputImage.GetLength(0); x++) // loop over columns
            {
                for (int y = 0; y < inputImage.GetLength(1); y++) // loop over rows
                {
                    float ycalc = getY(x, theta, r);
                    //als de berekende waarde van y op de lijn binnen de foto ligt en boven de threshold is, is wordt hij toegevoegd aan de lijst van punten op deze lijn.
                    if (y <= inputImage.GetLength(1) && y >= 0 && inputImage[x, y] >= threshold)
                    {
                        Point a = new Point(x, (int)ycalc);
                        coordinates.Add(a);
                    }
                }
            }

            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                //controleer of de segmentlength tussen twee punten kleiner is dan de max gap. als dit zo is, wordt het aan een lijn gezien en toegevoegd aan de coordinaten lijst.
                //als dit niet zo is, wordt de lijn toegevoegd aan de lijst van segmenten, mits hij langer is dan de minimum length.
                {
                    if ((int)segmentLength(coordinates[i].X, coordinates[i].Y, coordinates[i + 1].X, coordinates[i + 1].Y) < maxGap)
                        line.Add(coordinates[i]);
                    else
                    {
                        line.Add(coordinates[i]);
                        if (segmentLength(coordinates[0].X, coordinates[0].Y, coordinates[coordinates.Count - 1].X, coordinates[coordinates.Count - 1].Y) >= (float)minLength)
                        {
                            Point b = new Point(coordinates[0].X, coordinates[0].Y);
                            Point c = new Point(coordinates[coordinates.Count - 1].X, coordinates[coordinates.Count - 1].Y);
                            segments.Add(b);
                            segments.Add(c);
                            coordinates.Clear();
                        }
                        else
                            coordinates.Clear();
                    }
                }

            }

            return segments;
        }

        float getY(float x, float th, float r)
        {
            float theta = th * (float)Math.PI / 180;
            float y = (float)((-Math.Cos(theta) / Math.Sin(theta)) * x + (r / Math.Sin(theta)));
            //float y = (float)(r - x * Math.Cos(theta)) / (float)(Math.Sin(theta));
            return y;
        }

        float segmentLength(float x1, float y1, float x2, float y2)
        {
            float x = (x2 - x1) * (x2 - x1);
            float y = (y2 - y1) * (y2 - y1);
            float d = (float)Math.Sqrt(x + y);
            return d;
        }

        Bitmap HoughVisualisation(Bitmap inputImage, List<Point> segments)
        {
            Pen linePen = new Pen(Color.Red, 3);

            for (int i = 0; i < segments.Count; i += 2)
            {
                using (var g = Graphics.FromImage(inputImage))
                {
                    g.DrawLine(linePen, segments[i], segments[i + 1]);
                }
            }
            return inputImage;

        }


        //theta vvalues are in degrees
        private int[,] HoughTransformAngleLimits(byte[,] inputImage, int lowerTheta, int maxTheta)
        {
            int diagonalSize = (int)Math.Sqrt((inputImage.GetLength(0) * inputImage.GetLength(0)) + (inputImage.GetLength(1) * inputImage.GetLength(1)));
            int[,] rThetaImage = new int[maxTheta - lowerTheta, diagonalSize * 2];

            for (int x = 0; x < InputImage.Size.Width; x++) // loop over columns
            {
                for (int y = 0; y < InputImage.Size.Height; y++) // loop over rows
                {
                    //checks if the pixel is an edge pixel
                    if (inputImage[x, y] == 255)
                    {
                        for (int theta = lowerTheta; theta < maxTheta; theta++) // loop over angle values
                        {
                            float thetaRadians = theta * (float)Math.PI / 180;
                            int r = (int)((x * Math.Cos(thetaRadians)) + ((inputImage.GetLength(1) - y) * Math.Sin(thetaRadians)));
                            rThetaImage[theta, r + (diagonalSize)] += 1;

                        }

                    }

                }
            }
            return rThetaImage;
        }

        List<Point> cornerDetection(byte[,] inputImage, float[,] verticalKernel, float[,] horizontalKernel)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            float[,] gausFilter = createGaussianFilter(5, 5);

            byte[,] Dx = convolveImage(convolveImage(inputImage, horizontalKernel), gausFilter);
            byte[,] Dy = convolveImage(convolveImage(inputImage, verticalKernel), gausFilter);

            float alpha = 0.05f;
            int threshold = 5000;
            int minDBetweenCorners = 50;
            List<Tuple<float, Point>> tempCorners = new List<Tuple<float, Point>>();
            List<Point> corners = new List<Point>();

            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    byte Dxval = Dx[x, y];
                    byte Dyval = Dy[x, y];
                    float A = Dxval * Dxval;
                    float B = Dyval * Dyval;
                    float C = Dyval * Dyval;

                    float[,] M = new float[,] { { A, C }, { C, B } };

                    float Q = (A * B - C * C) - alpha * (A + B) * (A + B);
                    if (Q > threshold)
                    {
                        tempCorners.Add(new Tuple<float, Point>(Q, new Point(x, y)));
                    }

                }
            for (int i = 0; i < tempCorners.Count; i++)
            {
                Point p1 = tempCorners[i].Item2;
                for (int j = i + 1; j < tempCorners.Count; j++)
                {
                    Point p2 = tempCorners[j].Item2;
                    double d = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y));
                    if (d < minDBetweenCorners)
                    {
                        tempCorners.RemoveAt(j);
                        tempImage[p1.X, p1.Y] = 255;
                    }
                }
                corners.Add(p1);
            }

            return corners;
        }

        private byte[,] SIFT(byte[,] inputImage)
        {
            float k = (float)Math.Sqrt(2);
            byte size = 17;
            int[,][,] HSS = CreateHierarchicalScaleSpace(inputImage, size, k);

            List<Tuple<int, int, int, int>> keyPoints = SIFTFindKeypoints(HSS, 10, inputImage);

            List<Tuple<int, int, int, int>> keyPointsdir = DetermineKeyPointDirection(keyPoints, inputImage, k, size);

            List<Tuple<int, int, int, int>> teset = keyPoints;

            byte[,] tempImage = inputImage;

            return tempImage;
        }

        //assumes the inputimages are the same size, will throw index out of range error if the second image is larger
        private int[,] ImageDif(byte[,] inputImage1, byte[,] inputImage2)
        {

            int[,] tempImage = new int[inputImage1.GetLength(0), inputImage1.GetLength(1)];

            for (int x = 0; x < inputImage1.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage1.GetLength(1); y++)
                {
                    tempImage[x, y] = (inputImage1[x, y] - inputImage2[x, y]);
                }
            }
            return tempImage;
        }

        private int[,][,] CreateHierarchicalScaleSpace(byte[,] inputImage, byte size, float k)
        {
            float sig1 = k;
            float sig2 = sig1 * k;
            float sig3 = sig2 * k;
            float sig4 = sig3 * k;
            float sig5 = sig4 * k; //just used when constructing gausImage42
            float[,] gausFil0 = createGaussianFilter(size, 1); //meant to calculate gausImage(x,-1) which would have a sigma of k/k = 1
            float[,] gausFil1 = createGaussianFilter(size, sig1);
            float[,] gausFil2 = createGaussianFilter(size, sig2);
            float[,] gausFil3 = createGaussianFilter(size, sig3);
            float[,] gausFil4 = createGaussianFilter(size, sig4);
            float[,] gausFil5 = createGaussianFilter(size, sig5);

            byte[,] gausImageneg10 = convolveImage(inputImage, gausFil0);
            byte[,] gausImage00 = convolveImage(inputImage, gausFil1);
            byte[,] gausImage10 = convolveImage(inputImage, gausFil2);
            byte[,] gausImage20 = convolveImage(inputImage, gausFil3);
            byte[,] gausImage30 = convolveImage(inputImage, gausFil4);
            byte[,] gausImage40 = convolveImage(inputImage, gausFil5);

            int[,] D0neg1 = ImageDif(gausImageneg10, gausImage00);
            int[,] D00 = ImageDif(gausImage00, gausImage10);
            int[,] D01 = ImageDif(gausImage10, gausImage20);
            int[,] D02 = ImageDif(gausImage20, gausImage30);
            int[,] D03 = ImageDif(gausImage20, gausImage30);

            byte[,] halfImage = new byte[inputImage.GetLength(0) / 2, inputImage.GetLength(1) / 2];
            byte[,] quarterImage = new byte[inputImage.GetLength(0) / 4, inputImage.GetLength(1) / 4];
            for (int x = 0; x < inputImage.GetLength(0); x += 2)
            {
                for (int y = 0; y < inputImage.GetLength(1); y += 2)
                {
                    halfImage[x / 2, y / 2] = gausImage30[x, y];
                }
            }

            byte[,] gausImageneg11 = convolveImage(halfImage, gausFil0);
            byte[,] gausImage01 = convolveImage(halfImage, gausFil1);
            byte[,] gausImage11 = convolveImage(halfImage, gausFil2);
            byte[,] gausImage21 = convolveImage(halfImage, gausFil3);
            byte[,] gausImage31 = convolveImage(halfImage, gausFil4);
            byte[,] gausImage41 = convolveImage(halfImage, gausFil5);

            int[,] D1neg1 = ImageDif(gausImageneg11, gausImage01);
            int[,] D10 = ImageDif(gausImage01, gausImage11);
            int[,] D11 = ImageDif(gausImage11, gausImage21);
            int[,] D12 = ImageDif(gausImage21, gausImage31);
            int[,] D13 = ImageDif(gausImage31, gausImage41);


            for (int x = 0; x < halfImage.GetLength(0); x += 2)
            {
                for (int y = 0; y < halfImage.GetLength(1); y += 2)
                {
                    quarterImage[x / 2, y / 2] = gausImage31[x, y];
                }
            }

            byte[,] gausImageneg12 = convolveImage(quarterImage, gausFil0);
            byte[,] gausImage02 = convolveImage(quarterImage, gausFil1);
            byte[,] gausImage12 = convolveImage(quarterImage, gausFil2);
            byte[,] gausImage22 = convolveImage(quarterImage, gausFil3);
            byte[,] gausImage32 = convolveImage(quarterImage, gausFil4);
            byte[,] gausImage42 = convolveImage(quarterImage, gausFil5);

            int[,] D2neg1 = ImageDif(gausImageneg12, gausImage02);
            int[,] D20 = ImageDif(gausImage02, gausImage12);
            int[,] D21 = ImageDif(gausImage12, gausImage22);
            int[,] D22 = ImageDif(gausImage22, gausImage32);
            int[,] D23 = ImageDif(gausImage32, gausImage42);

            int[,][,] scaleSpace = new int[,][,] { { D0neg1, D00, D01, D02, D03 }, { D1neg1, D10, D11, D12, D13 }, { D2neg1, D20, D21, D22, D23 } };

            return scaleSpace;
        }

        /*byte[,] DoubleSize(byte[,] inputImage)
        {
            byte[,] doubled = new byte[inputImage.GetLength(0) * 2, inputImage.GetLength(1) * 2];
            for (int x = 0; x < inputImage.GetLength(0); x ++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y ++)
                {
                   doubled[x * 2, y * 2] = inputImage[x, y];
                   doubled[x * 2 + 1, y * 2] = inputImage[x, y];
                   doubled[x * 2, y * 2 + 1] = inputImage[x, y];
                   doubled[x * 2 + 1, y * 2 + 1] = inputImage[x, y];
                }
            }
            return doubled;
        }*/

        private List<Tuple<int, int, int, int>> SIFTFindKeypoints(int[,][,] hss, int threshold, byte[,] inputImage)
        {
            List<Tuple<int,int,int,int>> tempKeyPoints = new List<Tuple<int, int, int, int>>();
            List<Tuple<int, int, int, int>> keyPoints = new List<Tuple<int, int, int, int>>();

            /////////////finds all local max and mins/////////////////////
            for (int i = 1; i < hss.GetLength(0) - 1; i++) //loops over octaves
            {
                for (int j = 1; j < 4; j++) //loops over the images withing the octave
                {
                    int[,] currentImage = hss[i, j];
                    for (int x = 0; x < currentImage.GetLength(0); x++)
                    {
                        for (int y = 0; y < currentImage.GetLength(1); y++) //each pixel withing the image
                        {
                            int max = hss[i, j][x, y];
                            int min = hss[i, j][x, y];
                            for (int k = 0; k < 3; k++) // loops surrounding collumns
                            {
                                int xx = x + k - 1;
                                if (!(xx < 0 || xx >= currentImage.GetLength(0)))
                                {
                                    for (int l = 0; l < 3; l++) // loops rows
                                    {
                                        int yy = y + l - 1;
                                        if (!(yy < 0 || yy >= currentImage.GetLength(1)))
                                        {

                                            for (int n = -1; n < 2; n++) //don't need to check whether it is out of bounds since it never will, loop 3rd dimension
                                            {

                                                if (hss[i, j + n][xx, yy] > max)
                                                {
                                                    max = hss[i, j + n][xx, yy];
                                                }

                                                if (hss[i, j + n][xx, yy] < min)
                                                {
                                                    min = hss[i, j + n][xx, yy];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if ((max == hss[i, j][x, y] || min == hss[i, j][x, y]) && Math.Abs(hss[i, j][x, y]) > threshold)
                            {
                                tempKeyPoints.Add(new Tuple<int, int, int, int>(i,j,x,y));
                            }
                        }
                    }
                }
            }

            ///////fit function with taylors series/////////

            foreach (Tuple<int, int, int, int> candidate in tempKeyPoints)
            {

            }

            ///////////remove edge candidates//////////////
            float[,] horizontalKernal = new float[3, 1] { { -0.5f }, { 0 }, { 0.5f } };
            float[,] verticalKernal = new float[1, 3] { { -0.5f, 0, 0.5f } };
            float alpha = 0.05f;

            int cornerThreshold = 1;
            foreach (Tuple<int, int, int, int> candidate in tempKeyPoints)
            {
                int i = candidate.Item1;
                int j = candidate.Item2;
                int[,] Dx = convolveImageInt(hss[i,j], horizontalKernal);
                int[,] Dy = convolveImageInt(hss[i, j], verticalKernal);
                int x = candidate.Item3;
                int y = candidate.Item4;
                int Dxval = Dx[x, y];
                int Dyval = Dy[x, y];
                float A = Dxval * Dxval;
                float B = Dyval * Dyval;
                float C = Dyval * Dyval;

                float Q = (A * B - C * C) - alpha * (A + B) * (A + B);
                if (Q > cornerThreshold)
                {
                     keyPoints.Add(candidate);
                }


            }

            return keyPoints;
        }

        private List<Tuple<int, int, int, int>> DetermineKeyPointDirection(List<Tuple<int, int, int, int>> keypoints, byte[,] inputImage, float k, byte size)
        {
            foreach (Tuple<int, int, int, int> keyPoint in keypoints)
            {
                int i = keyPoint.Item1;
                int j = keyPoint.Item2;
                float sigma = (float)Math.Pow(k,j) * (2 ^ (i-1)); 
                byte[,] L = convolveImage(inputImage, createGaussianFilter(size, sigma));
                int[] directionHistogram = new int[36];
                for (int x = 0; x < L.GetLength(0); x++) // loop over columns
                {
                    for (int y = 0; y < L.GetLength(1); y++) // loop over rows
                    {
                        if (!(x == 0 || x >= L.GetLength(0) || y == 0 || y >= L.GetLength(1)))
                        {
                            float magnitude = (float)Math.Sqrt((L[x + 1,y] - L[x - 1,y]) ^ 2 + (L[x,y + 1] - L[x,y - 1]) ^ 2);
                            int theta = (int)(Math.Atan2(L[x,y + 1] - L[x,y - 1], L[x + 1, y] - L[x, y - 1]) * 180 / Math.PI) ;
                            int distanceFromLowerVal = theta % 10;
                            directionHistogram[(theta - distanceFromLowerVal) / 10] +=  (int)(magnitude * ((10f - distanceFromLowerVal) / 10));
                            directionHistogram[(theta - distanceFromLowerVal) / 10 + 1] += (int)(magnitude * (distanceFromLowerVal) / 10);
                        }
                        
                    }

                }
                //////smooth out histogram////////

            }
            return keypoints;
        }

        private float[,] normalizeFilter(float[,] filter)
        {
            float[,] normalized = filter;
            float sum = 0;
            foreach(float val in filter)
            {
                sum += Math.Abs(val);
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    normalized[i,j] = filter[i,j] / sum;
                }
            }
            return normalized;
        }

        private List<Point> SIFTDescriptor(byte[,] inputImage, Tuple<int, int, int, int> keypoint, int scale, float angle, float k, byte size)
        {
            int half = scale / 2;
            int segment = half / 2;
            Point punt = new Point(keypoint.Item3, keypoint.Item4);
            Point center = ConvertSystem(angle, punt);
            Point LB = PointOnLine(angle + 90, PointOnLine(angle, center, -half), -half);
            Point LM = PointOnLine(angle, center, -half);
            Point LO = PointOnLine(angle + 90, PointOnLine(angle, center, -half), half);

            //histogram in progress
            int i = keypoint.Item1;
            int j = keypoint.Item2;
            float sigma = (float)Math.Pow(k, j) * (2 ^ (i - 1));
            byte[,] L = convolveImage(inputImage, createGaussianFilter(size, sigma));
            int[] directionHistogram = new int[36];
            for (int x = 0; x < L.GetLength(0); x++) // loop over columns
            {
                for (int y = 0; y < L.GetLength(1); y++) // loop over rows
                {
                    if (!(x == 0 || x >= L.GetLength(0) || y == 0 || y >= L.GetLength(1)))
                    {
                        float magnitude = (float)Math.Sqrt((L[x + 1, y] - L[x - 1, y]) ^ 2 + (L[x, y + 1] - L[x, y - 1]) ^ 2);
                        int theta = (int)(Math.Atan2(L[x, y + 1] - L[x, y - 1], L[x + 1, y] - L[x, y - 1]) * 180 / Math.PI);
                        int distanceFromLowerVal = theta % 10;
                        directionHistogram[(theta - distanceFromLowerVal) / 10] += (int)(magnitude * ((10f - distanceFromLowerVal) / 10));
                        directionHistogram[(theta - distanceFromLowerVal) / 10 + 1] += (int)(magnitude * (distanceFromLowerVal) / 10);
                    }

                }

            }
        }

        Point ConvertSystem(float angle, Point p)
        {
            //convert the cartesian coordinate s
            double radian = angle * (Math.PI / 180);
            float newX = (float)Math.Cos(radian) * p.X - (float)Math.Sin(radian) * p.Y;
            float newY = (float)Math.Sin(radian) * p.X + p.Y * (float)Math.Cos(radian);
            Point coordinate = new Point((int)newX, (int)newY);
            return coordinate;
        }
        Point PointOnLine(float angle, Point p, int dist)
        {
            double radian = angle * (Math.PI / 180);
            Point c = new Point(p.X + dist * (int)Math.Cos(radian), p.Y + dist * (int)Math.Sin(radian));
            return c;
        }
    }
}