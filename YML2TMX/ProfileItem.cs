using Tommy;

namespace Yml2Tmx;

public class ProfileItem
{
    ProfileTable? Table { get; set; }

    public string Format { get; set; } = "tm";

    private string _source = "";

    public string Source
    {
        get => _source;
        set => _source = Table != null ? Table.ConvertParam(value) : value;
    }

    private string _destination = "";

    public string Destination
    {
        get => _destination;
        set => _destination = Table != null ? Table.ConvertParam(value) : value;
    }

    private string _output = "";

    public string Output
    {
        get => _output;
        set => _output = Table != null ? Table.ConvertParam(value) : value;
    }

    public bool OutputIsFile { get; set; } = false;

    public List<string> Excludes { get; } = new();

    public bool SkipSameTranslation { get; set; } = true;
}
