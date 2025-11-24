using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CCC41Lib;
using CommonWPF;

namespace CCCUI;

class MainViewModel : ViewModelBase
{
    // pixel size of a grid position on the map
    private int _gridPositionSize = 5;

    private Solver _solver = new();

    private bool _running = false;
    private CancellationTokenSource _tokensource;
    private CancellationToken _token;

    private HashSet<Vector2> _visited = new();
    private HashSet<Vector2> _activeStates = new();
    private HashSet<Vector2> _bestStates = new();
    private Vector2? _current;

    public bool ShowEveryStep
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public int Level
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    // tree of levels and files
    public ObservableCollection<ScenarioNode> FilesCollection { get; set; } = new();

    // data sets in one file
    public FileDataSet CurrentFileDataSet
    {
        get => GetValue<FileDataSet>();
        set
        {
            SetValue(value);
            CurrentDataSetIndexInput = "0";
            Level = value.Level;
            if (value != null && value.DataSets.Count > 0)
            {
                CurrentDataSet = value.DataSets[CurrentDataSetIndex];
            }
        }
    }

    // selected dataset in file
    public int CurrentDataSetIndex
    {
        get => GetValue<int>();
        set
        {
            if (value >= 0 && value < CurrentFileDataSet.DataSets.Count)
            {
                SetValue(value);
                CurrentDataSet = CurrentFileDataSet.DataSets[value];
            }
        }
    }
    public DataSet CurrentDataSet
    {
        get => GetValue<DataSet>();
        set
        {
            SetValue(value);
            if (CurrentFileDataSet != null && value != null)
            {
                XSequence = value.XSequenceString;
                YSequence = value.YSequenceString;
                Sequences = $"{XSequence}\n{YSequence}";
                SetPathPositions();
                DataSetErrors = string.Join("\n", CurrentDataSet.ErrorText);
                TimeUsed = value.TimeUsed;
                DataSetValid = value.Valid;
                _visited = new();
                _activeStates = new();
                _bestStates = new();
                _current = null;
                DrawDataSet(value);
            }
            Timing = 0;
        }
    }

    private BitmapGridDrawing? _bitmap;

    public Image Image
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }

    public string XSequence
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    public string YSequence
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    public string Sequences
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public bool DataSetValid
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }
    public string DataSetErrors
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    public string ValidityOverview
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public List<Vector2> PathPositions { get; set; } = new();

    public int TimeUsed
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    public long Timing
    {
        get => GetValue<long>();
        set => SetValue(value);
    }

    public MainViewModel()
    {
        ParseData();

        Level = 6;
        CurrentDataSetIndex = 0;
        Timing = 0;

        PreviousDataSet = new RelayCommand(CanDoPreviousDataSet, DoPreviousDataSet);
        NextDataSet = new RelayCommand(CanDoNextDataSet, DoNextDataSet);

        Solve = new RelayCommand(CanSolve, DoSolve);
        Simulation = new RelayCommand(CanDoSimulation, DoSimulation);

        WriteOutputFiles = new RelayCommand(CanWriteOutputFiles, DoWriteOutputFiles);
    }

    public RelayCommand PreviousDataSet { get; }
    public bool CanDoPreviousDataSet()
    {
        return CurrentFileDataSet != null && CurrentDataSetIndex > 0;
    }
    public void DoPreviousDataSet()
    {
        --CurrentDataSetIndex;
    }

    public RelayCommand NextDataSet { get; }
    public bool CanDoNextDataSet()
    {
        return CurrentFileDataSet != null && CurrentDataSetIndex < CurrentFileDataSet.DataSets.Count - 1;
    }
    public void DoNextDataSet()
    {
        ++CurrentDataSetIndex;
    }

    public RelayCommand Solve { get; }
    public bool CanSolve()
    {
        return CurrentFileDataSet != null;
    }
    public void DoSolve()
    {
        Timing = 0;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _solver.Solve(Level, CurrentDataSet);
        stopWatch.Stop();
        Timing = stopWatch.ElapsedMilliseconds;
        XSequence = CurrentDataSet.XSequenceString;
        YSequence = CurrentDataSet.YSequenceString;
        Sequences = string.Join("\n", CurrentDataSet.XSequenceString, CurrentDataSet.YSequenceString);
        SetPathPositions();
        TimeUsed = CurrentDataSet.TimeUsed;
        DataSetValid = CurrentDataSet.Valid;
        DataSetErrors = string.Join("\n", CurrentDataSet.ErrorText);
        DrawDataSet(CurrentDataSet);
    }

    public RelayCommand Simulation { get; }
    public bool CanDoSimulation()
    {
        return CurrentFileDataSet != null;
    }
    public void DoSimulation()
    {
        _tokensource = new CancellationTokenSource();
        _token = _tokensource.Token;

        StartSimulation();
    }

    private async Task StartSimulation()
    {
        _running = true;

        _solver.SetupLevel7(CurrentDataSet);

        while (_solver.MoveStates.Count > 0)
        {
            _solver.CheckBatchDone();
            _solver.PrepareNextStepLevel7();

            if (ShowEveryStep || _solver.BatchDone)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    _visited = _solver.Visited.Keys.ToHashSet();
                    var moveStates = _solver.MoveStates.SelectMany(m => m.Value);
                    _activeStates = moveStates.Select(m => m.Position).ToHashSet();
                    _bestStates = _solver.BestStates.Select(m => m.Position).ToHashSet();
                    _current = _solver.Current != null ? _solver.Current.Position : null;

                    foreach (var pos in _activeStates)
                    {
                        _visited.Remove(pos);
                    }
                    foreach (var pos in _bestStates)
                    {
                        _activeStates.Remove(pos);
                    }

                    UpdateDrawing();
                });
                await Task.Delay(1);
            }
            if (_token.IsCancellationRequested)
            {
                _running = false;
                break;
            }
            _solver.NextStepLevel7(CurrentDataSet);

            if (_solver.FinalState != null)
            {
                break;
            }
        }

        _solver.CreateSolutionLevel7(CurrentDataSet);
        XSequence = CurrentDataSet.XSequenceString;
        YSequence = CurrentDataSet.YSequenceString;
        Sequences = string.Join("\n", CurrentDataSet.XSequenceString, CurrentDataSet.YSequenceString);
        SetPathPositions();
        TimeUsed = CurrentDataSet.TimeUsed;
        DataSetValid = CurrentDataSet.Valid;
        DataSetErrors = string.Join("\n", CurrentDataSet.ErrorText);
        DrawDataSet(CurrentDataSet);
    }

    public RelayCommand Stop { get; }
    public bool CanStop()
    {
        return true;
    }
    public void DoStop()
    {
        _tokensource.Cancel();
    }

    public RelayCommand WriteOutputFiles { get; }
    public bool CanWriteOutputFiles()
    {
        return true;
    }
    public void DoWriteOutputFiles()
    {
        var filesLevelNode = FilesCollection.FirstOrDefault(f => f.Level == Level);
        var inputFiles = filesLevelNode.Children;


        var invalidText = new List<string>();

        foreach (var fileNode in inputFiles)
        {
            var outputFile = fileNode.FileDataSet.FilePath.Replace(".in", ".out");
            using var outputWriter = new StreamWriter(outputFile);


            var invalidDataSets = new List<int>();

            for (int i = 0; i < fileNode.FileDataSet.DataSets.Count; i++)
            {
                DataSet? dataSet = fileNode.FileDataSet.DataSets[i];
                _solver.Solve(Level, dataSet);
                if (!dataSet.Valid)
                {
                    invalidDataSets.Add(i);
                }
            }

            if (invalidDataSets.Any())
            {
                invalidText.Add($"{fileNode.Name}: {string.Join(",", invalidDataSets)}");
            }

            var output = _solver.CreateOutput(fileNode.FileDataSet);
            outputWriter.Write(output);

        }

        ValidityOverview = invalidText.Count > 0 ? string.Join("\n", invalidText) : "All data sets valid.";
    }

    private void ParseData()
    {
        var inputPath = $"N:/Birgit/Coding/CatCoder/CCC41/Files";

        var subDirs = Directory.GetDirectories(inputPath);
        var fileFolderRegex = @"Level(\d+)";

        foreach (var subDir in subDirs)
        {
            var parts = subDir.Split('\\');
            var folderName = parts.Last();

            var match = Regex.Match(folderName, fileFolderRegex);

            var currentLevel = int.Parse(match.Groups[1].Value);

            if (match.Success)
            {
                var inputFiles = Directory.GetFiles(subDir, "*.in");

                foreach (var file in inputFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    var levelName = $"Level {currentLevel}";
                    var FileDataSetName = fileName;

                    var fileDataSet = new FileDataSet(currentLevel, file);

                    var levelNode = FilesCollection.FirstOrDefault(n => n.Name == levelName);
                    if (levelNode == null)
                    {
                        levelNode = new ScenarioNode()
                        {
                            Name = levelName,
                            Level = currentLevel
                        };
                        FilesCollection.Add(levelNode);
                    }

                    levelNode.Children.Add(new ScenarioNode()
                    {
                        Name = FileDataSetName,
                        FileDataSet = fileDataSet,
                        Level = currentLevel
                    });
                }
            }
        }

    }

    private void SetPathPositions()
    {
        PathPositions.Clear();
        foreach (var pos in CurrentDataSet.TimedPositions)
        {
            PathPositions.Add(CurrentDataSet.GetGridPosition(pos.Value));
        }
    }

    private void DrawDataSet(DataSet dataset)
    {
        _gridPositionSize = 5;
        _bitmap = new BitmapGridDrawing(dataset.Width, dataset.Height, _gridPositionSize, 1);
        _bitmap.BackgroundColor = Color.FromRgb(0, 0, 30);
        _bitmap.GridLineColor = Color.FromRgb(0, 0, 60);

        _bitmap.DrawBackGround();

        foreach (var area in dataset.ForbiddenAreas)
        {
            var gridPosition = dataset.GetGridPosition(area);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(120, 0, 0));
        }
        foreach (var asteroid in dataset.Asteroids)
        {
            var gridPosition = dataset.GetGridPosition(asteroid);
            _bitmap.DrawXInGridcell((int)gridPosition.X, (int)gridPosition.Y, _gridPositionSize, Color.FromRgb(255, 0, 0));
        }

        foreach (var cell in _visited)
        {
            var gridPosition = dataset.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(50, 50, 50));
        }

        foreach (var cell in _activeStates)
        {
            var gridPosition = dataset.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(100, 100, 100));
        }

        foreach (var cell in _bestStates)
        {
            var gridPosition = dataset.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(255, 128, 0));
        }

        if (_current != null)
        {
            var gridPosition = dataset.GetGridPosition(_current.Value);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(255, 255, 0));
        }


        var gridStartPosition = dataset.GetGridPosition(dataset.StartPosition);
        _bitmap.FillGridCell((int)gridStartPosition.X, (int)gridStartPosition.Y, Color.FromRgb(0, 0, 200));
        _bitmap.DrawXInGridcell((int)gridStartPosition.X, (int)gridStartPosition.Y, _gridPositionSize, Color.FromRgb(0, 0, 255));
        var gridTargetPosition = dataset.GetGridPosition(dataset.TargetPosition);
        _bitmap.FillGridCell((int)gridTargetPosition.X, (int)gridTargetPosition.Y, Color.FromRgb(0, 200, 0));
        _bitmap.DrawXInGridcell((int)gridTargetPosition.X, (int)gridTargetPosition.Y, _gridPositionSize, Color.FromRgb(0, 255, 0));

        if (PathPositions.Count > 0)
        {
            _bitmap.DrawConnectedLines(PathPositions, Color.FromRgb(255, 255, 255));
        }


        Image = new Image();

        Image.Stretch = Stretch.None;
        Image.Margin = new Thickness(0);

        Image.Source = _bitmap.Picture;
        RaisePropertyChanged(nameof(Image));
    }

    public void UpdateDrawing()
    {
        foreach (var cell in _visited)
        {
            var gridPosition = CurrentDataSet.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(100, 100, 100));
        }

        foreach (var cell in _activeStates)
        {
            var gridPosition = CurrentDataSet.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(200, 200, 200));
        }

        foreach (var cell in _bestStates)
        {
            var gridPosition = CurrentDataSet.GetGridPosition(cell);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(255, 128, 0));
        }

        if (_current != null)
        {
            var gridPosition = CurrentDataSet.GetGridPosition(_current.Value);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(255, 255, 0));
        }

        var gridStartPosition = CurrentDataSet.GetGridPosition(CurrentDataSet.StartPosition);
        _bitmap.FillGridCell((int)gridStartPosition.X, (int)gridStartPosition.Y, Color.FromRgb(0, 0, 200));
        _bitmap.DrawXInGridcell((int)gridStartPosition.X, (int)gridStartPosition.Y, _gridPositionSize, Color.FromRgb(0, 0, 255));


        Image.Source = _bitmap.Picture;
        RaisePropertyChanged(nameof(Image));
    }

}
