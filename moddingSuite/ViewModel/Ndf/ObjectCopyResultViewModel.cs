using moddingSuite.Model.Ndfbin;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Ndf;

public class ObjectCopyResultViewModel : ViewModelBase
{
    public ObservableCollection<NdfObject> NewInstances { get; set; }

    public ICommand DetailsCommand { get; set; }

    public NdfEditorMainViewModel Editor { get; set; }

    public ObjectCopyResultViewModel(List<NdfObject> results, NdfEditorMainViewModel editor)
    {
        NewInstances = new ObservableCollection<NdfObject>(results);

        Editor = editor;

        DetailsCommand = new ActionCommand(DetailsExecute);
    }

    private void DetailsExecute(object obj)
    {
        if (obj is not NdfObject instance)
            return;

        NdfClassViewModel vm = new(instance.Class, this);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == instance.Id);

        if (inst == null)
            return;

        vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm, Editor);
    }
}
