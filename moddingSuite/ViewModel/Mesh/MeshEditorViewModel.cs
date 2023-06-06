using moddingSuite.Model.Mesh;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Ndf;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace moddingSuite.ViewModel.Mesh;

public class MeshEditorViewModel : ViewModelBase
{
    private MeshFile _meshFile;
    private ICollectionView _multiMaterialMeshesCollectionView;
    private string _multiMaterialMeshesFilterExpression = string.Empty;

    public ICommand EditTextureBindingsCommand { get; protected set; }

    public ICollectionView MultiMaterialMeshesCollectionView
    {
        get
        {
            if (_multiMaterialMeshesCollectionView == null)
            {
                BuildMultiMaterialMeshesCollectionView();
            }

            return _multiMaterialMeshesCollectionView;
        }
    }

    public MeshFile MeshFile
    {
        get => _meshFile;
        set { _meshFile = value; OnPropertyChanged("MeshFile"); }
    }

    public string MultiMaterialMeshesFilterExpression
    {
        get => _multiMaterialMeshesFilterExpression;
        set
        {
            _multiMaterialMeshesFilterExpression = value;
            OnPropertyChanged(() => MultiMaterialMeshesFilterExpression);
            MultiMaterialMeshesCollectionView.Refresh();
        }
    }

    public MeshEditorViewModel(MeshFile file)
    {
        MeshFile = file;

        EditTextureBindingsCommand = new ActionCommand(EditTextureBindingsExecute);
    }

    private void EditTextureBindingsExecute(object obj)
    {
        NdfEditorMainViewModel ndfEditor = new(MeshFile.TextureBindings);

        DialogProvider.ProvideView(ndfEditor, this);
    }

    private void BuildMultiMaterialMeshesCollectionView()
    {
        _multiMaterialMeshesCollectionView = CollectionViewSource.GetDefaultView(MeshFile.MultiMaterialMeshFiles);
        _multiMaterialMeshesCollectionView.Filter = FilterMultiMaterialMeshes;

        OnPropertyChanged(() => MultiMaterialMeshesCollectionView);
    }

    private bool FilterMultiMaterialMeshes(object obj) => obj is not MeshContentFile file || MultiMaterialMeshesFilterExpression == string.Empty || MultiMaterialMeshesFilterExpression.Length < 3
|| file.Path.Contains(MultiMaterialMeshesFilterExpression);

}
