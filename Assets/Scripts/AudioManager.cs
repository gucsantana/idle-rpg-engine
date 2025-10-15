using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	/// audio player script shamelessly ripped from project Kurogi
	
	/// --- Singleton definitions ---
	private static AudioManager _instance;
	public static AudioManager Instance { get { return _instance; } }
	
	// variables for the audio player
	public List<AudioClip> audioClips;
	public AudioSource _bgmSource;
	public AudioSource _bgmSourceIntro;
	public AudioSource _sfxSource;
	
	private float _defaultBgmVolume = 1f;
	
	// constants
	private const float _FADEDURATION = 2.5f;			// how long the fade in/out process takes
	private const float _FADESTEP = 0.05f;				// how much time in between each step of fading a song in/out
    
	// ------------------------------------------------------------------------------ //
	
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	
	// plays a parameterized sound
	public void PlaySound (String _sound)
	{
		_sfxSource.PlayOneShot(audioClips.Where(x => x.name == _sound).FirstOrDefault());
	}
	
	// plays a parameterized music, with optional fade in
	public IEnumerator PlayBgm (String _soundName, bool _fadeIn = false)
	{
		float _fadePerStep = _defaultBgmVolume / (_FADEDURATION / _FADESTEP);
		if(_fadeIn)
			_bgmSource.volume = 0f;
		
		_bgmSource.clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
		
		if(_bgmSource.clip != null)
		{
			_bgmSource.Play();
			if(_fadeIn)
				StartCoroutine(FadeInBgm(_fadePerStep));
		}
		
		yield return null;
	}
	
	// plays a parameterized music separated in Intro and Loop parts, with optional fade in
	public IEnumerator PlayBgmWithIntro (String _soundName, bool _fadeIn = false)
	{		
		float _fadePerStep = _defaultBgmVolume / (_FADEDURATION / _FADESTEP);
		if(_fadeIn)
			_bgmSourceIntro.volume = 0f;
		
		_bgmSourceIntro.clip = audioClips.Where(x => x.name == _soundName + "_intro").FirstOrDefault();
		_bgmSource.clip = audioClips.Where(x => x.name == _soundName + "_loop").FirstOrDefault();
		
		if(_bgmSourceIntro.clip != null)
		{
			_bgmSourceIntro.Play();
			if(_fadeIn)
				StartCoroutine(FadeInBgmIntro(_fadePerStep));
			
			double _nextEventTime = AudioSettings.dspTime + _bgmSourceIntro.clip.length;
			double _currEventTime = AudioSettings.dspTime;
			while (_currEventTime + 1.0f < _nextEventTime)
			{
				yield return new WaitForSeconds(0.05f);
				_currEventTime = AudioSettings.dspTime;
			}
			
			_bgmSource.PlayScheduled(_nextEventTime);
		}
		yield return null;
	}
	
	/// fades in the BGM, with a specified fade per step
	public IEnumerator FadeInBgm ( float _fadePerStep)
	{
		while(_bgmSource.volume < _defaultBgmVolume)
		{
			yield return new WaitForSeconds(_FADESTEP);
			_bgmSource.volume += _fadePerStep;
		}
		yield return null;
	}
	
	/// fades in the BGM intro, with a specified fade per step
	public IEnumerator FadeInBgmIntro ( float _fadePerStep)
	{
		while(_bgmSourceIntro.volume < _defaultBgmVolume)
		{
			yield return new WaitForSeconds(_FADESTEP);
			_bgmSourceIntro.volume += _fadePerStep;
		}
	}
	
	// stops a named sound
	public void StopSound(String sound)
	{
		foreach (AudioClip clip in audioClips)
		{
			if (clip.name == sound)
			{
				_sfxSource.Stop();
			}
		}
	}
	
	// stops a named bgm
	public IEnumerator StopMusic(String sound, bool _fadeOut = false)
	{
		float _fadePerStep = _defaultBgmVolume / (_FADEDURATION / _FADESTEP);
		
		if(_fadeOut)
			while(_bgmSource.volume >=  0.05f)
			{
				yield return new WaitForSeconds(_FADESTEP);
				_bgmSource.volume -= _fadePerStep;
			}
		
		_bgmSource.Stop();
	}

	// queue up a sound to be played after a delay
	public void QueueSound (String _soundName, float _timer)
	{
		_sfxSource.clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
		_sfxSource.PlayDelayed(_timer);
	}

	// adds a new clip to the available clips
	public void AddNewClip(AudioClip _clip)
	{
		audioClips.Add(_clip);
	}
	
	// sets the volume of the sfx player
	public void SetSfxVolume (float _volume)
	{
		_sfxSource.volume = _volume;
	}
	
	// sets the volume of the bgm player
	public void SetBgmVolume (float _volume)
	{
		_defaultBgmVolume = _bgmSource.volume = _volume;
	}
	
	// sets the Loop condition of the sfx player
	public void SetLoopSfx (bool _loop)
	{
		_sfxSource.loop = _loop;
	}
	
	// sets the Loop condition of the bgm player
	public void SetLoopBgm (bool _loop)
	{
		_bgmSource.loop = _loop;
	}
}
