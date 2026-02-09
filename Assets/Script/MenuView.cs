using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] private Button _startGame;
    private SaveManager _saveManager;
    [SerializeField] private Button _continue;

    [SerializeField]
    private Button _close;
    
    [SerializeField] private Button _character1;
    [SerializeField] private Button _character2;
    [SerializeField] private GameObject _chooseCharacterPanel;
    
    private void Awake()
    {
        _saveManager = new SaveManager();
        _startGame.onClick.AddListener(ChooseCharacterPanel);
        if (SaveManager.GetGameSaves() == null)
        {
            _continue.gameObject.SetActive(false);
        }
        _continue.onClick.AddListener(ContinueGame);
        _close.onClick.AddListener(CloseChooseCharacter);
        _character1.onClick.AddListener(() => ChooseCharacter(0));
        _character2.onClick.AddListener(() => ChooseCharacter(1));
    }

    private void CloseChooseCharacter()
    {
        _chooseCharacterPanel.SetActive(false);
    }

    private void ChooseCharacterPanel()
    {
        _chooseCharacterPanel.SetActive(true);
    }

    private void ChooseCharacter(int character)
    {
        _saveManager.CreateGameSaves(character);
        SceneManager.LoadScene("Game");
    }
    
    private void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
 
}