using System.Diagnostics;
using Yml2Tmx;

NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
FileVersionInfo? ver = null;

try
{
    ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
    logger.Debug($"{ver.ProductName} Version {ver.ProductVersion}");

    YmlConverter yml = new();

    logger.Debug($"args.Length={args.Length}");
    for (int i = 0; i < args.Length; i++)
    {
        logger.Debug($"args[{i}] = {args[i]}");
    }

    if (args.Length == 3)
    {
        yml.ConvertTML(args[0], args[1], args[2]);
        return;
    }

    if (args.Length == 4)
    {
        if (args[0] == "-g")
        {
            yml.ConvertGlossary(args[1], args[2], args[3]);
            return;
        }
        else if (args[2] == "-g")
        {
            yml.ConvertGlossary(args[0], args[1], args[3]);
            return;
        }
        else if (args[3] == "-g")
        {
            yml.ConvertGlossary(args[0], args[1], args[2]);
            return;
        }
    }

    if (args.Length == 1)
    {
        yml.ConvertToml(args[0]);
        return;
    }
}
catch (Exception e)
{
    logger.Error(e);
}

if (ver != null)
{
    Console.WriteLine($"{ver.ProductName} Version {ver.ProductVersion}");
    Console.WriteLine("");
}
Console.WriteLine("Usage:");
Console.WriteLine("yml2tmx infile_l_english.yml infile_l_japanese.yml outfile.tmx");
Console.WriteLine("yml2tmx infile_l_english.yml infile_l_japanese.yml glossary.txt -g");
Console.WriteLine("yml2tmx files.toml");
