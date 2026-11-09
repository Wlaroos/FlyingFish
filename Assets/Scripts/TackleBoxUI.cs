using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TackleBoxUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _tackleBoxPanel;
    private CanvasGroup _tackleBoxPanelCanvasGroup;
    [SerializeField] private Transform _rodGridContainer;
    [SerializeField] private Transform _baitGridContainer;
    [SerializeField] private Transform _lureGridContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPrefab;

    [Header("Inventory Data")]
    [SerializeField] private List<RodData> _availableRods = new List<RodData>();
    [SerializeField] private List<BaitData> _availableBaits = new List<BaitData>();
    [SerializeField] private List<LureData> _availableLures = new List<LureData>();

    private void Awake()
    {
        _tackleBoxPanelCanvasGroup = _tackleBoxPanel.GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PlayerInputs.OnTackleBoxPressed += ToggleTackleBox;
        TackleBoxManager.Instance.OnGearChanged += RefreshUI;
    }

    private void OnDisable()
    {
        PlayerInputs.OnTackleBoxPressed -= ToggleTackleBox;
        if (TackleBoxManager.Instance != null)
            TackleBoxManager.Instance.OnGearChanged -= RefreshUI;
    }

    private void Start()
    {
        PopulateTackleBox();
        
        _tackleBoxPanelCanvasGroup.alpha = 0f;
        _tackleBoxPanelCanvasGroup.interactable = false;
    }

    public void ToggleTackleBox()
    {
        bool isOpening = _tackleBoxPanelCanvasGroup.alpha == 0f;
        _tackleBoxPanelCanvasGroup.alpha = isOpening ? 1f : 0f;
        _tackleBoxPanelCanvasGroup.interactable = isOpening;

        if (isOpening)
        {
            RefreshUI();
        }
    }

    private void PopulateTackleBox()
    {
        ClearContainer(_rodGridContainer);
        ClearContainer(_baitGridContainer);
        ClearContainer(_lureGridContainer);

        // Populate Rods
        foreach (var rod in _availableRods)
        {
            var slot = Instantiate(_slotPrefab, _rodGridContainer).GetComponent<TackleBoxSlotUI>();
            slot.Setup(rod, IsEquipped(rod), () => {
                TackleBoxManager.Instance.EquipRod(rod);
                RefreshUI();
            });
        }

        // Populate Baits
        foreach (var bait in _availableBaits)
        {
            var slot = Instantiate(_slotPrefab, _baitGridContainer).GetComponent<TackleBoxSlotUI>();
            slot.Setup(bait, IsEquipped(bait), () => {
                TackleBoxManager.Instance.EquipBait(bait);
                RefreshUI();
            });
        }

        // Populate Lures
        foreach (var lure in _availableLures)
        {
            var slot = Instantiate(_slotPrefab, _lureGridContainer).GetComponent<TackleBoxSlotUI>();
            slot.Setup(lure, IsEquipped(lure), () => {
                TackleBoxManager.Instance.EquipLure(lure);
                RefreshUI();
            });
        }
    }

    public void RefreshUI()
    {
        // Re-evaluate equipped highlights on all child slots
        foreach (Transform child in _rodGridContainer)
            child.GetComponent<TackleBoxSlotUI>()?.UpdateEquippedState(IsEquipped);

        foreach (Transform child in _baitGridContainer)
            child.GetComponent<TackleBoxSlotUI>()?.UpdateEquippedState(IsEquipped);

        foreach (Transform child in _lureGridContainer)
            child.GetComponent<TackleBoxSlotUI>()?.UpdateEquippedState(IsEquipped);
    }

    private bool IsEquipped(FishingGearData gear)
    {
        if (TackleBoxManager.Instance == null || gear == null) return false;

        return TackleBoxManager.Instance.EquippedRod == gear ||
               TackleBoxManager.Instance.EquippedBait == gear ||
               TackleBoxManager.Instance.EquippedLure == gear;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}