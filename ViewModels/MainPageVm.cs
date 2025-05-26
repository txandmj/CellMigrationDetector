using CommunityToolkit.Maui.Storage;
using OpenCvSharp;
using System.Security.Cryptography;
using System.Text;

namespace CellMigrationDetector;

public class MainPageVm : ViewModelBase, IDisposable
{
    public static readonly ImageSource NoImage = "noimage.png";
    private Mat? _rawImage = null;
    private Mat? _augmentedImage = null;
    private Mat? _processedImage = null;
    private string? _filePath = null;
    public static async Task<FileResult?> PickAndShow(PickOptions options)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = await result.OpenReadAsync();
                    var image = ImageSource.FromStream(() => stream);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            App.LogNShow(ex);
        }
        return null;
    }

    public Command OpenFileCmd => new Command(async () =>
    {
        FileResult? res = await MainPageVm.PickAndShow(PickOptions.Images).CAF();
        if (res != null && res.FullPath.Length > 0)
        {
            _filePath = res.FullPath;
            _rawImage = Cv2.ImRead(res.FullPath, ImreadModes.Unchanged);
            RaisePropertyChanged(nameof(ImgSource));
            RaisePropertyChanged(nameof(ImgMetaData));
        }
    });

    public Command SaveFileCmd => new Command(async () =>
    {
        Mat m = GetRenderImage();
        if (m is not null)
        {
            Cv2.ImEncode(".png", m, out byte[] bs);
            using Stream stream = new MemoryStream(bs);
#if WINDOWS
            await FileSaver.Default.SaveAsync(StaticValues.DefaultSaveFolderPath,
                StaticValues.DefaultSaveFilePath(), stream).CAF();
#else
            DialogService.ShowAlert("Error", "Saving file only supported in Windows.");
            await Task.Delay(100).CAF();
#endif
        }
    });

    public Command ProcessImageCmd => new Command(() =>
    {
        Mat m = _rawImage ?? throw new Exception("_rawImage is null");
        Mat processed = new Mat();
        Cv2.Split(m, out Mat[] bgr);
        //Cv2.Blur(bgr[2], processed, new OpenCvSharp.Size(5, 5));
        //Cv2.Dilate(processed, processed, new Mat());
        //Cv2.CvtColor(processed, processed, ColorConversionCodes.BGR2GRAY);
        //var clahe = Cv2.CreateCLAHE();
        //clahe.Apply(processed, processed);
        //Cv2.Watershed()
        Cv2.Threshold(bgr[2], processed, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
        Cv2.MorphologyEx(processed, processed, MorphTypes.Open, kernel);
        //Cv2.Canny()
        //Cv2.Canny(processed, processed, 90, 150);
        //Cv2.AdaptiveThreshold(processed, processed, 250, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 5, 5);
        _processedImage = processed;
        RaisePropertyChanged(nameof(ImgSource));
        RaisePropertyChanged(nameof(ImgMetaData));
    });

    public Command FindContourCmd => new Command(() =>
    {
        Mat m = _processedImage ?? throw new Exception("_processedImage is null");
        Cv2.FindContours(m, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] idx, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
        if (_rawImage is null)
        { throw new Exception("_rawImage is null"); }
        _augmentedImage = _rawImage.Clone();
        for (int i = 0; i < contours.Length; ++i)
        {
            double area = Cv2.ContourArea(contours[i]);
            if (area >= 200)
            {
                Cv2.DrawContours(_augmentedImage, contours, i, new Scalar(255, 255, 255));
            }
        }
        RaisePropertyChanged(nameof(ImgSource));
        RaisePropertyChanged(nameof(ImgMetaData));
    });

    private Mat GetRenderImage()
    {
        if (_augmentedImage is not null)
        { return _augmentedImage; }
        if (_processedImage is not null)
        { return _processedImage; }
        if (_rawImage is not null)
        { return _rawImage; }
        throw App.LogNException("Image is null");
    }

    private Stream GetImageStream()
    {
        Mat m = GetRenderImage();
        Cv2.ImEncode(".jpg", m, out byte[] bs);
        return new MemoryStream(bs, false);
    }

    public ImageSource? ImgSource
    {
        get
        {
            if (_rawImage is null)
            { return NoImage; }
            return ImageSource.FromStream(GetImageStream);
        }
    }

    public string ImgMetaData
    {
        get
        {
            if (_rawImage is null)
            { return "NoImage"; }
            Mat m = GetRenderImage();
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Path.GetFileName(_filePath)}: ");
            sb.Append($"Width: {m.Cols} pixels. Height: {m.Rows} pixels. ");
            sb.Append($"Number of channels: {m.Channels()}. ");
            sb.Append($"Type: {m.Type().ToString()}. ");
            sb.Append($"Each channel bit depth: {m.ElemSize1() * 8} bits. ");
            sb.Append($"Total channel bit depth: {m.ElemSize() * 8} bits. ");
            return sb.ToString();
        }
    }

    #region IDispose
    bool _disposed = false;
    public void Dispose()
    {
        if (_disposed)
        { return; }
        Dispose(true);
        GC.SuppressFinalize(this);
        _disposed = true;
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            //
        }
    }
    #endregion
}
