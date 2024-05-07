using DirectShowLib;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

namespace OCR_VISION_REV
{
    /// <summary>
    /// Interaction logic for CameraControl.xaml
    /// </summary>
    public partial class CameraControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler CamerasCollectionEmplty;
        private Task _previewTask;

        private CancellationTokenSource _cancellationTokenSource;

        public VideoCapture capture = new VideoCapture(1, VideoCaptureAPIs.DSHOW);
        private Mat _LastMatFrame;
        public Mat LastMatFrame
        {
            get { return _LastMatFrame; }
            set
            {
                if (value != null || value != _LastMatFrame)
                    _LastMatFrame = value;
            }
        }

        private BitmapSource lastFrame;

        public BitmapSource LastFrame
        {
            get
            {
                return lastFrame;
            }
            set
            {
                lastFrame = value;
                NotifyPropertyChanged(nameof(LastFrame));
            }
        }

        public BitmapSource LastBitmapSource { get; set; }
        public CameraControl()
        {
            InitializeComponent();
            this.DataContext = this;
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            Start();
        }

        async void Start()
        {
            _= Task.Run(async () =>
            {

                //capture.FrameWidth = 2560;
                //capture.FrameHeight = 1440;
                //capture.AutoExposure = -5;
                //capture.Exposure = -4;
                //capture.Fps = 50;
                //capture.FourCC = "MJPG";
                Mat frame = new Mat();
                //var rect = new OpenCvSharp.Rect(1045, 545, 360, 140);
                //var rect2 = new OpenCvSharp.Rect(1040, 540, 370, 150);

                var rect = new OpenCvSharp.Rect(1045, 545, 520, 240);
                var rect2 = new OpenCvSharp.Rect(1040, 540, 530, 250);
                while (true)
                {
                    if (capture.Read(frame))
                    {
                        if (frame != null && !frame.Empty())
                        {

                            //noLastMatFrame = new Mat(frame, rect);
                            LastMatFrame = frame.Clone();
                           // frame.Rectangle(rect2, new Scalar(0, 0, 255), 3);
                            var bi = frame.Clone().ToBitmapSource();
                            bi.Freeze();
                            LastFrame = bi;
                        }
                    }
                    await Task.Delay(1000); // 30 FPS  1000/30 = 30.33333333 30FPS
                }
            });
        }


    }
}
