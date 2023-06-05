using System.Collections.Generic;

namespace moddingSuite.Model.Scenario;

public class AreaFile
{
    private List<AreaColletion>  _areaManagers = new();

    public List<AreaColletion> AreaManagers
    {
        get { return _areaManagers; }
        set { _areaManagers = value; }
    }
}
