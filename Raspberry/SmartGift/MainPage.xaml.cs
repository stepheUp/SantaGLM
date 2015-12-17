using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace SmartGift
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture _captureManager = null;
        private BitmapImage _bmpImage = null;
        private StorageFile _file = null;
        private DispatcherTimer _timer = null;
        private FEZHAT _hat = null;
        private bool _switch = true;

        static DeviceClient _deviceClient;
        static string host = "SantaGLM-IoTHub";
        static string deviceId = "santasFirstDevice";
        static string deviceKey = "oTXYY/HeTxfFY73OT26wqFMemuILqUqFLLUUQtTS+VU=";


        public MainPage()
        {
            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await InitMediaCapture();
        }

        #region Initialization

        private async Task<bool> InitMediaCapture()
        {
            _captureManager = new MediaCapture();
            await _captureManager.InitializeAsync();
            //await SetMinResolution();

            // init fez hat
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            if (qualifiers["DeviceFamily"] == "IoT")
            {
                _hat = await FEZHAT.CreateAsync();

                captureButton.Visibility = Visibility.Collapsed;

                //Grid.SetColumn(facesGrid, 0);
                //Grid.SetColumnSpan(facesGrid, 2);

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.Tick += this.OnTick;
                _timer.Start();
            }

            try
            {
                mediaPreview.Source = _captureManager;
                mediaPreview.FlowDirection = FlowDirection.RightToLeft;

                // start capture preview
                await _captureManager.StartPreviewAsync();

                var connectionString = string.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}", host, deviceId, deviceKey);

                // var key = new DeviceAuthenticationWithRegistrySymmetricKey("santasFirstDevice", deviceKey);
                _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
           
            return true;
        }

        //This is how you can set your resolution
        public async Task<bool> SetMinResolution()
        {
            System.Collections.Generic.IReadOnlyList<IMediaEncodingProperties> res;
            res = _captureManager.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);

            uint minResolution = 2048;
            int indexResolution = 0;

            if (res.Count >= 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    VideoEncodingProperties vp = (VideoEncodingProperties)res[i];

                    if (vp.Width < minResolution)
                    {
                        indexResolution = i;
                        minResolution = vp.Width;
                        Debug.WriteLine("Resolution: " + vp.Width);
                    }
                }

                await _captureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, res[indexResolution]);
            }

            return true;
        }

        private async void OnTick(object sender, object e)
        {
            if (_hat.IsDIO18Pressed())
            {
                _hat.D2.Color = FEZHAT.Color.Red;
                _hat.D3.Color = FEZHAT.Color.Red;

                await DoTheJob();
            }

            if (_hat.IsDIO22Pressed())
            {
                _hat.D2.Color = FEZHAT.Color.Red;
                _hat.D3.Color = FEZHAT.Color.Red;

                await DoTheJob();
            }

            _hat.D2.Color = _switch ? FEZHAT.Color.Green : FEZHAT.Color.White;
            _hat.D3.Color = _switch ? FEZHAT.Color.White : FEZHAT.Color.Green;

            _switch = _switch ? false : true;
        }

        #endregion

        #region Image Capture

        private async Task<bool> ImageCaptureAndDisplay()
        {
            ImageEncodingProperties imageFormat = ImageEncodingProperties.CreateJpeg();

            // create storage file in local app storage
            _file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "SantaPhoto.jpg",
                CreationCollisionOption.GenerateUniqueName);

            // capture to file
            await _captureManager.CapturePhotoToStorageFileAsync(imageFormat, _file);

            _bmpImage = new BitmapImage(new Uri(_file.Path));
            imagePreview.Source = _bmpImage;

            return true;
        }

        #endregion

        #region Main Action - Do The Job

        private async Task<bool> DoTheJob()
        {
            
            await ImageCaptureAndDisplay();

            EmotionServiceClient oxford = new EmotionServiceClient("b923157c821f42e4bb36e4e1fb1ec007");
            Emotion[] emotions = await oxford.RecognizeAsync(await _file.OpenStreamForReadAsync());

            await DisplayAndSendEmotions(emotions);

            return true;
        }

        #endregion

        #region Display Emotions

        private async Task<bool> DisplayAndSendEmotions(Emotion[] emotions)
        {
            var previewProperties = _captureManager.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            var previewStream = previewProperties as VideoEncodingProperties;

            double ratioHeight = imagePreview.ActualHeight / previewStream.Height;
            double ratioWidth = imagePreview.ActualWidth / previewStream.Width;

            facesLayer.Height = imagePreview.ActualHeight;
            facesLayer.Width = imagePreview.ActualWidth;

            facesLayer.Children.Clear();

            foreach (var em in emotions)
            {
                Rectangle faceBoundingBox = new Rectangle();
                faceBoundingBox.Width = em.FaceRectangle.Width * ratioWidth;
                faceBoundingBox.Height = em.FaceRectangle.Height * ratioHeight;

                Dictionary<string, float> scores = new Dictionary<string, float>();
                scores.Add("Anger", em.Scores.Anger);
                scores.Add("Contempt", em.Scores.Contempt);
                scores.Add("Disgust", em.Scores.Disgust);
                scores.Add("Fear", em.Scores.Fear);
                scores.Add("Happiness", em.Scores.Happiness);
                scores.Add("Neutral", em.Scores.Neutral);     
                scores.Add("Sadness", em.Scores.Sadness);
                scores.Add("Surprise", em.Scores.Surprise);

                var emotion = scores.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;

                faceBoundingBox.Stroke = new SolidColorBrush(Colors.White);
                faceBoundingBox.StrokeThickness = 2;

                TextBlock emotionText = new TextBlock();
                emotionText.Text = emotion;
                emotionText.Foreground = new SolidColorBrush(Colors.White);

                Canvas.SetLeft(faceBoundingBox, em.FaceRectangle.Left * ratioWidth);
                Canvas.SetTop(faceBoundingBox, em.FaceRectangle.Top * ratioHeight);

                Canvas.SetLeft(emotionText, (em.FaceRectangle.Left * ratioWidth));
                Canvas.SetTop(emotionText, (em.FaceRectangle.Top * ratioHeight) - 20);

                facesLayer.Children.Add(faceBoundingBox);
                facesLayer.Children.Add(emotionText);

                await SendDeviceToCloudMessagesAsync(emotion);
            }

            return true;
        }

        #endregion

        #region Send Messages IoT Hub

        private async Task<bool> SendDeviceToCloudMessagesAsync(string emotion)
        {
            // code only for stephanie ^^ - adding rating
            int rating = 0;
            switch (emotion)
            {
                case "Sadness":
                    rating = 0;
                    break;
                case "Anger":
                    rating = 1;
                    break;
                case "Disgust":
                    rating = 2;
                    break;
                case "Contempt":
                    rating = 3;
                    break;
                case "Fear":
                    rating = 4;
                    break;
                case "Neutral":
                    rating = 5;
                    break;
                case "Surprise":
                    rating = 6;
                    break;
                case "Happiness":
                    rating = 7;
                    break;
            }

            var telemetryDataPoint = new
            {
                deviceId = "Gift #Raspberry#",
                location = "Prague",
                emotion = emotion,
                rating = rating,
                state = "open",
                giftType = "Microsoft Band 2"
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

            return true;
        }

        #endregion

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await DoTheJob();
        }
    }
}
