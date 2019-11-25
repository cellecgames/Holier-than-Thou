﻿using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
	[SerializeField] private Sound[] sounds;
	[SerializeField] private readonly string SFXSlider = "SoundsSliderBar";
	[SerializeField] private readonly string MusicSlider = "MusicSliderBar";

	[Range(0.5f, 1.0f)]
	[SerializeField] private float pitchRangeMin;
	[Range(1.0f, 1.5f)]
	[SerializeField] private float pitchRangeMax;

	[SerializeField] private int maxVolume = 100;

    private Sound temp;


	// Start is called before the first frame update
	void Awake()
    {
		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
			s.source.spatialBlend = s.spatialBlend;
		}

    }

    public void Play(string name, bool pitchModulate = true)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if(s != null)
		{
			float sliderVolume = 0;

			switch (s.type)
			{
				case Sound.SoundType.SFX:
					s.source.pitch = UnityEngine.Random.Range(pitchRangeMin, pitchRangeMax);
					sliderVolume = (float)PlayerPrefs.GetInt(SFXSlider, 100)/ (float)maxVolume;
					break;
				case Sound.SoundType.Music:
					sliderVolume = (float)PlayerPrefs.GetInt(MusicSlider, 100) / (float)maxVolume;
                    temp = s;
					break;
				default:
					sliderVolume = 0;
					break;
			}

			s.source.volume = s.volume * sliderVolume;
			s.source.Play();
		}
	}
    

    public void Stop()
    {
        if (temp == null)
        {
            Debug.Log("No music playing");
        }
        else
            temp.source.Stop();


    }
}
