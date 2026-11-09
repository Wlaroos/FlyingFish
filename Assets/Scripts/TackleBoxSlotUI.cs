using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TackleBoxSlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _equippedBorder;
    [SerializeField] private Button _slotButton;

    private FishingGearData _gearData;

    public void Setup(FishingGearData gear, bool isEquipped, Action onClickAction)
    {
        _gearData = gear;

        if (_nameText != null) _nameText.text = gear.gearName;
        if (_iconImage != null && gear.gearIcon != null) _iconImage.sprite = gear.gearIcon;

        SetEquippedVisual(isEquipped);

        _slotButton.onClick.RemoveAllListeners();
        _slotButton.onClick.AddListener(() => onClickAction?.Invoke());
    }

    public void UpdateEquippedState(Func<FishingGearData, bool> isEquippedCheck)
    {
        if (_gearData != null)
        {
            SetEquippedVisual(isEquippedCheck(_gearData));
        }
    }

    private void SetEquippedVisual(bool isEquipped)
    {
        if (_equippedBorder != null)
        {
            _equippedBorder.color = isEquipped ? new Color32(113, 255, 129, 255) : Color.clear;
        }
    }
}