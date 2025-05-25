using CommunityToolkit.Maui.Storage;

namespace CellMigrationDetector;

public partial class MainPage : ContentPage
{
    private readonly IFileSaver _fileSaver;
    public MainPage(MainPageVm vm, IFileSaver fileSaver)
    {
        BindingContext = vm;
        InitializeComponent();
        _fileSaver = fileSaver;
    }

    public Task OnCreated()
    {
        MainPageVm vm = BindingContext as MainPageVm ??
            throw App.LogNException("MainPageVm is null");
        return Task.CompletedTask;
    }

    public Task OnDestroy()
    {
        MainPageVm vm = BindingContext as MainPageVm ??
            throw App.LogNException("CameraHmiViewModel is null");
        vm.Dispose();
        return Task.CompletedTask;
    }
}
