using System;

namespace moddingSuite.View.DialogProvider;

public interface IViewMap
{
    Type ViewType { get; }
    Type ViewModelType { get; }
}
