using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace SmartGift
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture _captureManager = null;
        private BitmapImage _bmpImage = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            _bmpImage = new BitmapImage();
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

            mediaPreview.Source = _captureManager;
            await _captureManager.StartPreviewAsync();

            return true;
        }

        #endregion

        #region Image Capture

        private async Task<bool> ImageCaptureAndDisplay()
        {
            ImageEncodingProperties imageFormat = ImageEncodingProperties.CreateJpeg();

            using (InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream())
            {
                await _captureManager.CapturePhotoToStreamAsync(imageFormat, imageStream);
                await imageStream.FlushAsync();
                imageStream.Seek(0);

                _bmpImage.SetSource(imageStream);
                imagePreview.Source = _bmpImage;
            }

            return true;
        }

        #endregion

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await ImageCaptureAndDisplay();
        }
    }
}
