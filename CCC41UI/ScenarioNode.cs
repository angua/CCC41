using System.Collections.ObjectModel;
using CCC41Lib;

namespace CCCUI;

class ScenarioNode
{
    public ObservableCollection<ScenarioNode> Children { get; set; } = new();

    public FileDataSet FileDataSet { get; set; }

    public string Name { get; set; }

}
