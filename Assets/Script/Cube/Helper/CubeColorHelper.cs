using UnityEngine;

/// <summary>
/// Вспомогательный класс для конвертации CubeColor в Unity Color
/// </summary>
public static class CubeColorHelper
{
    private static readonly Color WhiteTone = new(0.96f, 0.97f, 0.99f);
    private static readonly Color YellowTone = new(0.98f, 0.82f, 0.26f);
    private static readonly Color RedTone = new(0.85f, 0.24f, 0.24f);
    private static readonly Color OrangeTone = new(0.95f, 0.52f, 0.21f);
    private static readonly Color BlueTone = new(0.22f, 0.49f, 0.86f);
    private static readonly Color GreenTone = new(0.18f, 0.67f, 0.44f);

    public static Color ToUnityColor(this CubeColor cubeColor)
    {
        switch (cubeColor)
        {
            case CubeColor.White:
                return WhiteTone;
            case CubeColor.Yellow:
                return YellowTone;
            case CubeColor.Red:
                return RedTone;
            case CubeColor.Orange:
                return OrangeTone;
            case CubeColor.Blue:
                return BlueTone;
            case CubeColor.Green:
                return GreenTone;
            default:
                return Color.black;
        }
    }

    public static CubeColor ToCubeColor(this Color cubeColor)
    {
        if (cubeColor == WhiteTone)
            return CubeColor.White;
        if (cubeColor == YellowTone)
            return CubeColor.Yellow;
        if (cubeColor == RedTone)
            return CubeColor.Red;
        if (cubeColor == OrangeTone)
            return CubeColor.Orange; 
        if (cubeColor == BlueTone)
            return CubeColor.Blue;
        if (cubeColor == GreenTone)
            return CubeColor.Green;
        return CubeColor.Black;
    }
}

