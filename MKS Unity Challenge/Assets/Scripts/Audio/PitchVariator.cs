using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchVariator : MonoBehaviour
{
    AudioSource _audioSource;
    [SerializeField] Vector2 _pitchVariation;

    float _originalPitch;

    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _originalPitch = _audioSource.pitch;

        _audioSource.pitch = _originalPitch * Random.Range(_pitchVariation.x, _pitchVariation.y);
    }
}
