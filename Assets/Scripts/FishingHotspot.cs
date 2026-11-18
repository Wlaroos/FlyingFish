using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class FishingHotspot : MonoBehaviour
{
    [Header("Hotspot Settings")]
    [Tooltip("Multiplier for wait time. 0.5 means it takes half the time to get a bite.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _waitTimeMultiplier = 0.5f;

    [Tooltip("Multiplier applied to rare fish drop chances while in this spot.")]
    [SerializeField] private float _rareDropMultiplier = 1.5f;
    [SerializeField] private Sprite[] _initialSprites;
    [SerializeField] private Sprite[] _loopingSprites;
    [SerializeField] private float _timeBetweenFrames = 0.2f;

    public float WaitTimeMultiplier => _waitTimeMultiplier;
    public float RareDropMultiplier => _rareDropMultiplier;

    private SpriteRenderer _sr;
    private int _spriteIndex = 0;
    private bool _finishedInitAnim = false;
    private int _maxReps = 50;

    private void Reset()
    {
        // Automatically make sure the collider is set as a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        while (!_finishedInitAnim && _spriteIndex < _initialSprites.Length - 1)
        {
            yield return new WaitForSeconds(_timeBetweenFrames);
            _spriteIndex++;
            _sr.sprite = _initialSprites[_spriteIndex];
        }

        _spriteIndex = 0;

        int _repIndex = 0;

        while(_repIndex < _maxReps)
        {
            yield return new WaitForSeconds(_timeBetweenFrames);

            if (_spriteIndex == _loopingSprites.Length - 1)
            {
                _spriteIndex = 0;
                _repIndex++;
            }
            else
            {
                _spriteIndex++;
            }

            _sr.sprite = _loopingSprites[_spriteIndex];
        }
    }
}