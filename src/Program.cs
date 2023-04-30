using Newtonsoft.Json.Linq;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LLC_To_Paratranz
{
    public static class Program
    {
        static string Localize_Path;
        static string ParatranzWrok_Path;
        static int Localize_Path_Length;
        static int ParatranzWrok_Path_Length;
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((object o, UnhandledExceptionEventArgs e) => { File.WriteAllText("./Error.txt", o.ToString() + e.ToString()); });
            try
            {
                Localize_Path = new DirectoryInfo(File.ReadAllLines("./Debug/LLC_GitHubWrokLocalize_Path.txt")[0]).FullName;
                Localize_Path_Length = Localize_Path.Length + 3;
                ParatranzWrok_Path = new DirectoryInfo(File.ReadAllLines("./Debug/LLC_ParatranzWrok_Path.txt")[0]).FullName;
                ParatranzWrok_Path_Length = ParatranzWrok_Path.Length;
                LoadGitHubWroks(new DirectoryInfo(Localize_Path + "/EN"), en_dic);
                var RawNickNameObj = JSON.Parse(File.ReadAllText(Localize_Path + "/NickName.json")).AsObject;
                cn_dic["/RawNickName.json"] = RawNickNameObj;
                var NickNameObj = JSON.Parse(File.ReadAllText(Localize_Path + "/CN/CN_NickName.json")).AsObject;
                cn_dic["/NickName.json"] = NickNameObj;
                var Synchronous = JSON.Parse(File.ReadAllText(Localize_Path + "/synchronous-data_product.json")).AsObject;
                cn_dic["/Synchronous.json"] = Synchronous;
                if (args[0] == "ToParatranzWrokFull")
                    ToParatranzWrok(true);
                else if (args[0] == "ToGitHubWrok")
                {
                    LoadParatranzWroks(new DirectoryInfo(ParatranzWrok_Path), pt_dic);
                    ToGitHubWrok();
                }
                else if (args[0] == "ToParatranzWrok")
                    ToParatranzWrok(false);
            }
            catch (Exception ex)
            {
                File.WriteAllText("./Error.txt", ex.ToString());
            }
        }
        public static Dictionary<string, JSONObject> cn_dic = new Dictionary<string, JSONObject>();
        public static Dictionary<string, JSONObject> en_dic = new Dictionary<string, JSONObject>();
        public static Dictionary<string, JSONObject> jp_dic = new Dictionary<string, JSONObject>();
        public static Dictionary<string, JSONObject> kr_dic = new Dictionary<string, JSONObject>();
        public static Dictionary<string, JSONArray> pt_dic = new Dictionary<string, JSONArray>();
        public static void LoadGitHubWroks(DirectoryInfo directory, Dictionary<string, JSONObject> dic)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileName = fileInfo.DirectoryName.Remove(0, Localize_Path_Length) + "/" + fileInfo.Name.Remove(0, 3);
                dic[fileName] = JSON.Parse(value).AsObject;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
                LoadGitHubWroks(directoryInfo, dic);
        }
        static bool IsEnglishAndSymbols(this string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z\s\p{P}]+$");
        }
        public static void ToParatranzWrokNickName(Dictionary<string, JSONObject> NickNames, bool 输出翻译)
        {
            var RawNickNames = cn_dic["/RawNickName.json"][0].AsArray;
            JSONArray PT_NickName = new JSONArray();
            foreach (JSONObject rnnobj in RawNickNames.List.Cast<JSONObject>())
            {
                var NameKey = rnnobj[0].Value;
                bool cnhas = NickNames.TryGetValue(NameKey, out var cnobj);
                if (!cnhas)
                    NickNames[NameKey] = rnnobj;
                bool krhas = rnnobj.Dict.TryGetValue("krname", out _);
                bool kr2has = rnnobj.Dict.TryGetValue("nickName", out var krnickName);
                bool enhas = rnnobj.Dict.TryGetValue("enname", out var enname);
                bool en2has = rnnobj.Dict.TryGetValue("enNickName", out var ennickName);
                bool jphas = rnnobj.Dict.TryGetValue("jpname", out var jpname);
                bool jp2has = rnnobj.Dict.TryGetValue("jpNickName", out var jpnickName);

                JSONObject krnameobj = new JSONObject();
                krnameobj.Dict["key"] = NameKey + "-krname";
                krnameobj.Dict["original"] = NameKey;
                if (输出翻译)
                    if (cnhas && krhas)
                        krnameobj.Dict["translation"] = cnobj.Dict["krname"].Value;
                krnameobj.Dict["context"] = "EN :\n" + (enhas ? enname.Value : string.Empty) + "\nJP :\n" + (jphas ? jpname.Value : string.Empty);
                PT_NickName.Add(krnameobj);
                if (kr2has)
                {
                    JSONObject nickNameobj = new JSONObject();
                    nickNameobj.Dict["key"] = NameKey + "-nickName";
                    nickNameobj.Dict["original"] = krnickName;
                    if (输出翻译)
                        if (cnhas)
                            nickNameobj.Dict["translation"] = cnobj.Dict["nickName"].Value;
                    nickNameobj.Dict["context"] = "EN :\n" + (en2has ? ennickName.Value : string.Empty) + "\nJP :\n" + (jp2has ? jpnickName.Value : string.Empty);
                    PT_NickName.Add(nickNameobj);
                }
            }
            File.WriteAllText(ParatranzWrok_Path + "/NickName.json", JArray.Parse(PT_NickName.ToString()).ToString());
        }
        public static void ToParatranzWrokNone(Dictionary<string, JSONObject> NickNames, bool 输出翻译)
        {
            foreach (var kr_kvp in kr_dic)
            {
                var kr = kr_kvp.Value;
                var en = en_dic[kr_kvp.Key];
                var jp = jp_dic[kr_kvp.Key];
                bool cnhas = cn_dic.TryGetValue(kr_kvp.Key, out var cn);
                JSONArray ParatranzWrok = new JSONArray();
                var krobjs = kr[0].AsArray;
                var enobjs = en[0].AsArray;
                var jpobjs = jp[0].AsArray;
                Dictionary<string, JSONObject> cnobjs = new Dictionary<string, JSONObject>();
                if (cnhas)
                    foreach (var kvst in cn[0].AsArray)
                        cnobjs[kvst.Value[0].Value] = kvst.Value.AsObject;
                else
                    cnobjs = null;
                for (int i = 0; i < krobjs.Count; i++)
                {
                    var krobj = krobjs[i].AsObject;
                    var enobj = enobjs[i].AsObject;
                    var jpobj = jpobjs[i].AsObject;
                    string ObjectId = krobj[0];
                    if (ObjectId == "-1")
                        continue;
                    var cnobj = cnhas ? cnobjs.TryGetValue(krobj.Dict.Values.First(), out var c) ? c : new JSONObject() : null;
                    foreach (var keyValue in krobj.Dict)
                    {
                        if (!keyValue.Value.IsNumber)
                        {
                            JSONObject ParatranzObject = new JSONObject();
                            ParatranzObject.Dict["key"] = ObjectId + "-" + keyValue.Key;
                            if (keyValue.Key == "model")
                            {
                                if (NickNames.TryGetValue(keyValue.Value.Value, out var NickName))
                                {
                                    ParatranzObject.Dict["original"] = NickName[0];
                                    ParatranzObject.Dict["translation"] = NickName[1];
                                    ParatranzObject.Dict["context"] = "EN :\n" + NickName[3] + "\nJP :\n" + NickName[2];
                                }
                                else
                                {
                                    ParatranzObject.Dict["original"] = keyValue.Value.Value;
                                    ParatranzObject.Dict["translation"] = keyValue.Value.Value;
                                }
                                ParatranzWrok.Add(ParatranzObject);
                            }
                            else if (keyValue.Key != "id" && keyValue.Key != "usage")
                            {
                                if (keyValue.Value.IsString)
                                {
                                    ParatranzObject.Dict["original"] = keyValue.Value;
                                    if (输出翻译)
                                        if (cnhas)
                                            if (cnobj.Dict.TryGetValue(keyValue.Key, out var x))
                                            {
                                                string translation = x.Value;
                                                if (!translation.IsEnglishAndSymbols())
                                                    ParatranzObject.Dict["translation"] = translation;
                                            }
                                    ParatranzObject.Dict["context"] = "EN :\n" + enobj[keyValue.Key].Value + "\nJP :\n" + jpobj[keyValue.Key].Value;
                                }
                                else
                                {
                                    ParatranzObject.Dict["original"] = keyValue.Value.ToString();
                                    if (输出翻译)
                                        if (cnhas)
                                            if (cnobj.Dict.TryGetValue(keyValue.Key, out var x))
                                            {
                                                string translation = x.ToString();
                                                if (!translation.IsEnglishAndSymbols())
                                                    ParatranzObject.Dict["translation"] = translation;
                                            }
                                    ParatranzObject.Dict["context"] = "EN :\n" + enobj[keyValue.Key].ToString() + "\nJP :\n" + jpobj[keyValue.Key].ToString();
                                }
                                ParatranzWrok.Add(ParatranzObject);
                            }
                        }
                    }
                }
                string filePath = ParatranzWrok_Path + kr_kvp.Key;
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                File.WriteAllText(filePath, JArray.Parse(ParatranzWrok.ToString()).ToString());
            }
        }
        public static void ToParatranzWrokSynchronous()
        {
            var Synchronous = cn_dic["/Synchronous.json"][1].AsArray;
            JSONArray PT_Synchronous = new JSONArray();
            foreach (JSONObject rnnobj in Synchronous.List.Cast<JSONObject>())
            {
                var ObjectId = rnnobj[0].Value;
                JSONObject TitleObject = new JSONObject();
                TitleObject.Dict["key"] = ObjectId + "-title";
                TitleObject.Dict["original"] = rnnobj[6].Value;
                TitleObject.Dict["context"] = "EN :\n" + rnnobj[8].Value + "\nJP :\n" + rnnobj[10].Value;
                PT_Synchronous.Add(TitleObject);
                JSONObject ContentObject = new JSONObject();
                ContentObject.Dict["key"] = ObjectId + "-content";
                ContentObject.Dict["original"] = rnnobj[7].Value;
                ContentObject.Dict["context"] = "EN :\n" + rnnobj[9].Value + "\nJP :\n" + rnnobj[11].Value;
                PT_Synchronous.Add(ContentObject);
            }
            File.WriteAllText(ParatranzWrok_Path + "/Synchronous.json", JArray.Parse(PT_Synchronous.ToString()).ToString());
        }
        public static void ToParatranzWrok(bool 输出翻译)
        {
            if (输出翻译)
                LoadGitHubWroks(new DirectoryInfo(Localize_Path + "/CN"), cn_dic);
            LoadGitHubWroks(new DirectoryInfo(Localize_Path + "/JP"), jp_dic);
            LoadGitHubWroks(new DirectoryInfo(Localize_Path + "/KR"), kr_dic);
            if (Directory.Exists(ParatranzWrok_Path))
                Directory.Delete(ParatranzWrok_Path, true);
            Directory.CreateDirectory(ParatranzWrok_Path);
            Dictionary<string, JSONObject> NickNames = cn_dic["/NickName.json"][0].AsArray.List.ToDictionary(key => key[0].Value, value => value.AsObject);
            ToParatranzWrokNickName(NickNames, 输出翻译);

            ToParatranzWrokSynchronous();

            ToParatranzWrokNone(NickNames, 输出翻译);
        }
        public static void LoadParatranzWroks(DirectoryInfo directory, Dictionary<string, JSONArray> dic)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileName = fileInfo.DirectoryName.Remove(0, ParatranzWrok_Path_Length) + "/" + fileInfo.Name;
                dic[fileName] = JSON.Parse(value).AsArray;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
                LoadParatranzWroks(directoryInfo, dic);
        }
        public static void ToGitHubWrok()
        {
            if (Directory.Exists(Localize_Path + "/CN"))
                Directory.Delete(Localize_Path + "/CN", true);
            Directory.CreateDirectory(Localize_Path + "/CN");
            en_dic["/NickName.json"] = cn_dic["/RawNickName.json"];
            foreach (var pt_kvs in pt_dic)
            {
                var pt = pt_kvs.Value.List.ToDictionary(key => key[0].Value, value => value.AsObject);
                if (en_dic.TryGetValue(pt_kvs.Key, out var en))
                {
                    var enobjs = en[0].AsArray;
                    for (int i = 0; i < enobjs.Count; i++)
                    {
                        var enobj = enobjs[i].AsObject;
                        string ObjectId = enobj[0];
                        foreach (var keyValue in enobj.Dict.ToArray())
                        {
                            if (!keyValue.Value.IsNumber && keyValue.Key != "id" && keyValue.Key != "model" && keyValue.Key != "usage")
                            {
                                if (pt.TryGetValue(ObjectId + "-" + keyValue.Key, out var ptobj))
                                {
                                    if (!ptobj.Dict.TryGetValue("translation", out var translation) || string.IsNullOrEmpty(translation))
                                        continue;
                                    if (keyValue.Value.IsString)
                                        enobj[keyValue.Key].Value = ptobj[2].Value.Replace("\\n", "\n");
                                    else
                                        enobj.Dict[keyValue.Key] = JSON.Parse(ptobj[2].Value);
                                }
                            }
                        }
                    }
                    string filePath = Localize_Path + "/CN" + pt_kvs.Key;
                    string directoryPath = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(filePath, JObject.Parse(en.ToString()).ToString());
                    var file = new FileInfo(filePath);
                    file.MoveTo(directoryPath + "/CN_" + file.Name);
                }
            }
        }
    }
}
