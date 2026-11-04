using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaughtFishPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fishName;
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _fishRarity;
    [SerializeField] private TextMeshProUGUI _fishWeight;
    private void Awake()
    {
        GetComponent<CanvasGroup>().alpha = 0;
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
    }
}
