using moddingSuite.Model.Ndfbin;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Ndf;

public class ReferenceSearchResultViewModel : ViewModelBase
{
    public ObservableCollection<NdfPropertyValue> Results { get; set; }

    public ICommand DetailsCommand { get; set; }

    public NdfEditorMainViewModel Editor { get; set; }

    public ReferenceSearchResultViewModel(List<NdfPropertyValue> results, NdfEditorMainViewModel editor)
    {
        Results = new ObservableCollection<NdfPropertyValue>(results);

        Editor = editor;

        DetailsCommand = new ActionCommand(DetailsExecute);
    }

    private void DetailsExecute(object obj)
    {
        if (obj is not NdfPropertyValue propVal)
            return;

        NdfClassViewModel vm = new(propVal.Instance.Class, this);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == propVal.Instance.Id);

        if (inst == null)
            return;

        _ = vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm, Editor);
    }
}
