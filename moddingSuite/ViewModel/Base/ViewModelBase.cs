using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace moddingSuite.ViewModel.Base;

public class ViewModelBase : INotifyPropertyChanged
{
    private bool _isUiBusy = false;

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged<T>(params Expression<Func<T>>[] props)
    {
        foreach (Expression<Func<T>> prop in props)
        {
            if (PropertyChanged != null && prop.Body is MemberExpression body)
                PropertyChanged(this, new PropertyChangedEventArgs(body.Member.Name));
        }
    }

    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    [XmlIgnore]
    public bool IsUIBusy
    {
        get
        {
            return _isUiBusy;
        }
        set
        {
            _isUiBusy = value;
            OnPropertyChanged(() => IsUIBusy);
        }
    }
}
