using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    internal void Setup(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }
}