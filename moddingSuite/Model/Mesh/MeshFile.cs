using moddingSuite.Model.Ndfbin;
using System.Collections.ObjectModel;

namespace moddingSuite.Model.Mesh;

public class MeshFile
{
    public MeshHeader Header { get; set; }
    public MeshSubHeader SubHeader { get; set; }

    public ObservableCollection<MeshContentFile> MultiMaterialMeshFiles { get; set; }
    public NdfBinary TextureBindings { get; set; }
}
