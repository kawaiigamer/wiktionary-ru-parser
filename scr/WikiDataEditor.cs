using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Web;
using System;

[CustomEditor(typeof(WikiData))]

public class WikiDataEditor : Editor
{
    private WikiData scriptableData;

    public override async void OnInspectorGUI()
    {
        scriptableData = (WikiData)target;

        DrawDefaultInspector();
        // ---- save
        GUILayout.Label("Words count: " + scriptableData.Dict.Count);
        GUILayout.Label("Json File Path");
        scriptableData.savePath = GUILayout.TextField(scriptableData.savePath);
        if (GUILayout.Button("Save words(json)"))
        {
            SerrializeDict(scriptableData.savePath);
        }
        if (GUILayout.Button("Load words(json)"))
        {
            DeserrializeDict(scriptableData.savePath);
            scriptableData.searchResults = new string[0];
            scriptableData.selectedIndex = 0;
        }
        GUILayout.Label("Words File Path");
        scriptableData.savePathPlain = GUILayout.TextField(scriptableData.savePathPlain);
        if (GUILayout.Button("Save words(txt)"))
        {
            SaveWordsAsPlainText(scriptableData.savePathPlain);
        }

        // ---- parse
        GUILayout.Label(scriptableData.progress);
        GUILayout.Label("Url");
        scriptableData.categoryUrl = GUILayout.TextField(scriptableData.categoryUrl);
        bool parse = GUILayout.Button("Parse");
        if (GUILayout.Button("Stop"))
        {
            MediaWikiUtils.run = false;
        }
        // ---- search
        GUILayout.Label("Search");
        scriptableData.searchStartWith = GUILayout.Toggle(scriptableData.searchStartWith, "Start With");
        scriptableData.searchWord = GUILayout.TextField(scriptableData.searchWord);

        if (GUILayout.Button("Search"))
        {
            if (scriptableData.searchStartWith)
            {
                var keys = from x in scriptableData.Dict
                           where x.Key.StartsWith(scriptableData.searchWord)
                           select x.Key;
                scriptableData.searchResults = keys.ToArray();
                Debug.Log(scriptableData.searchResults.Length);
            }

            if (!scriptableData.searchStartWith)
            {
                scriptableData.searchResults = new string[0];
                scriptableData.selectedIndex = 0;
                if (scriptableData.Dict.ContainsKey(scriptableData.searchWord))
                {                    
                    var data = scriptableData.Dict[scriptableData.searchWord];
                    scriptableData.searchResult = WordFormat(data);
                }
                else
                {
                    scriptableData.searchResult = "Not found";
                }

            }


        }
        GUILayout.TextArea(scriptableData.searchResult, GUILayout.Height(230));
        scriptableData.selectedIndex = GUILayout.SelectionGrid(scriptableData.selectedIndex, scriptableData.searchResults, 1);
        if (scriptableData.searchResults.Length > 0)
        {
            try
            {
                string word = scriptableData.searchResults[scriptableData.selectedIndex];
                if (scriptableData.Dict.ContainsKey(word))
                    scriptableData.searchResult = WordFormat(scriptableData.Dict[word]);
            }
            catch (Exception)
            {
                Debug.Log(scriptableData.selectedIndex);
            }

        } 


        // ---- async
        if (parse)
        {
            if (MediaWikiUtils.run)
                return;
            MediaWikiUtils.run = true;
            string url = HttpUtility.UrlDecode(scriptableData.categoryUrl);
            string category = url.Substring(url.LastIndexOf(":") + 1);
            Debug.Log(category);
            var pages = await Task.Run(() => MediaWikiUtils.ParseCategory(category, ref scriptableData.progress, scriptableData.WordFilter));
            var dict = await Task.Run(() => MediaWikiUtils.ParsePages(pages, ref scriptableData.progress, scriptableData.PageFilter));
            dict.ToList().ForEach(x =>
            {
                if (!scriptableData.Dict.ContainsKey(x.Key))
                    scriptableData.Dict.Add(x.Key, x.Value);
            }
            );

        }


    }

    private void SaveWordsAsPlainText(string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        foreach(string key in scriptableData.Dict.Keys)
        {
           writer.WriteLine(key);
        }
        writer.Close();
    }

    private string WordFormat(MediaWikiUtils.WikiPageContent page)
    {
        return new StringBuilder()
                    .Append("Морфологические и синтаксические свойства\n")
                    .Append(page.morph)
                    .Append("Значение\n")
                    .Append(page.value)
                    .Append("Гипонимы\n")
                    .Append(page.giponims)
                    .Append("Гиперонимы\n")
                    .Append(page.giperonims)
                    .ToString();
    }


    private void SerrializeDict(string path)
    {
        string json = JsonConvert.SerializeObject(scriptableData.Dict, Formatting.Indented);
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(json);
        writer.Close();

    }

    private void DeserrializeDict(string path)
    {
        if (!File.Exists(path))
            return;
        StreamReader reader = new StreamReader(path);
        scriptableData.Dict = JsonConvert.DeserializeObject<Dictionary<string, MediaWikiUtils.WikiPageContent>>(reader.ReadToEnd());
        reader.Close();

    }

}
