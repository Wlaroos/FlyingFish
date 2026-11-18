using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaughtFishPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fishName;
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _fishRarity;
    [SerializeField] private TextMeshProUGUI _fishWeight;

    private CanvasGroup _cg;
    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();

        HidePopup();
    }

    public void InitCatch(string name, Sprite sprite, Rarity rarity, Vector2 weight)
    {
        _fishName.text = ($"You caught a {name}!");

        _fishImage.sprite = sprite;
        _fishImage.preserveAspect = true;

        _fishRarity.text = ($"Rarity - {rarity}");

        float randomWeight = Random.Range(weight.x, weight.y);
        float roundedWeight = (float)System.Math.Round(randomWeight, 2);
        _fishWeight.text = ($"Weight - {roundedWeight}lbs!");

        ShowPopup();
    }

    public void ShowPopup()
    {
        _cg.alpha = 1;
        _cg.blocksRaycasts = true;
        _cg.interactable = true;
    }
    public void HidePopup()
    {
        _cg.alpha = 0;
        _cg.blocksRaycasts = false;
        _cg.interactable = false;
    }
}
