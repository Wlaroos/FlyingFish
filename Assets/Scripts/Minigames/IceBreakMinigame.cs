using UnityEngine;
using UnityEngine.Tilemaps;

public class IceBreakMinigame : BaseMinigame
{
    private Tilemap _tilemap;
    private IceRefreeze _refreezeManager;
    private Vector3Int _currentHoveredCell;
    private TileBase _iceHoleTile;

    // This might be changed to an event or an action instead of setting it up like this
    public void Init(Tilemap tm, IceRefreeze rm, Vector3Int chc, TileBase iht)
    {
        _tilemap = tm;
        _refreezeManager = rm;
        _currentHoveredCell = chc;
        _iceHoleTile = iht;
    }
    protected override void FailMinigame()
    {
        base.FailMinigame();
    }
    protected override void CompleteMinigame()
    {
        base.CompleteMinigame();

        _tilemap.SetTile(_currentHoveredCell, _iceHoleTile);
        _refreezeManager.StartRefreezingTile(_currentHoveredCell);
    }
}
