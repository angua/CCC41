using CCC41Lib;

var level = 1;

WriteOutputs(level);

void WriteOutputs(int level)
{
    var solver = new Solver();

    for (var inputFileNumber = 1; inputFileNumber <= 5; inputFileNumber++)
    {
        var inputfilename = $"../../../../Files/Level{level}/{level}_{inputFileNumber}.in";
        var lines = File.ReadAllLines(inputfilename).ToList();

        var outputfilename = $"../../../../Files/Level{level}/level{level}_{inputFileNumber}.out";
        using var outputWriter = new StreamWriter(outputfilename);

        Console.WriteLine($"Level {level} File {inputFileNumber}");
        var output = solver.Solve(level, lines);

        outputWriter.Write(output);
    }
}
