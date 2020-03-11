using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WikiData", menuName = "WikiData", order = 51)]
[ExecuteInEditMode]

public class WikiData : ScriptableObject
{
    [SerializeField]
    public Dictionary<string, MediaWikiUtils.WikiPageContent> Dict = new Dictionary<string, MediaWikiUtils.WikiPageContent>();
    public WikiWordFilter WordFilter;
    public WikiPageFilter PageFilter;

    [HideInInspector]
    [SerializeField]
    internal string categoryUrl = "";
    [HideInInspector]
    [SerializeField]
    internal string savePathPlain = "Assets/Words.txt";
    internal string progress = "progress";
    internal string searchWord = "";
    internal string searchResult = "";
    [HideInInspector]
    [SerializeField]
    internal string savePath = "Assets/Dictionary.json";
    internal string[] searchResults = new string[0];
    internal bool searchStartWith;
    internal int selectedIndex;

}

