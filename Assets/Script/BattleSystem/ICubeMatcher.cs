using System.Collections.Generic;

namespace BattleSystem
{
    public interface ICubeMatcher
    {
        Pattern TryMatch(Dictionary<CubeSide, CubeFaceModel> face);
    }
}