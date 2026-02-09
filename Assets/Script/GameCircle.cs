using UnityEngine;
//TODO State Machine
public class GameCircle : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private GameView _gameView;
    
    private void Awake()
    {
        _gameView._player.Setup(_gameConfig.Character[SaveManager.GetGameSaves().Character].Sprite);
        _gameView._enemy.Setup(_gameConfig.GetRandomEnemy().Sprite);
    }
}