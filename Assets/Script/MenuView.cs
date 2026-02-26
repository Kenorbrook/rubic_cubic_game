using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] private Button _startGame;
    [SerializeField] private Button _continue;
    [SerializeField] private Button _close;
    [SerializeField] private Button _character1;
    [SerializeField] private Button _character2;
    [SerializeField] private GameObject _chooseCharacterPanel;

    private IGameSaveService _saveService;

    private void Awake()
    {
        _saveService = new SaveManager();

        _startGame.onClick.AddListener(ChooseCharacterPanel);
        _close.onClick.AddListener(CloseChooseCharacter);
        _character1.onClick.AddListener(() => ChooseCharacter(0));
        _character2.onClick.AddListener(() => ChooseCharacter(1));

        var saves = _saveService.Load();
        _continue.gameObject.SetActive(saves != null);
        _continue.onClick.AddListener(ContinueGame);
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
        _saveService.Create(character);
        SceneManager.LoadScene("Game");
    }

    private void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
}
