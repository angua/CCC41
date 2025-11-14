using CCC41Lib;

var level = 2;

WriteOutputs(level);

void WriteOutputs(int level)
{
    var folder = $"N:/Birgit/Coding/CatCoder/CCC41/Files";

    var solver = new Solver();

    foreach (var inputfilename in new List<string> { $"{folder}/Level{level}/level{level}_1_small.in", $"{folder}/Level{level}/level{level}_2_large.in", $"{folder}/Level{level}/level{level}_0_example.in" })
    {
        var lines = File.ReadAllLines(inputfilename).ToList();

        var outputfilename = inputfilename.Replace(".in", ".out");
        using var outputWriter = new StreamWriter(outputfilename);

        Console.WriteLine($"Level {level} File {inputfilename}");
        var output = solver.Solve(level, lines);

        outputWriter.Write(output);
    }
}
