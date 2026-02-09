using System.Collections.Generic;
using UnityEngine;

public class Matcher 
{
    
    private readonly List<Pattern> _patterns;
    private readonly PatternContext _context;

    public Matcher(
        List<Pattern> patterns,
        PatternContext context)
    {
        _patterns = patterns;
        _context = context;
    }

    public void OnCubeChanged(Dictionary<CubeSide, CubeFaceModel> face)
    {
        var front = face[CubeSide.Front];
        foreach (var pattern in _patterns)
        {
            if (TryMatch(pattern, front.Cells))
            {
                Debug.Log("I Find Patter SHIIIISH: " + pattern.name);
                ApplyPattern(pattern);
            }
        }
        
    }
    private bool TryMatch(Pattern pattern, CubeColor[,] face)
    {
        for (int oy = 0; oy < 3; oy++)
        for (int ox = 0; ox < 3; ox++)
        {
            if (MatchAt(pattern, face, ox, oy))
                return true;
        }

        return false;
    }
    bool MatchAt(Pattern pattern, CubeColor[,] face, int ox, int oy)
    {
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            var patternCell = pattern.cells[y * 3 + x];
            if (patternCell == CubeColor.Black)
                continue;
            if(x + ox>2 || y + oy>2)
                return false;
            if (!ColorMatches(face[y + oy, x + ox], patternCell))
                return false;
        }
        return true;
    }
    private bool ColorMatches(CubeColor cubeColor, CubeColor patternColor)
    {
        return cubeColor == patternColor;
    }
    void ApplyPattern(Pattern pattern)
    {

        foreach (var effect in pattern.effects)
            effect.Apply(_context);
    }

}
