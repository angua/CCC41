using System.Numerics;

namespace Common;

public class Filedata
{
    public List<string> Lines = new List<string>();

    private Filedata()
    { }

    public Filedata(string path)
    {
        var datalist = File.ReadAllLines(path);
        foreach (var line in datalist)
        {
            Lines.Add(line);
        }
    }

    public static Filedata CreateFromString(string str)
    {
        var fileData = new Filedata();
        fileData.Lines.AddRange(str.Split("\r\n"));
        return fileData;
    }

    public static Filedata CreateFromStream(TextReader tr)
    {
        var fileData = new Filedata();
        string line;
        while ((line = tr.ReadLine()) != null)
        {
            fileData.Lines.Add(line);
        }
        return fileData;
    }

    /// <summary>
    /// Create x,y character grid 
    /// </summary>
    /// <returns></returns>
    public static Dictionary<Vector2, char> ParseGrid(List<string> Lines)
    {
        var grid = new Dictionary<Vector2, char>();
        for (int y = 0; y < Lines.Count; y++)
        {
            var line = Lines[y];
            for (int x = 0; x < line.Length; x++)
            {
                var character = line[x];
                grid.Add(new Vector2(x, y), character);
            }
        }

        return grid;
    }

}
