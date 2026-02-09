using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{
    [SerializeField]
    private Button _pause;
    [SerializeField] private MenuPanel _pausePanel;
    public CharacterView _player;
    public CharacterView _enemy;
    
    private void Awake()
    {
        _pause.onClick.AddListener(PauseGame);
        
    }

    private void PauseGame()
    {
        _pausePanel.Open();
        //TODO Pause gameplay
    }
}