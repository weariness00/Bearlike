using System;
using System.Collections.Generic;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance;

	public AudioMixer audioMixer;
	
	public AudioSource[] audioSources;
	Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();
	
	string defualt_Path = "Sound/";
	private const float minDecibel = -80f; // 믹서의 최소 데시벨 값
	private const float maxDecibel = 20f;  // 믹서의 최대 데시벨 값
	
	public enum SoundType
	{
		BGM,
		Effect,
	
		MaxValue
	}
	
	public struct AudioSourceSetting
	{
		public int Pitch;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnApplicationQuit()
	{
		if (audioMixer == null) return;
		
		foreach (var audioSource in audioSources)
		{
			if(audioSource.outputAudioMixerGroup == null) continue;
			var audioMixerGroupName= audioSource.outputAudioMixerGroup.name;
			
			audioMixer.GetFloat(audioMixerGroupName, out var volume);
			PlayerPrefs.SetFloat($"SoundManager_{audioMixerGroupName}", volume);
		}
	}
	
	public void AudioSourcesGenerate()
	{
		var soundTypeString = Enum.GetNames(typeof(SoundType));

		var childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}

		audioSources = new AudioSource[(int)SoundType.MaxValue];
		for(int i = 0; i < (int)SoundType.MaxValue; i++)
		{
			GameObject obj = new GameObject { name = ((SoundType)i).ToString() };
			audioSources[i] = obj.AddComponent<AudioSource>();
			
			var keyName = $"SoundManager_{soundTypeString[i]}";
			if (PlayerPrefs.HasKey(keyName))
			{
				var volume = PlayerPrefs.GetFloat(keyName);
				audioMixer.SetFloat(soundTypeString[i], volume);
			}
			audioSources[i].outputAudioMixerGroup = audioMixer.FindMatchingGroups(soundTypeString[i])[0];
			obj.transform.parent = transform;
		}
	}

	public static void Clear()
	{
		foreach(AudioSource source in Instance.audioSources)
		{
			if (source == null) continue;
			source.clip = null;
			source.Stop();
		}
		Instance.clipDictionary.Clear();
	}

	public static void Play(string path, SoundType type = SoundType.Effect, float pitch = 1.0f)
	{
		if(!path.Contains("/"))
			path = Instance.defualt_Path + path;

		AudioSource source;
		AudioClip clip = Instance.GetOrAddAudioClip(path);
        if (clip == null)
        {
	        DebugManager.LogWarning($"AudioClip Missing : {path}");
            return;
        }
		Play(clip, type, pitch);
    }

    public static void Play(AudioClip clip, SoundType type = SoundType.Effect, float pitch = 1.0f)
	{
		if(clip == null)
		{
			DebugManager.LogWarning("사운드 Clip이 존재하지 않습니다.");
			return;
		}

        AudioSource source;
        if (type == SoundType.BGM)
        {
            source = Instance.audioSources[(int)SoundType.BGM];
            if (source.isPlaying)
                source.Stop();

            source.clip = clip;
            source.pitch = pitch;
            source.loop = true;

            source.Play();
        }
        else
        {
            source = Instance.audioSources[(int)SoundType.Effect];
            source.pitch = pitch;
            source.PlayOneShot(clip);
        }
    }

    public static void Play(AudioSource audioSource)
    {
	    if(audioSource == null)
	    {
		    DebugManager.LogWarning("Sound Source가 존재하지 않습니다.");
		    return;
	    }
	    
	    audioSource.Stop();
	    audioSource.PlayOneShot(audioSource.clip);
    }

    public static float GetVolume(SoundType soundType)
    {
	    if (Instance.audioMixer == null) return 0f;
	    Instance.audioMixer.GetFloat(Instance.audioSources[(int)soundType].outputAudioMixerGroup.name, out var volume);
	    return volume;
    }

    public static void SetVolume(SoundType soundType, float value)
    {
	    if (Instance.audioMixer == null) return;
	    Instance.audioMixer.SetFloat(Instance.audioSources[(int)soundType].outputAudioMixerGroup.name, value);
    }
    
    // 0~100 범위의 선형 값을 데시벨 값으로 변환하는 함수
    public static float LinearToDecibel(float linear)
    {
	    float dB = Mathf.Lerp(minDecibel, maxDecibel, linear / 100f);
	    return dB;
    }

    // 데시벨 값을 0~100 범위의 선형 값으로 변환하는 함수
    public static float DecibelToLinear(float dB)
    {
	    float linear = Mathf.InverseLerp(minDecibel, maxDecibel, dB) * 100f;
	    return linear;
    }
    
	AudioClip GetOrAddAudioClip(string path)
	{
		AudioClip clip = null;
		if(clipDictionary.TryGetValue(path, out clip) == false)
		{
			clip = Resources.Load<AudioClip>(path);
			clipDictionary.Add(path, clip);
		}
		return clip;
	}
}

