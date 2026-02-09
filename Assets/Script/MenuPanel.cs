using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class MenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button _continue;

    [SerializeField]
    private Button _menu;

    private void Awake()
    {
        _continue.onClick.AddListener(ContinueGame);
        _menu.onClick.AddListener(BackToMenu);
    }
    
    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void ContinueGame()
    {
        gameObject.SetActive(false);
        //TODO Continue gameplay
    }

    private void BackToMenu()
    {
        //TODO Save game
        SceneManager.LoadScene("Menu");
    }
}