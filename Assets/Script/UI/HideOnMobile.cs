using UnityEngine;

public sealed class HideOnMobile : MonoBehaviour
{
    private void Awake()
    {
        if (Application.isMobilePlatform)
            gameObject.SetActive(false);
    }
}
