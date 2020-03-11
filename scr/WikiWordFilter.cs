using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WikiWordFilter", menuName = "WikiWordFilter", order = 51)]
[ExecuteInEditMode]
public class WikiWordFilter : ScriptableObject
{
    public string[] IgnoreSymbols = new string[] {":",".","-","_" };
    public int MinSymbols = 2;
    public bool IgnoreWordsWithUpper = true;
    public bool IgnoreSameLetters = true;
}
