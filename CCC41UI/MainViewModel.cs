using System.Collections.ObjectModel;
using System.IO;
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
    private int _gridPositionSize = 19;

    private Solver _solver = new();

    public int Level
    {
        get => GetValue<int>();
        set
        {
            SetValue(value);
            ParseData();
        }
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
            CurrentDataSetIndex = 0;
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
        set => SetValue(value);
    }
    public DataSet CurrentDataSet
    {
        get => GetValue<DataSet>();
        set
        {
            SetValue(value);
            if (CurrentFileDataSet != null && value != null)
            {
                DrawDataSet(value);
            }
            StepCount = 0;
            Timing = 0;
        }
    }

    private BitmapGridDrawing? _bitmap;

    public Image Image
    {
        get => GetValue<Image>();
        set => SetValue(value);
    }

    public string LastStepValid
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public int StepCount
    {
        get => GetValue<int>();
        set => SetValue(value);
    }


    public int CurrentPathIndex
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    /*
    public PathStep? CurrentLastPathstep
    {
        get => GetValue<PathStep>();
        set
        {
            SetValue(value);
            if (value != null)
            {
                CurrentDataSet.SetStepsfromLast(value);
                _solver.CreatePathfromSteps(CurrentDataSet);

                LastStepValid = CurrentDataSet.CorrectPathSteps.Last().IsValid.ToString();

                DrawLawn(CurrentDataSet);
            }
        }
    }
    */

    public int AllPathCount
    {
        get => GetValue<int>();
        set => SetValue(value);
    }


    public string Instructions
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    public long Timing
    {
        get => GetValue<long>();
        set => SetValue(value);
    }

    public MainViewModel()
    {
        Level = 6;
        CurrentDataSetIndex = 0;
        Timing = 0;

        PreviousDataSet = new RelayCommand(CanDoPreviousDataSet, DoPreviousDataSet);
        NextDataSet = new RelayCommand(CanDoNextDataSet, DoNextDataSet);

        /*
        FindPath = new RelayCommand(CanFindPath, DoFindPath);

        FindPathNextStep = new RelayCommand(CanFindPathNextStep, DoFindPathNextStep);

        PreviousPath = new RelayCommand(CanPreviousPath, DoPreviousPath);
        NextPath = new RelayCommand(CanNextPath, DoNextPath);

        ClearPath = new RelayCommand(CanClearPath, DoClearPath);
        */
    }

    public RelayCommand PreviousDataSet { get; }
    public bool CanDoPreviousDataSet()
    {
        return CurrentFileDataSet != null && CurrentDataSetIndex > 0;
    }
    public void DoPreviousDataSet()
    {
        CurrentDataSet = CurrentFileDataSet.DataSets[--CurrentDataSetIndex];
    }

    public RelayCommand NextDataSet { get; }
    public bool CanDoNextDataSet()
    {
        return CurrentFileDataSet != null && CurrentDataSetIndex < CurrentFileDataSet.DataSets.Count - 1;
    }
    public void DoNextDataSet()
    {
        CurrentDataSet = CurrentFileDataSet.DataSets[++CurrentDataSetIndex];
    }

    /*
    public RelayCommand PreviousPath { get; }
    public bool CanPreviousPath()
    {
        return CurrentDataSet != null && CurrentPathIndex > 0;
    }
    public void DoPreviousPath()
    {
        CurrentLastPathstep = CurrentDataSet.AllLastSteps[--CurrentPathIndex];
    }

    public RelayCommand NextPath { get; }
    public bool CanNextPath()
    {
        return CurrentDataSet != null && CurrentPathIndex < CurrentDataSet.AllLastSteps.Count - 1;
    }
    public void DoNextPath()
    {
        CurrentLastPathstep = CurrentDataSet.AllLastSteps[++CurrentPathIndex];
    }


    public RelayCommand FindPath { get; }
    public bool CanFindPath()
    {
        return CurrentFileDataSet != null;
    }
    public void DoFindPath()
    {
        var useCycles = false;
        if (CurrentFileDataSet.Level == 6 || CurrentFileDataSet.Level == 7)
        {
            useCycles = true;
        }

        _solver.FindPath(CurrentDataSet, useCycles);
        _solver.CreatePathfromSteps(CurrentDataSet);
        if (CurrentDataSet.AllLastSteps.Count > 0)
        {
            LastStepValid = CurrentDataSet.CorrectPathSteps.Last().IsValid.ToString();
            StepCount = CurrentDataSet.PathStepsCount;
        }
        Timing = _solver.Timing;

        DrawLawn(CurrentDataSet);
        Instructions = CurrentDataSet.InstructionString;

    }


    public RelayCommand FindPathNextStep { get; }
    public bool CanFindPathNextStep()
    {
        return true;
    }
    public void DoFindPathNextStep()
    {
        var useCycles = false;
        if (CurrentFileDataSet.Level == 6 || CurrentFileDataSet.Level == 7)
        {
            useCycles = true;
        }

        _solver.FindPathNextStep(CurrentDataSet, useCycles);
        _solver.CreateAllPaths(CurrentDataSet);

        // rectangle method
        if (CurrentDataSet.AllLastSteps.Count > 0)
        {
            AllPathCount = CurrentDataSet.AllLastSteps.Count;

            CurrentPathIndex = 0;
            CurrentLastPathstep = CurrentDataSet.AllLastSteps[CurrentPathIndex];
            LastStepValid = CurrentDataSet.CorrectPathSteps.Last().IsValid.ToString();
            StepCount = CurrentDataSet.PathStepsCount;
        }

        StepCount = CurrentDataSet.PathStepsCount;
        Timing = _solver.Timing;

        _solver.CreatePathfromSteps(CurrentDataSet);

        DrawLawn(CurrentDataSet);
    }

    public RelayCommand ClearPath { get; }
    public bool CanClearPath()
    {
        return true;
    }
    public void DoClearPath()
    {
        CurrentDataSet.ClearPath();
        StepCount = 0;
        Timing = 0;
        AllPathCount = 0;
        DrawLawn(CurrentDataSet);
    }
    */

    private void ParseData()
    {
        var inputPath = $"N:/Birgit/Coding/CatCoder/CCC41/Files/Level{Level}";
        var inputFiles = Directory.GetFiles(inputPath, "*.in");

        foreach (var file in inputFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            var levelName = $"Level {Level}";
            var FileDataSetName = fileName;

            var fileDataSet = new FileDataSet(Level, file);

            var levelNode = FilesCollection.FirstOrDefault(n => n.Name == levelName);
            if (levelNode == null)
            {
                levelNode = new ScenarioNode()
                {
                    Name = levelName
                };
                FilesCollection.Add(levelNode);
            }

            levelNode.Children.Add(new ScenarioNode()
            {
                Name = FileDataSetName,
                FileDataSet = fileDataSet
            });

        }
    }


    private void DrawDataSet(DataSet dataset)
    {
        _bitmap = new BitmapGridDrawing(dataset.Width, dataset.Height, _gridPositionSize, 1);
        _bitmap.BackgroundColor = Color.FromRgb(0, 0, 30);
        _bitmap.GridLineColor = Color.FromRgb(0, 0, 60);

        _bitmap.DrawBackGround();

        foreach (var area in dataset.ForbiddenAreas)
        {
            var gridPosition = dataset.GetGridPosition(area);
            _bitmap.FillGridCell((int)gridPosition.X, (int)gridPosition.Y, Color.FromRgb(60, 0, 0));
        }
        foreach (var asteroid in dataset.Asteroids)
        {
            var gridPosition = dataset.GetGridPosition(asteroid);
            _bitmap.DrawXInGridcell((int)gridPosition.X, (int)gridPosition.Y, _gridPositionSize, Color.FromRgb(255, 0, 0));
        }

        var gridStartPosition = dataset.GetGridPosition(dataset.StartPosition);
        _bitmap.FillGridCell((int)gridStartPosition.X, (int)gridStartPosition.Y, Color.FromRgb(0, 0, 150));
        _bitmap.DrawXInGridcell((int)gridStartPosition.X, (int)gridStartPosition.Y, _gridPositionSize, Color.FromRgb(0, 0, 200));
        var gridTargetPosition = dataset.GetGridPosition(dataset.TargetPosition);
        _bitmap.FillGridCell((int)gridTargetPosition.X, (int)gridTargetPosition.Y, Color.FromRgb(0, 150, 0));
        _bitmap.DrawXInGridcell((int)gridTargetPosition.X, (int)gridTargetPosition.Y, _gridPositionSize, Color.FromRgb(0, 0, 200));

        /*
        // fill pathing rectangles with different colors
        for (int s = 0; s < lawn.CorrectPathSteps.Count; s++)
        {
            var step = lawn.CorrectPathSteps[s];
            var rectangleColor = GetRectangleColor(s);

            foreach (var pos in step.Path)
            {
                _bitmap.FillGridCell((int)pos.X, (int)pos.Y, rectangleColor);
            }
        }

        if (lawn.Path.Count > 0)
        {
            var startPos = lawn.Path.First();
            _bitmap.DrawXInGridcell((int)startPos.X, (int)startPos.Y, _gridPositionSize, Color.FromRgb(255, 0, 0));
            _bitmap.DrawConnectedLines(lawn.Path, Color.FromRgb(255, 255, 0));

            var endPos = lawn.Path.Last();
            _bitmap.DrawXInGridcell((int)endPos.X, (int)endPos.Y, _gridPositionSize / 2, Color.FromRgb(255, 255, 0));
        }
        */
        Image = new Image();

        Image.Stretch = Stretch.None;
        Image.Margin = new Thickness(0);

        Image.Source = _bitmap.Picture;
        RaisePropertyChanged(nameof(Image));
    }

    private Color GetRectangleColor(int s)
    {
        var div = s % 5;

        return div switch
        {
            0 => Color.FromRgb(0, 12, 170),
            1 => Color.FromRgb(45, 19, 241),
            2 => Color.FromRgb(0, 210, 255),
            3 => Color.FromRgb(0, 240, 220),
            4 => Color.FromRgb(0, 133, 119),
            _ => throw new InvalidOperationException($"Unknown color {s}")
        };

    }
}
