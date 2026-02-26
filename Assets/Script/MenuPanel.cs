using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class MenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button _continue;

    [SerializeField]
    private Button _menu;
    
    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        _continue.onClick.AddListener(ContinueGame);
        _menu.onClick.AddListener(BackToMenu);
    }
    
    public void Open()
    {
        gameObject.SetActive(true);
        IsOpen = true;
    }

    private void ContinueGame()
    {
        gameObject.SetActive(false);
        IsOpen = false;
        //TODO Continue gameplay
    }

    private void BackToMenu()
    {
        IsOpen = false;
        //TODO Save game
        SceneManager.LoadScene("Menu");
    }

    private void OnDisable()
    {
        IsOpen = false;
    }
}
