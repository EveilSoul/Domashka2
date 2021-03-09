using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Selectable : MonoBehaviour
{
    public Color HighlightedColor;
    private Color normalColor;
    private Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        normalColor = image.color;
    }

    public void OnPointerEnter()
    {
        if (image.color == normalColor)
            image.color = HighlightedColor;
        Explorer.IsPointerOutFile = false;
    }

    public void OnPointerExit()
    {
        if (image.color == HighlightedColor)
            image.color = normalColor;
        Explorer.IsPointerOutFile = true;
    }

    public void OnPointerClick()
    {
        Explorer.OnFileSelected(gameObject);
    }
}
