using Launcher.Source.ViewModel;

namespace Launcher;

public partial class MainWindow {
    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowVM();
    }
}