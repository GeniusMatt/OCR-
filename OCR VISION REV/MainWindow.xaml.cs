using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tesseract;

namespace OCR_VISION_REV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        System.Timers.Timer timer = new System.Timers.Timer()
        {
            Interval = 250,
        };
        public MainWindow()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            timer.Start(); 
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();

            if (camera.LastMatFrame != null && !camera.LastMatFrame.Empty())
            {
                this.Dispatcher.Invoke(new Action(() => {
                    string detection = "";
                    // DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, "4311EVKA");
                    DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, FindString.Text);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }

            timer.Start();
        }

        private void ForcusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            camera.capture.Focus = (int)(e.NewValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string detection = "";
            DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, "4311EVKA");
            DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, "4311EVKA");
        }
        //public Mat DetectString(Mat source, out string str, string tagertString)
        //{
        //    engine.SetVariable("debug_file", "NUL");
        //    DateTime now = DateTime.Now;
        //    Mat mat = source.Clone();
        //    Bitmap output = mat.ToBitmap();
        //    var ocrtext = string.Empty;
        //    try
        //    {
        //        using (var img = PixConverter.ToPix(output))
        //        {
        //            using (var page = engine.Process(img))
        //            {
        //                ocrtext = page.GetText();
        //                var rects = page.GetSegmentedRegions(PageIteratorLevel.Block);
        //                foreach (var item in rects)
        //                {
        //                    mat.Rectangle(new OpenCvSharp.Rect(item.X, item.Y, item.Width, item.Height), new Scalar(0, 255, 0));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    str = ocrtext.Replace("\r", "").Replace("\n", "");
        //    lbDetectString.Content = "ORC: " + str;
        //    img1.Source = mat.ToBitmapSource();
        //    return mat;
        //}
        //public Mat DetectString(Mat source,int threashold, out string detectString, string targetStr)
        //{
        //    Mat sourceToTest = source.Clone();


        //    Mat graysource = sourceToTest.CvtColor(ColorConversionCodes.BGR2GRAY);

        //    Mat binarySource = graysource.Threshold(0, 255, ThresholdTypes.Otsu);

        //    this.img1.Source = binarySource.ToBitmapSource();

        //    OpenCvSharp.Point[][] contour;
        //    HierarchyIndex[] hierarchy;
        //    List<OpenCvSharp.Rect> digitContour = new List<OpenCvSharp.Rect>();
        //    Cv2.FindContours(binarySource, out contour, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        //    for (int i = 0; i < contour.Length; i++)
        //    {
        //        OpenCvSharp.Rect rect = Cv2.BoundingRect(contour[i]);
        //        if (Cv2.ContourArea(contour[i]) < (threashold * threashold))
        //        {
        //            binarySource.DrawContours(contour, i, new Scalar(0,0,0));
        //        }
        //    }

        //    this.img2.Source = binarySource.ToBitmapSource();

        //    OpenCvSharp.Text.OCRTesseract tesseract = OpenCvSharp.Text.OCRTesseract.Create(language: "eng", psmode: 2, charWhitelist: targetStr);
        //    //OpenCvSharp.Text.OCRTesseract tesseract = OpenCvSharp.Text.OCRTesseract.Create(language: "eng", psmode: 2);
        //    Cv2.BitwiseNot(binarySource, binarySource);
        //    tesseract.Run(binarySource, out string text, out _, out _, out _, OpenCvSharp.Text.ComponentLevels.Word);

        //    this.img3.Source = binarySource.ToBitmapSource();
        //    Console.WriteLine(" Output text: {0}", text);
        //    detectString = text.Replace("\r", "\n").Replace("\n", "");
        //    //detectString = "";
        //    lbDetectString.Content = "ORC: " + detectString;
        //    return binarySource;
        //}

        OpenCvSharp.Text.OCRTesseract tesseract = OpenCvSharp.Text.OCRTesseract.Create(language: "eng", oem:0, psmode: 3);
        private OpenCvSharp.Rect[] digitContour;
        private float[] confidence;
        private string[] texts;

        public Mat DetectString(Mat source, int threashold, out string detectString, string targetStr)
        {
            Mat sourceToTest = source.Clone();


            Mat graysource = sourceToTest.CvtColor(ColorConversionCodes.BGR2GRAY);
            graysource = graysource.Blur(new OpenCvSharp.Size(5,5));
            this.img3.Source = graysource.ToBitmapSource();
            Mat binarySource = graysource.AdaptiveThreshold(255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 9, 4);
            //Mat binarySource = graysource.AdaptiveThreshold(255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, 21, 11);
            //Mat binarySource = graysource.Threshold(threashold, 255, ThresholdTypes.Binary);

            //this.img2.Source = binarySource.ToBitmapSource();
            OpenCvSharp.Point[][] contour;
            Cv2.FindContours(binarySource, out contour, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            for (int i = 0; i < contour.Length; i++)
            {
                if (Cv2.ContourArea(contour[i]) <  threashold * 2)
                {
                    binarySource.DrawContours(contour, i, new Scalar(0,0,0), -1);
                    //binarySource.Rectangle(Cv2.BoundingRect(contour[i]), new Scalar(0, 0, 0), -1);
                }
            }
            //binarySource.Rectangle(new OpenCvSharp.Rect(0,60,100,60), new Scalar(0, 0, 0), -1);
            //binarySource.Rectangle(new OpenCvSharp.Rect(320,60,60,60), new Scalar(0, 0, 0), -1);

            //OpenCvSharp.Text.OCRTesseract tesseract = OpenCvSharp.Text.OCRTesseract.Create(language: "eng", psmode: 2);
            Cv2.BitwiseNot(binarySource, binarySource);

            this.img2.Source = binarySource.ToBitmapSource();

            tesseract.SetWhiteList(targetStr);
            tesseract.Run(binarySource, out string text, out digitContour, out texts, out confidence, OpenCvSharp.Text.ComponentLevels.Word);
            Mat mat = binarySource.CvtColor(ColorConversionCodes.GRAY2RGB);
            detectString = "";
            int index = 0;
            for (int i = 0; i < texts.Length; i++)
            {
                bool OK = false;
                for (int j = 0; j < texts[i].Length; j ++)
                {
                    index = detectString.Length;
                    var loc = targetStr.IndexOf(texts[i][j], index == 0 ? 0 : detectString.Length);
                    Console.WriteLine("{0}---{1}---{2}---{3}", texts[i][j], confidence[i], loc, index);
                    if (loc == index)
                    {
                        OK = true;
                        detectString += texts[i][j];
                    }
                }
                if (OK)
                {
                    sourceToTest.Rectangle(digitContour[i], new Scalar(0, 255, 0));
                }
                else
                {
                    sourceToTest.Rectangle(digitContour[i], new Scalar(0, 0, 255));
                }
            }

            this.img1.Source = sourceToTest.ToBitmapSource();
            Console.WriteLine(" Output text: {0}", detectString);
            //detectString = text.Replace("\r", "\n").Replace("\n\n", "\n").Replace(" ","");
            //detectString = "";
            lbDetectString.Content = "ORC: " + detectString;
            return binarySource;
        }
        private void threshodSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           // string detection = "";
           // DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, "4311EVKA");
            //DetectString(camera.LastMatFrame, (int)threshodSlider.Value, out detection, "SLMDIPSC39KM5");
        }
    }
}
