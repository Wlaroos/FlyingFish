using System.Collections;
using UnityEngine;

public class FishingBobber : MonoBehaviour
{
    [Header("Arc Motion Settings")]
    [SerializeField] private float _arcHeight = 1.5f;

    [Header("Float Oscillation Settings")]
    [SerializeField] private float _floatFrequency = 2f;
    [SerializeField] private float _floatAmplitude = 0.08f;

    [Header("Line Visual Settings")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private int _lineSegmentCount = 12;
    [SerializeField] private float _lineSagAmount = 0.35f;

    private Transform _rodTip;
    private Vector3 _waterLandingPos;
    private bool _isFloating = false;
    private bool _isLineActive = false;
    private float _bobberTimer = 0f;

    public void InitializeLine(Transform rodTip)
    {
        _rodTip = rodTip;
        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = _lineSegmentCount;
            _lineRenderer.enabled = true;
            _isLineActive = true;
        }
    }

    // Launches the bobber along a parabolic path to the designated water coordinate.
    public IEnumerator CastToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        _waterLandingPos = targetPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Linear interpolation + parabolic arc on the Y axis
            Vector3 currentPos = Vector3.Lerp(startPos, targetPosition, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;
            currentPos.y += heightOffset;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition;
        _isFloating = true;
    }

    private void Update()
    {
        if (_isFloating)
        {
            // Idle floating motion using sine wave offset
            _bobberTimer += Time.deltaTime * _floatFrequency;
            float yOffset = Mathf.Sin(_bobberTimer) * _floatAmplitude;
            transform.position = _waterLandingPos + new Vector3(0f, yOffset, 0f);
        }

        UpdateFishingLine();
    }

    // Renders a catenary curve between the rod tip and this bobber.
    private void UpdateFishingLine()
    {
        if (!_isLineActive || _lineRenderer == null || _rodTip == null) return;

        Vector3 start = _rodTip.position;
        Vector3 end = transform.position;

        for (int i = 0; i < _lineSegmentCount; i++)
        {
            float t = (float)i / (_lineSegmentCount - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);

            // Add a parabolic sag curve for slack/rope physics
            float sag = Mathf.Sin(t * Mathf.PI) * _lineSagAmount;
            pos.y -= sag;

            _lineRenderer.SetPosition(i, pos);
        }
    }

    // Simulates a quick tug down into the water when a fish bites.
    public IEnumerator TriggerBiteAnimation()
    {
        Vector3 initialPos = transform.position;
        Vector3 dippedPos = initialPos + Vector3.down * 0.25f;

        // Quick dip down
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPos, dippedPos, elapsed / 0.1f);
            yield return null;
        }

        // Return back up
        elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(dippedPos, _waterLandingPos, elapsed / 0.15f);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }
}