using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MigrationImage.ViewModel;

public class MainPageVm : INotifyPropertyChanged, IDisposable
{
    public string MyStr { get; private set; } = "Hello World";
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    [Conditional("DEBUG")]
    //[DebuggerStepThrough]
    public void VerifyPropertyName(String propertyName)
    {
        // verify that the property name matches a real,  
        // public, instance property on this Object.
        if (TypeDescriptor.GetProperties(this)[propertyName] == null)
        {
            Debug.Fail("Invalid property name: " + propertyName);
        }
    }
    public async Task<FileResult> PickAndShow(PickOptions options)
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
            // The user canceled or something went wrong
        }

        return null;
    }

    public ICommand OpenFileCmd => new Command(async () =>
    {
        //FileResult res = await PickAndShow(PickOptions.Images).ConfigureAwait(false);
        MyStr = "OpenFileCmd clicked";
        await Task.Delay(100).ConfigureAwait(true);
        OnPropertyChanged(nameof(MyStr));
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
