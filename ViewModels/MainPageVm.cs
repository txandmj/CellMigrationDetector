using System.Windows.Input;

namespace CellMigrationDetector;

public class MainPageVm : ViewModelBase, IDisposable
{
    public string MyStr { get; private set; } = "Hello World";
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

    public ICommand OpenFileCmd => new Command(async () =>
    {
        FileResult? res = await MainPageVm.PickAndShow(PickOptions.Images).CAF();
        MyStr = "OpenFileCmd clicked";
        await Task.Delay(100).ConfigureAwait(true);
        RaisePropertyChanged(nameof(MyStr));
    });

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
