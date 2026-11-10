using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IceRefreeze : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Tile[] _freezePhaseTiles; // Assign your stage tiles here, ending with Solid Ice
    [SerializeField] private float _refreezeTime = 3f;

    private void Awake()
    {
        if (_tilemap == null)
        {
            _tilemap = GetComponent<Tilemap>();
        }
    }

    public void StartRefreezingTile(Vector3Int cellPosition)
    {
        StartCoroutine(RefreezeRoutine(cellPosition));
    }

    private IEnumerator RefreezeRoutine(Vector3Int cellPosition)
    {
        float timeCount = 0;
        int totalPhases = _freezePhaseTiles.Length;

        if (totalPhases == 0) yield break;

        while (timeCount < _refreezeTime)
        {
            yield return null; // Wait for the next frame
            timeCount += Time.deltaTime;

            float progress = Mathf.Clamp01(timeCount / _refreezeTime);

            // Determine which phase tile to show based on elapsed time
            int phaseIndex = Mathf.Clamp(Mathf.FloorToInt(progress * totalPhases), 0, totalPhases - 1);

            // Update the tile at this specific grid coordinate
            _tilemap.SetTile(cellPosition, _freezePhaseTiles[phaseIndex]);
        }

        // Ensure the final fully-frozen tile is set at 100%
        _tilemap.SetTile(cellPosition, _freezePhaseTiles[totalPhases - 1]);
    }
}