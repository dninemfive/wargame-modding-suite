using moddingSuite.ViewModel.Base;
using System;
using System.Text;
using System.Windows.Input;

namespace moddingSuite.ViewModel.UnhandledException;

public class UnhandledExceptionViewModel : ViewModelBase
{
    private string _title;
    private string _errorText;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged("Title");
        }
    }

    public string ErrorText
    {
        get => _errorText;
        set
        {
            _errorText = value;
            OnPropertyChanged("ErrorText");
        }
    }

    public ICommand SendErrorCommand { get; set; }

    public UnhandledExceptionViewModel(Exception exception)
    {
        Title = "An unhandled exception occured";

        SendErrorCommand = new ActionCommand(SendErrorExecute);

        StringBuilder sb = new();

        Exception excep = exception;

        while (excep != null)
        {
            _ = sb.Append(exception);
            excep = excep.InnerException;
        }

        ErrorText = sb.ToString();
    }

    private void SendErrorExecute(object obj) => throw new NotImplementedException();
}
