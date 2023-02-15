namespace Yml2Tmx;

public class TUItem
{
    public string Key { get; set; } = "";

    public string Original { get; set; } = "";

    public string Translate { get; set; } = "";

    public bool Alternative { get; set; } = false;

    public string Prev { get; set; } = "";

    public string Next { get; set; } = "";

    // 前後の""とかインラインコメントを取り除く
    // TODO: 稀に末尾に空白がついてる原文がある
    public static string TrimValue(string value)
    {
        // 先頭から"を取り除く
        string result = value.StartsWith("\"") ? value[1..] : value;

        // 末端が"だった
        if (result.LastIndexOf("\"") == result.Length - 1)
        {
            return result[..^1];
        }

        // インラインコメントを取り除く
        int i = result.LastIndexOf("#");
        if (i < 0)
        {
            // コメントがみつからなかった
            // 末端に"をつけ忘れたと判断→何もしない
            return result;
        }

        // コメントと(あるかもしれない)空白を取り除く
        result = result[..(i - 1)].TrimEnd();

        // 末端が"なら取り除く
        return result.LastIndexOf("\"") == result.Length - 1 ? result[..^1] : result;
    }

    public static List<TUItem> CreateOriginal(string[] lines1)
    {
        List<TUItem> list = new();

        foreach (string line in lines1)
        {
            var trimmed = line.Trim();

            // 空行はスキップ
            if (trimmed.Length == 0)
            {
                continue;
            }

            // コメント行はスキップ
            if (trimmed.StartsWith("#"))
            {
                continue;
            }

            var splited = trimmed.Split(" ");

            // キーだけの場合はスキップ
            if (splited.Length < 2)
            {
                continue;
            }

            string key = splited[0];

            // 既に同じキーがある場合はスキップ
            if (list.Any(x => x.Key == key))
            {
                continue;
            }

            string value = string.Join(" ", splited.Skip(1));
            value = TrimValue(value);

            TUItem item = new()
            {
                Key = key,
                Original = value,
            };

            list.Add(item);
        }

        return list;
    }

    public static void AddTranslate(List<TUItem> list, string[] lines2)
    {
        foreach (var line in lines2)
        {
            var trimmed = line.Trim();

            // 空行はスキップ
            if (trimmed.Length == 0)
            {
                continue;
            }

            // コメント行はスキップ
            if (trimmed.StartsWith("#"))
            {
                continue;
            }

            string[] splited = trimmed.Split(" ");

            // キーだけの場合はスキップ
            if (splited.Length < 2)
            {
                continue;
            }

            string key = splited[0];

            foreach (TUItem item in list)
            {
                if (item.Key == key)
                {
                    string value = string.Join(" ", splited.Skip(1));
                    item.Translate = TrimValue(value);

                    break;
                }
            }
        }
    }

    // Translateが空行やスペースの場合は削除する
    public static List<TUItem> SkipEmptyTranslation(List<TUItem> list) => list.Where(x => !string.IsNullOrWhiteSpace(x.Translate)).ToList();

    // OriginalとTranslateが同じものだったら削除する
    // Trimや空行は判定しない
    public static List<TUItem> SkipSameTranslation(List<TUItem> list) => list.Where(x => x.Original != x.Translate).ToList();

    // 同じ値のものがないか探す(重複文節)
    //   同じものがなかったらdict_defへ追加する
    //   同じものだったらdict_altを検索し
    //    まだ同じものがなかったらdict_altへ追加する
    //    すでに同じものがあったら追加しない
    public static List<TUItem> Normalization(List<TUItem> list)
    {
        List<TUItem> result = new();
        List<TUItem> alt = new();

        string prev = "";
        string next = "";
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Original.Trim().Length == 0)
            {
                continue;
            }

            bool isFound = false;

            // resultに原文が追加済みか探す
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j].Original == list[i].Original)
                {
                    isFound = true;

                    // 訳文が一致するか確認する
                    // 原文、訳文ともに一致してれば処理しない
                    if (result[j].Translate != list[i].Translate)
                    {
                        // nextを先読みして探す
                        next = "";
                        for (int k = i + 1; k < list.Count; k++)
                        {
                            if (list[k].Original.Trim().Length != 0)
                            {
                                next = list[k].Original;
                                break;
                            }
                        }

                        // 原文は同じだが訳文は一致しなかった
                        // 今回をAlternative=trueでさらに追加する
                        TUItem item = new()
                        {
                            Original = list[i].Original,
                            Translate = list[i].Translate,
                            Alternative = true,
                            Prev = prev,
                            Next = next,
                        };

                        // すでに同じものがある場合は追加しない
                        if (!alt.Any(x => x.Original == item.Original
                                          && x.Translate == item.Translate
                                          && x.Prev == item.Prev
                                          && x.Next == item.Next))
                        {
                            alt.Add(item);
                            prev = item.Original;
                        }
                    }
                }
            }

            if (!isFound)
            {
                // resultに原文が見つからなかったので新規追加する
                TUItem item = new()
                {
                    Original = list[i].Original,
                    Translate = list[i].Translate
                };
                result.Add(item);
                prev = item.Original;
            }
        }

        result.AddRange(alt);
        return result;
    }
}