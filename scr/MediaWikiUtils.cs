using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class MediaWikiUtils
{
    public static volatile bool run;

    public class WikiPage
    {
        public string id, title;
    }
    public class WikiPageContent
    {
        public string morph, value, giperonims, giponims;
    }

    private static string BuildPageUrlApi(string title) => "https://ru.wiktionary.org/w/api.php?action=parse&page=" + title + "&prop=text&format=json&formatversion=2";


    private static string BuildPageIdUrlApi(string id) => "https://ru.wiktionary.org/w/api.php?action=parse&pageid=" + id + "&prop=text&format=json&formatversion=2";

    private static string BuildCategoryUrlApi(string title, string _continue) =>
        "https://ru.wiktionary.org/w/api.php?action=query&format=json&list=categorymembers&cmtitle=Category:" + title + "&cmlimit=500&cmcontinue=" + _continue;

    private static JObject DownloadJson(string url)
    {
        WebClient client = new WebClient();
        string downloaded = client.DownloadString(url);
        return JObject.Parse(downloaded);
    }

    private static string StripHTML(string input) => Regex.Replace(input, "<.*?>", String.Empty);

    private static string SubstringFromTo(string f, string t, string input)
    {
        int startIndex = input.IndexOf(f);
        int endIndex = input.IndexOf(t, startIndex);
        return input.Substring(startIndex, endIndex - startIndex);
    }

    private static string PageContentDecode(string content)
    {
        string temp = WebUtility.HtmlDecode(StripHTML(content)).Trim('\n').Replace("Гипонимы[править]", "").Replace("Гиперонимы[править]", "");
        string[] lines = temp.Split('\n');
        string result = "";
        if (lines.Length == 1 & lines[0].Length <= 2)
        {
            return "\n";
        }
        int i = 0;
        foreach (string line in lines)
        {
            if (line.Length < 3)
            {
                continue;
            }
            result += String.Format("{0}. {1}\n", i + 1, line);
            i++;
        }
        return result;
    }

    private static int CountWords(string s, string s0) => (s.Length - s.Replace(s0, "").Length) / s0.Length;

    private static bool WordFilter(string title, WikiWordFilter filter)
    {
        if(filter.IgnoreWordsWithUpper & title.Any(char.IsUpper))
            return false;
        if (title.Length < filter.MinSymbols)
            return false;
        if(filter.IgnoreSameLetters)
        {
            if (title.Length < 5)
            {
                if (CountWords(title, title[0].ToString()) > 3)
                    return false;
            }
        }
        foreach (string sym in filter.IgnoreSymbols)
        {
            if (title.Contains(sym))
                return false;
        }
        return true;
    }

    public static bool PageFilter(WikiPageContent page, WikiPageFilter filter)
    {
        foreach(string m in filter.IgnoreMorphology)
        {
            if (page.morph.Contains(m))
                return false;
        }
        foreach (string m in filter.IgnoreValue)
        {
            if (page.value.Contains(m))
                return false;
        }

        string[] vals = page.value.Split('\n');
        foreach (string m in filter.IgnoreValueText)
        {
            int valueContainsCount = 0;
            foreach (string v in vals)
            {
                if (v.Contains(m))
                    valueContainsCount++;
            }
            if (valueContainsCount == vals.Length)
                return false;
        }

        foreach (string m in filter.IgnoreGiponims)
        {
            if (page.giponims.Contains(m))
                return false;
        }
        foreach (string m in filter.IgnoreGiperonims)
        {
            if (page.giperonims.Contains(m))
                return false;
        }
        return true;
    }

    private static WikiPageContent ParsePage(string id)
    {
        JObject json = DownloadJson(BuildPageUrlApi(id));
        string raw = json["parse"]["text"].ToString();
        // Морфологические и синтаксические свойства
        string morph = PageContentDecode(SubstringFromTo("<p><b>", "<h3>", raw));
        // Значение           
        string value = PageContentDecode(SubstringFromTo("</span></h4>", "<h4><span", raw));
        // Гипонимы
        string giponims = PageContentDecode(SubstringFromTo("Гипонимы</span><span", "<h3><span", raw));
        // Гиперонимы
        string giperonims = PageContentDecode(SubstringFromTo("Гиперонимы</span><span", "<h4><span", raw));


        return new WikiPageContent { morph = morph, value = value, giponims = giponims, giperonims = giperonims };
    }

    public static List<WikiPage> ParseCategory(string category, ref string progress, WikiWordFilter filter)
    {
        int counter = 0;
        string _continue = "";
        List<WikiPage> pages = new List<WikiPage>();
        while (run)
        {
            try
            {
                JObject json = DownloadJson(BuildCategoryUrlApi(category, _continue));
                var keys = json["query"]["categorymembers"].ToList<JToken>();
                foreach (JToken attribute in keys)
                {
                    string title = attribute["title"].ToString();
                    string id = attribute["pageid"].ToString();
                    if (WordFilter(title, filter))
                        pages.Add(new WikiPage { title = title, id = id });
                }
                if (json.Property("continue") == null)
                    break;
                _continue = json["continue"]["cmcontinue"].ToString();
                counter += 500;
                progress = "Downloading catalog: " + counter;
            }
            catch (Exception){ break; }
            
        }
        progress = "finished";
        return pages;
    }

    public static Dictionary<string, WikiPageContent> ParsePages(List<WikiPage> pages, ref string progress, WikiPageFilter filter)
    {
        var dict = new Dictionary<string, WikiPageContent>();
        int counter = 0;
        foreach (var page in pages)
        {
            if (!run)
                break;
            try
            {
                WikiPageContent currentPage = ParsePage(page.title);
                if (PageFilter(currentPage, filter))
                    dict.Add(page.title, currentPage);

            }
            catch (Exception) { }

            counter++;
            progress = String.Format("Downloading pages: {0}/{1}", counter, pages.Count);

        }

        progress = "finished";
        return dict;

    }





}

