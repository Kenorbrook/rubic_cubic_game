using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _cubeModeButton;
    [SerializeField] private Button _invertVerticalButton;
    [SerializeField] private TMP_Text _cubeModeLabel;
    [SerializeField] private TMP_Text _invertVerticalLabel;
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;

    private void Awake()
    {
        _openButton?.onClick.AddListener(Open);
        _backButton?.onClick.AddListener(Close);
        _cubeModeButton?.onClick.AddListener(ToggleCubeMode);
        _invertVerticalButton?.onClick.AddListener(ToggleVerticalInversion);
        _settingsPanel?.SetActive(false);
        RefreshLabels();
    }

    private void OnDestroy()
    {
        _openButton?.onClick.RemoveListener(Open);
        _backButton?.onClick.RemoveListener(Close);
        _cubeModeButton?.onClick.RemoveListener(ToggleCubeMode);
        _invertVerticalButton?.onClick.RemoveListener(ToggleVerticalInversion);
    }

    private void Open()
    {
        _mainPanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
        RefreshLabels();
    }

    private void Close()
    {
        _settingsPanel?.SetActive(false);
        _mainPanel?.SetActive(true);
    }

    private void ToggleCubeMode()
    {
        CubeViewPreferences.Use3D = !CubeViewPreferences.Use3D;
        RefreshLabels();
    }

    private void ToggleVerticalInversion()
    {
        CubeViewPreferences.InvertVertical = !CubeViewPreferences.InvertVertical;
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (_cubeModeLabel != null)
            _cubeModeLabel.text = CubeViewPreferences.Use3D ? "Куб: 3D" : "Куб: 2D";
        if (_invertVerticalLabel != null)
            _invertVerticalLabel.text = CubeViewPreferences.InvertVertical
                ? "Инверсия по вертикали: вкл"
                : "Инверсия по вертикали: выкл";
    }
}
