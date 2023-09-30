using System.ComponentModel;

namespace Launcher.Source.ViewModel.Base;

public abstract class BaseViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChangedEvent(string propertyName) {
        PropertyChangedEventHandler handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}