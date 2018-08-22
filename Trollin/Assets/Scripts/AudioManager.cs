using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : Singleton<AudioManager> {

    public Sound[] sounds;

    public void Start()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.spatialBlend = 1;
            s.source.rolloffMode = AudioRolloffMode.Logarithmic;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.maxDistance = 30f;
        }
    }

    public void Play(string name, Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        AudioSource.PlayClipAtPoint(s.clip, position);
    }

}
