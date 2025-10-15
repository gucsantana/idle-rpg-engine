using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

	// variables for the audio player
	public List<AudioClip> audioClips;
	public AudioSource[] _bgmSource;
	public AudioSource _bgmSourceIntro;
	public AudioSource _sfxSource;
	
	private float _defaultBgmVolume = 0.5f;
	private bool _usingFirstAudioSource = true;
	
	// constants
	private const float _FADEDURATION = 2.5f;			// how long the fade in/out process takes
	private const float _FADESTEP = 0.05f;				// how much time in between each step of fading a song in/out
    
	// ------------------------------------------------------------------------------ //
    
	void Update(){
		
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
			_bgmSource[0].volume = 0f;
		
		_bgmSource[0].clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
		
		if(_bgmSource[0].clip != null)
		{
			_bgmSource[0].Play();
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
		_bgmSource[0].clip = audioClips.Where(x => x.name == _soundName + "_loop").FirstOrDefault();
		
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
			
			_bgmSource[0].PlayScheduled(_nextEventTime);
		}
		yield return null;
	}
	
	/// fades in the BGM, with a specified fade per step
	public IEnumerator FadeInBgm ( float _fadePerStep, int _bgmSourceNum = 0)
	{
		while(_bgmSource[_bgmSourceNum].volume < _defaultBgmVolume)
		{
			yield return new WaitForSeconds(_FADESTEP);
			_bgmSource[_bgmSourceNum].volume += _fadePerStep;
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

	/// crossfades in the BGM, with a specified fade per step, checking appropriate sources
	public IEnumerator CrossfadeInBgm (String _soundName)
	{
		float _fadePerStep = _defaultBgmVolume / (_FADEDURATION / _FADESTEP);

		if(_usingFirstAudioSource)
		{
			// we don't want to restart or otherwise switch to the same song
			if(_bgmSource[0].clip?.name == _soundName)
				yield break;
			_usingFirstAudioSource = false;
			_bgmSource[1].volume = 0f;
			_bgmSource[1].clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
			if(_bgmSource[1].clip != null)
			{
				_bgmSource[1].Play();
				StartCoroutine(FadeInBgm(_fadePerStep, _bgmSourceNum : 1));
				StartCoroutine(StopMusic(_fadeOut : true, _bgmSourceNum : 0));
			}
		}
		else
		{
			// we don't want to restart or otherwise switch to the same song
			if(_bgmSource[1].clip?.name == _soundName)
				yield break;
			_usingFirstAudioSource = true;
			_bgmSource[0].volume = 0f;
			_bgmSource[0].clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
			if(_bgmSource[0].clip != null)
			{
				_bgmSource[0].Play();
				StartCoroutine(FadeInBgm(_fadePerStep, _bgmSourceNum : 0));
				StartCoroutine(StopMusic(_fadeOut : true, _bgmSourceNum : 1));
			}
		}
		
		yield return null;
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
	public IEnumerator StopMusic(bool _fadeOut = false, int _bgmSourceNum = 0)
	{
		float _fadePerStep = _defaultBgmVolume / (_FADEDURATION / _FADESTEP);
		
		if(_fadeOut)
			while(_bgmSource[_bgmSourceNum].volume >=  0.05f)
			{
				yield return new WaitForSeconds(_FADESTEP);
				_bgmSource[_bgmSourceNum].volume -= _fadePerStep;
			}
		
		_bgmSource[_bgmSourceNum].Stop();
	}

	// queue up a sound to be played after a delay
	public void QueueSound (String _soundName, float _timer)
	{
		_sfxSource.clip = audioClips.Where(x => x.name == _soundName).FirstOrDefault();
		_sfxSource.PlayDelayed(_timer);
	}

	// adds a new clip to the available clips if it's not already there
	public void AddNewClip(AudioClip _clip)
	{
		if(!audioClips.Contains(_clip))
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
		_defaultBgmVolume = _bgmSource[0].volume = _volume;
	}
	
	// sets the Loop condition of the sfx player
	public void SetLoopSfx (bool _loop)
	{
		_sfxSource.loop = _loop;
	}
	
	// sets the Loop condition of the bgm player
	public void SetLoopBgm (bool _loop)
	{
		_bgmSource[0].loop = _loop;
	}
}
