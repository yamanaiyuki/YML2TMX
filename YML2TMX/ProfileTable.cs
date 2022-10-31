namespace Yml2Tmx;

public class ProfileTable
{
    public bool IsPause { get; set; } = false;

    public string Name { get; set; } = "yml2tmx";

    public string ChangeDate { get; set; } = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

    private string _sourceRoot = "";

    public string SourceRoot
    {
        get => _sourceRoot;
        set => _sourceRoot = ConvertParam(value);
    }

    private string _destinationRoot = "";

    public string DestinationRoot
    {
        get => _destinationRoot;
        set => _destinationRoot = ConvertParam(value);
    }

    private string _outputRoot = "";

    public string OutputRoot
    {
        get => _outputRoot;
        set => _outputRoot = ConvertParam(value);
    }

    public List<ProfileItem> Items { get; } = new();

    public string ConvertParam(string input)
    {
        string result = input;

        result = result.Replace("%UserProfile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));
        result = result.Replace("%Userprofile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));
        result = result.Replace("%userprofile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));
        result = result.Replace("%User_Profile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));
        result = result.Replace("%User_profile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));
        result = result.Replace("%user_profile%", Environment.ExpandEnvironmentVariables("%UserProfile%"));

        result = result.Replace("%SourceRoot%", SourceRoot);
        result = result.Replace("%Sourceroot%", SourceRoot);
        result = result.Replace("%sourceroot%", SourceRoot);
        result = result.Replace("%Source_Root%", SourceRoot);
        result = result.Replace("%Source_root%", SourceRoot);
        result = result.Replace("%source_root%", SourceRoot);

        result = result.Replace("%DestinationRoot%", DestinationRoot);
        result = result.Replace("%Destinationroot%", DestinationRoot);
        result = result.Replace("%destinationroot%", DestinationRoot);
        result = result.Replace("%Destination_Root%", DestinationRoot);
        result = result.Replace("%Destination_root%", DestinationRoot);
        result = result.Replace("%destination_root%", DestinationRoot);

        result = result.Replace("%OutputRoot%", OutputRoot);
        result = result.Replace("%Outputroot%", OutputRoot);
        result = result.Replace("%outputroot%", OutputRoot);
        result = result.Replace("%Output_Root%", OutputRoot);
        result = result.Replace("%Output_root%", OutputRoot);
        result = result.Replace("%output_root%", OutputRoot);

        return result;
    }
}
