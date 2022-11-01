using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "PDollar/Pattern Manager", order = 1)]
public class PDollarPatternManager : ScriptableObject
{
    public string Name;
    [TextArea(5, 15)]
    public string[] GestureXML;
}
