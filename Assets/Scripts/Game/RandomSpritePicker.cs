using UnityEngine;
using UnityEngine.UI;

public class RandomSpritePicker : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null && sprites.Length > 0)
            image.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
