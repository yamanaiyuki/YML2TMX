using System.Diagnostics;
using System.Text;
using System.Xml;
using Tommy;

namespace Yml2Tmx;

public class YmlConverter
{
    readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    readonly FileVersionInfo ver = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
    string changename = "yml2tmx";
    string changedate = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

    public void ConvertTML(string infile1, string infile2, string outfile, bool skipsame = true)
    {
        logger.Debug($"infile1={infile1}, infile2={infile2}, outfile={outfile}");

        if (!File.Exists(infile1))
        {
            logger.Error($"Error: Could not find file '{infile1}'");
            return;
        }
        if (!File.Exists(infile2))
        {
            logger.Error($"Error: Could not find file '{infile2}'");
            return;
        }

        // キーと値の辞書を作成する

        string[] lines1 = File.ReadAllLines(infile1);
        List<TUItem> items = TUItem.CreateOriginal(lines1);

        string[] lines2 = File.ReadAllLines(infile2);
        TUItem.AddTranslate(items, lines2);

        // 訳文が空、もしくは空白で埋められてた
        items = TUItem.SkipEmptyTranslation(items);

        if (skipsame)
        {
            // 原文と同一な訳文は追加しない
            items = TUItem.SkipSameTranslation(items);
        }

        // 重複を整理する
        items = TUItem.Normalization(items);

        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
        };

        XmlWriter writer = XmlWriter.Create(outfile, settings);

        writer.WriteDocType("tmx", null, "tmx11.dtd", null);
        writer.WriteStartElement("tmx");
        writer.WriteStartAttribute("version");
        writer.WriteValue("1.1");
        writer.WriteEndAttribute();

        /*
        <header creationtool="OmegaT" o-tmf="OmegaT TMX" adminlang="EN-US" datatype="plaintext" creationtoolversion="4.3.3_0_d8ab25dd" segtype="sentence" srclang="EN-US"/>
        */
        writer.WriteStartElement("header");
        writer.WriteStartAttribute("creationtool");
        writer.WriteValue(ver.ProductName);
        writer.WriteEndAttribute();
        writer.WriteStartAttribute("o-tmf");
        writer.WriteValue("OmegaT TMX");
        writer.WriteEndAttribute();
        writer.WriteStartAttribute("adminlang");
        writer.WriteValue("EN-US");
        writer.WriteEndAttribute();
        writer.WriteStartAttribute("datatype");
        writer.WriteValue("plaintext");
        writer.WriteEndAttribute();
        writer.WriteStartAttribute("creationtoolversion");
        writer.WriteValue(ver.ProductVersion);
        writer.WriteEndAttribute();
        writer.WriteStartAttribute("segtype");
        writer.WriteValue("sentence");
        writer.WriteStartAttribute("srclang");
        writer.WriteValue("EN-US");
        writer.WriteEndAttribute();
        writer.WriteEndElement(); // end header

        writer.WriteStartElement("body");

        // 基本訳文
        writer.WriteComment("Default translations");

        foreach (TUItem item in items)
        {
            // 最初のAltがみつかったら打ち切る
            if (item.Alternative)
            {
                break;
            }

            //logger.Debug($"dict_def: {key} {value}");
            /*
            <tu>
              <tuv lang="EN-US">
                <seg>#T $carn_enslave_interaction$#!</seg>
              </tuv>
              <tuv lang="JA" changeid="yamanaiyuki" changedate="20210412T034734Z" creationid="yamanaiyuki" creationdate="20210412T034734Z">
                <seg>#T $carn_enslave_interaction$#!</seg>
              </tuv>
            </tu>
            */
            writer.WriteStartElement("tu");
            {
                writer.WriteStartElement("tuv");
                {
                    writer.WriteStartAttribute("lang");
                    writer.WriteValue("EN-US");
                    writer.WriteEndAttribute(); //lang
                    {
                        writer.WriteStartElement("seg");
                        writer.WriteValue(item.Original);
                        writer.WriteEndElement(); //seg
                    }
                }
                writer.WriteEndElement(); //tuv

                writer.WriteStartElement("tuv");
                {
                    writer.WriteStartAttribute("lang");
                    writer.WriteValue("JA");
                    writer.WriteEndAttribute(); //lang
                    writer.WriteStartAttribute("changeid");
                    writer.WriteValue(changename);
                    writer.WriteEndAttribute(); //changeid
                    writer.WriteStartAttribute("changedate");
                    writer.WriteValue(changedate);
                    writer.WriteEndAttribute(); //changedate
                    writer.WriteStartAttribute("creationid");
                    writer.WriteValue(changename);
                    writer.WriteEndAttribute(); //creationid
                    writer.WriteStartAttribute("creationdate");
                    writer.WriteValue(changedate);
                    writer.WriteEndAttribute(); //creationdate
                    {
                        writer.WriteStartElement("seg");
                        writer.WriteValue(item.Translate);
                        writer.WriteEndElement(); //seg
                    }
                }
                writer.WriteEndElement(); //tuv
            }
            writer.WriteEndElement(); //tu
        }

        // 例外訳文
        writer.WriteComment("Alternative translations");

        string filename = Path.GetFileName(infile1);

        foreach (TUItem item in items)
        {
            // Altまでスキップ
            if (!item.Alternative)
            {
                continue;
            }

            //logger.Debug($"dict_alt: {key} {string.Join(",", values)}");
            /*
            <tu>
                <prop type="file">viet_artifact_events_l_english.yml</prop>
                <prop type="prev">Buy the yogurt recipe from the host</prop>
                <prop type="next">Send a servant to spy on the host and learn it</prop>
                <tuv lang="EN-US">
                <seg>Negotiate to pay a lower price for it</seg>
                </tuv>
                <tuv lang="JA" changeid="yamanaiyuki" changedate="20220817T092534Z" creationid="yamanaiyuki" creationdate="20220817T091642Z">
                <seg>実に魅力的な提案だが、いくらか値切れないか？</seg>
                </tuv>
            </tu>
            */
            writer.WriteStartElement("tu");
            {
                writer.WriteStartElement("prop");
                {
                    writer.WriteStartAttribute("type");
                    writer.WriteValue("file");
                    writer.WriteEndAttribute(); //type
                    writer.WriteValue(filename);
                }
                writer.WriteEndElement(); //prop

                writer.WriteStartElement("prop");
                {
                    writer.WriteStartAttribute("type");
                    writer.WriteValue("prev");
                    writer.WriteEndAttribute(); //prev
                    writer.WriteValue(item.Prev);
                }
                writer.WriteEndElement(); //prop

                writer.WriteStartElement("prop");
                {
                    writer.WriteStartAttribute("type");
                    writer.WriteValue("next");
                    writer.WriteEndAttribute(); //next
                    writer.WriteValue(item.Next);
                }
                writer.WriteEndElement(); //prop

                writer.WriteStartElement("tuv");
                {
                    writer.WriteStartAttribute("lang");
                    writer.WriteValue("EN-US");
                    writer.WriteEndAttribute(); //lang
                    {
                        writer.WriteStartElement("seg");
                        writer.WriteValue(item.Original);
                        writer.WriteEndElement(); //seg
                    }
                }
                writer.WriteEndElement(); //tuv

                writer.WriteStartElement("tuv");
                {
                    writer.WriteStartAttribute("lang");
                    writer.WriteValue("JA");
                    writer.WriteEndAttribute(); //lang
                    writer.WriteStartAttribute("changeid");
                    writer.WriteValue(changename);
                    writer.WriteEndAttribute(); //changeid
                    writer.WriteStartAttribute("changedate");
                    writer.WriteValue(changedate);
                    writer.WriteEndAttribute(); //changedate
                    writer.WriteStartAttribute("creationid");
                    writer.WriteValue(changename);
                    writer.WriteEndAttribute(); //creationid
                    writer.WriteStartAttribute("creationdate");
                    writer.WriteValue(changedate);
                    writer.WriteEndAttribute(); //creationdate
                    {
                        writer.WriteStartElement("seg");
                        writer.WriteValue(item.Translate);
                        writer.WriteEndElement(); //seg
                    }
                }
                writer.WriteEndElement(); //tuv
            }
            writer.WriteEndElement(); //tu
        }

        writer.WriteEndElement(); //body
        writer.WriteEndElement(); //tmx

        writer.Flush();
    }

    public void ConvertGlossary(string infile1, string infile2, string outfile, bool skipsame = true)
    {
        logger.Debug($"infile1={infile1}, infile2={infile2}, outfile={outfile}");

        if (!File.Exists(infile1))
        {
            logger.Error($"Error: Could not find file '{infile1}'");
            return;
        }
        if (!File.Exists(infile2))
        {
            logger.Error($"Error: Could not find file '{infile2}'");
            return;
        }

        string[] lines1 = File.ReadAllLines(infile1);
        List<TUItem> items = TUItem.CreateOriginal(lines1);

        string[] lines2 = File.ReadAllLines(infile2);
        TUItem.AddTranslate(items, lines2);

        Dictionary<string, string> glossary = new();

        foreach (var item in items)
        {
            // 原文が空の場合はスキップ
            if (item.Original.Length == 0)
            {
                continue;
            }

            // 原文と訳文が同一ならスキップ
            if (skipsame && item.Original == item.Translate)
            {
                continue;
            }

            // 特定の記号($|#![])が入ってる場合はスキップ
            string exclusion = "$|#!@[]";
            if (exclusion.Any(x => item.Original.Contains(x)))
            {
                continue;
            }

            // 訳文が空、もしくは空白で埋められてた
            if (item.Translate.Trim().Length == 0)
            {
                continue;
            }

            // すでに登録済み
            if (glossary.ContainsKey(item.Original))
            {
                continue;
            }

            glossary.Add(item.Original, item.Translate);
        }

        using var writer = new StreamWriter(outfile, false, Encoding.UTF8);
        foreach (var (key, value) in glossary)
        {
            writer.WriteLine($"{key}\t{value}");
        }
    }

    public void ConvertToml(string infile)
    {
        logger.Debug($"infile={infile}");

        if (!File.Exists(infile))
        {
            logger.Error($"Error: Could not find file '{infile}'");
            return;
        }

        // Tomlを解析してProfileリストを得る
        ProfileTable profile = ParseToml(infile);

        changename = profile.Name;
        changedate = profile.ChangeDate;

        // Profile毎の処理
        for (int i = 0; i < profile.Items.Count; i++)
        {
            ProfileItem item = profile.Items[i];
            string format = item.Format.ToLower();


            if (format != "tm" && format != "tmx" && format != "glossary")
            {
                // 存在しないフォーマット
                logger.Error($"Profile({i}): Invalid Convert Format'{item.Format}'");
                continue;
            }

            bool src_is_dir = Directory.Exists(item.Source);
            bool src_is_file = File.Exists(item.Source);
            bool dst_is_dir = Directory.Exists(item.Destination);
            bool dst_is_file = File.Exists(item.Destination);

            if (!src_is_dir && !src_is_file)
            {
                // 存在しないSource
                logger.Error($"Profile({i}): Not Found Source '{item.Source}'");
                continue;
            }
            if (!dst_is_dir && !dst_is_file)
            {
                // 存在しないDestination
                logger.Error($"Profile({i}): Not Found Destination '{item.Destination}'");
                continue;
            }

            // Source→dir Destination→dir 再帰ループ処理
            // Source→file Destination→file 単一処理
            // Source→dir Destination→file エラー
            // Source→file Destination→dir エラー

            if (src_is_dir && dst_is_file)
            {
                // 一致していない
                logger.Error($"Profile({i}): Type not matched: Source = Directory, Destination = File");
                logger.Error($"Profile({i}): Source = '{item.Source}'");
                logger.Error($"Profile({i}): Destination = '{item.Destination}'");
                continue;
            }
            if (src_is_file && dst_is_dir)
            {
                // 一致していない
                logger.Error($"Profile({i}): Type not matched: Source=File, Destination=Directory");
                logger.Error($"Profile({i}): Sourc e ='{item.Source}'");
                logger.Error($"Profile({i}): Destination = '{item.Destination}'");
                continue;
            }

            if (src_is_file && dst_is_file)
            {
                Message.PrintMessage(item.Source);

                string output;
                if (item.OutputIsFile)
                {
                    output = item.Output;
                }
                else
                {
                    // 末尾がtmxかtxtの場合はoutput_is_file=trueを忘れたのでは？と類推する
                    string ext = Path.GetExtension(item.Output);
                    if (ext == ".tmx" || ext == ".txt")
                    {
                        output = item.Output;
                    }
                    else
                    {
                        // 出力ファイル名がなかったのでSourceから取得して設定
                        // 拡張子を変更する
                        output = Path.Combine(item.Output, Path.GetFileName(item.Source));
                        if (format == "tm" || format == "tmx")
                        {
                            output = Path.ChangeExtension(output, ".tmx");
                        }
                        else
                        {
                            output = Path.ChangeExtension(output, ".txt");
                        }
                    }
                }

                CreateOutputDirectory(output);

                if (format == "tm" || format == "tmx")
                {
                    ConvertTML(item.Source, item.Destination, output, item.SkipSameTranslation);
                }
                else
                {
                    ConvertGlossary(item.Source, item.Destination, output, item.SkipSameTranslation);
                }

                continue;
            }

            if (src_is_dir && dst_is_dir)
            {
                // ファイル一覧を得る
                int src_length = item.Source.Length;
                int dst_length = item.Destination.Length;
                var sources = Directory.EnumerateFiles(item.Source, "*.yml", SearchOption.AllDirectories).Select(x => x[item.Source.Length..]);
                var destinations = Directory.EnumerateFiles(item.Destination, "*.yml", SearchOption.AllDirectories).Select(x => x[item.Destination.Length..]);

                Message.MaxCount = sources.Count();
                int progress = 0;

                foreach (var it in sources)
                {
                    Message.PrintProgress(++progress, it);

                    // 除外するファイル名だった
                    if (item.Excludes.Any(x => it.EndsWith(x)))
                    {
                        continue;
                    }

                    if (destinations.Contains(it))
                    {
                        string source = item.Source + it;
                        string destination = item.Destination + it;
                        string output = item.Output + it;

                        if (format == "tm" || format == "tmx")
                        {
                            output = Path.ChangeExtension(output, ".tmx");
                            CreateOutputDirectory(output);
                            ConvertTML(source, destination, output, item.SkipSameTranslation);
                        }
                        else
                        {
                            output = Path.ChangeExtension(output, ".txt");
                            CreateOutputDirectory(output);
                            ConvertGlossary(source, destination, output, item.SkipSameTranslation);
                        }
                    }
                }

                Message.PrintProgressEnd();
            }
        }

        // 終了前に停止する
        if (profile.IsPause)
        {
            Message.ReadKey("== Press any key to end. ==");
        }
    }

    private ProfileTable ParseToml(string infile)
    {
        // Tomlを解析してProfileリストを得る
        TomlTable table;
        ProfileTable profile = new();

        using StreamReader reader = File.OpenText(infile);
        try
        {
            table = TOML.Parse(reader);

            if (table["pause_when_finish"] is TomlBoolean pause_when_finish)
            {
                profile.IsPause = pause_when_finish;
            }
            if (table["changename"] is TomlString changename)
            {
                profile.Name = changename;
            }
            if (table["changedate"] is TomlString changedate)
            {
                profile.ChangeDate = changedate;
            }
            if (table["source_root"] is TomlString source_root)
            {
                profile.SourceRoot = source_root;
            }
            if (table["destination_root"] is TomlString destination_root)
            {
                profile.DestinationRoot = destination_root;
            }
            if (table["output_root"] is TomlString output_root)
            {
                profile.OutputRoot = output_root;
            }

            if (table["profiles"] is TomlArray items)
            {
                if (items.IsTableArray)
                {
                    foreach (TomlTable item in items)
                    {
                        ProfileItem it = new();
                        if (item["format"] is TomlString format)
                        {
                            it.Format = format;
                        }
                        if (item["source"] is TomlString source)
                        {
                            it.Source = profile.ConvertParam(source);
                        }
                        if (it.Source == "")
                        {
                            it.Source = profile.ConvertParam(profile.SourceRoot);
                        }
                        if (item["destination"] is TomlString destination)
                        {
                            it.Destination = profile.ConvertParam(destination);
                        }
                        if (it.Destination == "")
                        {
                            it.Destination = profile.ConvertParam(profile.DestinationRoot);
                        }
                        if (item["output"] is TomlString output)
                        {
                            it.Output = profile.ConvertParam(output);
                        }
                        if (it.Output == "")
                        {
                            it.Output = profile.ConvertParam(profile.OutputRoot);
                        }
                        if (item["output_is_file"] is TomlBoolean output_is_file)
                        {
                            it.OutputIsFile = output_is_file;
                        }
                        if (item["excludes"] is TomlArray excludes)
                        {
                            foreach (var i in excludes.Children)
                            {
                                it.Excludes.Add(i);
                            }
                        }
                        if (item["skip_same_translation_as_source"] is TomlBoolean skip_same_translation_as_source)
                        {
                            it.SkipSameTranslation = skip_same_translation_as_source;
                        }

                        profile.Items.Add(it);
                    }
                }
            }
        }
        catch (TomlParseException e)
        {
            logger.Error(e.Message);

            table = e.ParsedTable;

            logger.Error($"(Line, Column): Message");
            logger.Error($"=======================");
            foreach (TomlSyntaxException ex in e.SyntaxErrors)
            {
                logger.Error($"({ex.Line}, {ex.Column}): {ex.Message}");
            }
        }

        return profile;
    }

    // 必要に応じてディレクトリを作成する
    private static void CreateOutputDirectory(string outfile)
    {
        string? outdir = Path.GetDirectoryName(outfile);

        if (!string.IsNullOrEmpty(outdir))
        {
            Directory.CreateDirectory(outdir);
        }
    }
}
