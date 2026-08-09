using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CubeModeToggleButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private RubikCubeView _cube;

    private void Awake()
    {
        if (_button != null)
            _button.onClick.AddListener(ToggleMode);
        RefreshLabel();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(ToggleMode);
    }

    private void ToggleMode()
    {
        if (_cube != null)
            _cube.Set3DMode(!_cube.Is3DMode);
        else
            CubeViewPreferences.Use3D = !CubeViewPreferences.Use3D;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (_label != null)
        {
            var use3D = _cube != null ? _cube.Is3DMode : CubeViewPreferences.Use3D;
            _label.text = use3D ? "Куб: 3D" : "Куб: 2D";
        }
    }
}
