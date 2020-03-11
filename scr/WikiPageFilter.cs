using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WikiPageFilter", menuName = "WikiPageFilter", order = 51)]
[ExecuteInEditMode]
public class WikiPageFilter : ScriptableObject
{
    public string[] IgnoreMorphology = new string[] { "Предлог;", "Частица;", "Топоним;" };
    public string[] IgnoreValue = new string[] { "устар." };
    public string[] IgnoreValueText = new string[] { "Отсутствует пример употребления" };
    public string[] IgnoreGiponims = new string[] { "буква" };
    public string[] IgnoreGiperonims = new string[] {  };
}
