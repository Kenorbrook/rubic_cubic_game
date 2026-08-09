using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BattleResultPanel : MonoBehaviour
{
    [SerializeField] private Button _playAgain;
    [SerializeField] private Button _menu;

    private void Awake()
    {
        if (_playAgain != null)
            _playAgain.onClick.AddListener(PlayAgain);
        if (_menu != null)
            _menu.onClick.AddListener(BackToMenu);
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
