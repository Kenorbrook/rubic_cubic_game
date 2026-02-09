using UnityEngine;

/// <summary>
/// Вспомогательный класс для конвертации CubeColor в Unity Color
/// </summary>
public static class CubeColorHelper
{
    public static Color ToUnityColor(this CubeColor cubeColor)
    {
        switch (cubeColor)
        {
            case CubeColor.White:
                return Color.white;
            case CubeColor.Yellow:
                return Color.yellow;
            case CubeColor.Red:
                return Color.red;
            case CubeColor.Orange:
                return new Color(1f, 0.5f, 0f); // Оранжевый
            case CubeColor.Blue:
                return Color.blue;
            case CubeColor.Green:
                return Color.green;
            default:
                return Color.black;
        }
    }

    public static CubeColor ToCubeColor(this Color cubeColor)
    {
        if (cubeColor == Color.white)
            return CubeColor.White;
        if (cubeColor == Color.yellow)
            return CubeColor.Yellow;
        if (cubeColor == Color.red)
            return CubeColor.Red;
        if (cubeColor == new Color(1f, 0.5f, 0f))
            return CubeColor.Orange; // Оранжевый
        if (cubeColor == Color.blue)
            return CubeColor.Blue;
        if (cubeColor == Color.green)
            return CubeColor.Green;
        return CubeColor.Black;
    }
}

