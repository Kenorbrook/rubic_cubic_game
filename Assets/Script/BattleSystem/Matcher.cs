using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleSystem
{
    public class Matcher : ICubeMatcher
    {

        private readonly List<Pattern> _patterns;

        public Matcher(
            List<Pattern> patterns
        )
        {
            _patterns = patterns;
        }

        public Pattern TryMatch(Dictionary<CubeSide, CubeFaceModel> face)
        {
            var front = face[CubeSide.Front];
            return _patterns.FirstOrDefault(pattern => TryMatchPattern(pattern, front.Cells));
        }

        private static bool MatchAt(Pattern pattern, CubeColor[,] face, int ox, int oy)
        {
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                var patternCell = pattern.cells[y * 3 + x];
                if (patternCell == CubeColor.Black)
                    continue;
                if (x + ox > 2 || y + oy > 2)
                    return false;
                if (!ColorMatches(face[y + oy, x + ox], patternCell))
                    return false;
            }

            return true;
        }

        private static bool ColorMatches(CubeColor cubeColor, CubeColor patternColor)
        {
            return cubeColor == patternColor;
        }

        private bool TryMatchPattern(Pattern pattern, CubeColor[,] face)
        {
            for (int oy = 0; oy < 3; oy++)
            for (int ox = 0; ox < 3; ox++)
            {
                if (MatchAt(pattern, face, ox, oy))
                    return true;
            }
            return false;
        }
    }
}