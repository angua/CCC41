using System.Collections.ObjectModel;
using CCC41Lib;

namespace CCCUI;

class ScenarioNode
{
    public ObservableCollection<ScenarioNode> Children { get; set; } = new();

    public int Level { get; set; }

    public FileDataSet? FileDataSet { get; set; }

    public string Name { get; set; } = string.Empty;

}
