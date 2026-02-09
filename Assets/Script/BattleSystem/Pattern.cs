using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Battle/Pattern")]
public class Pattern : ScriptableObject
{
    public CubeColor[] cells = new CubeColor[9]; // 3x3
    [SerializeReference]
    public List<Effect> effects= new();
}
