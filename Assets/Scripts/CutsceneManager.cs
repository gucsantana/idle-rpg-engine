using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    /// --- Singleton definitions ---
	private static CutsceneManager _instance;
	public static CutsceneManager Instance { get { return _instance; } }
    
    /// --- Constants ---
    // timing constants
	const float _MINIMUMWAITTIME = 0.2f;	//  the minimum time interval before the system starts accepting input to skip events
	const float _TIMEPERLETTER = 0.02f;		//  the time interval between each letter written in the dialog box
	const int _TIMEPERTEXTSOUND = 2;		// time, in letters, between each 'text writing sound' played
	const float _SKIPHOLDTIME = 1.5f;

	// visibility constants
	const float _SKIPTEXTFADERATE = 2f;
	
    // sound file constants
    const string _CHARACTERTEXTSOUND = "menu fx 2 (Bumblefly)";

	// volume constants
	const float _TEXTSOUNDVOLUME = 0.2f;

    /// --- Cutscene Contents ---
    public List<string> _cutsceneText = new List<string>();

    /// --- References ---
	private Text _dialogText;
	private GameObject _textBox;
	private GameObject _skipTextBox;
	public TMP_Text _tmpComponent;
	private AudioPlayer _audioPlayer;
	private GameManagerScript _gameManager { get { return GameManagerScript.Instance; } }
	private HUDController _hudControl { get { return HUDController.Instance; } }
	
	public GameObject _screenCanvas;
    public GameObject _backgroundImg;
	public GameObject _currentCutsceneImg;
	public GameObject _previousCutsceneImg;
	public GameObject _detailImg;

    /// --- Control variables ---
    public bool _inCutscene = false;
	private bool _writingInProgress = true;
	private bool hasTextChanged;
	private string _nextScene = "";

	// private float _curSkipHeldTime = 0f;
	// private string _skipFadeDirection = "out";

    // ------------------------------------------------------- //

    private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
		
		SetReferences();
		
		DontDestroyOnLoad(this);
		
		// if(SceneManager.GetActiveScene().name == "MainMenu")
		// 	this.gameObject.SetActive(false);
	}

    void Update()
	{
		if(_inCutscene && Input.GetMouseButtonDown(0) && _writingInProgress)
		{
			_writingInProgress = false;
		}
		else if(_inCutscene && Input.GetMouseButtonDown(0) && !_writingInProgress)
		{
			EndCutscene();
		}

		// fade the skip text in or out
		// if(_skipFadeDirection == "in")
		// 	_skipTextBox.GetComponent<TMP_Text>().alpha = Mathf.Min(_skipTextBox.GetComponent<TMP_Text>().alpha + (_SKIPTEXTFADERATE * Time.deltaTime), 1f);
		// else if(_skipFadeDirection == "out")
		// 	_skipTextBox.GetComponent<TMP_Text>().alpha = Mathf.Max(_skipTextBox.GetComponent<TMP_Text>().alpha - (_SKIPTEXTFADERATE * Time.deltaTime), 0f);
	}

    private void SetReferences() {
		_audioPlayer = GetComponent<AudioPlayer>();
    }

    // ------------------------------------------------------- //
    /// plays the cutscene with the passed id
    public void PlayCutscene(int cutsceneId, string next="nextarea"){
        _inCutscene = true;
		_gameManager.SetInCutscene(true);
		SceneManager.LoadScene("CutsceneArea");
		_nextScene = next;

		_hudControl.gameObject.SetActive(false);
        _backgroundImg.SetActive(true);

        StartCoroutine(WriteDialogByLetter(_text:_cutsceneText[cutsceneId]));
    }

    /// writes a dialogue box line by line, playing the appropriate sounds
	IEnumerator WriteDialogByLetter(string  _text, float _textSpeed = _TIMEPERLETTER)
    {
		int totalVisibleCharacters = _text.Length; 		// Get # of Visible Character in text object
		int visibleCount = 0;							// current number of visible letters
		int _currSound = 0;								// current counter for deciding when to play a sound
		
		_tmpComponent.gameObject.SetActive(true);
		_tmpComponent.ForceMeshUpdate();
		_audioPlayer.SetSfxVolume(_TEXTSOUNDVOLUME);

		// initializes variables, and sets the base transparency to 1 in case it has been set to 0 before
		TMP_TextInfo textInfo = _tmpComponent.textInfo;
		Color _color = _tmpComponent.color;
		_color.a = 1;
		_tmpComponent.color = _color;
		
		_tmpComponent.maxVisibleCharacters = 0;
		_tmpComponent.text = _text;
		
		_writingInProgress = true;

		// while there are still invisible letters...
		while (visibleCount <= totalVisibleCharacters)
		{
			yield return new WaitForSeconds(_textSpeed);
			
			// if the player presses the interact button while text is rendering, skips rendering to the end
			if(!_writingInProgress)
				visibleCount = totalVisibleCharacters;

			_tmpComponent.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?

			visibleCount += 1;
			
			// calculates whether to play the sound in this letter
			_currSound++;
			if(_currSound == _TIMEPERTEXTSOUND)
			{
				_currSound = 0;
				_audioPlayer.PlaySound(_CHARACTERTEXTSOUND);
			}
			
			yield return null;
		}
		_writingInProgress = false;
    }

	private void EndCutscene(){
		_inCutscene = false;
		_hudControl.gameObject.SetActive(true);
		HideCutsceneElements();
		_gameManager.EndCutscene(_nextScene);
	}

	private void HideCutsceneElements(){
		_tmpComponent.gameObject.SetActive(false);
		_backgroundImg.gameObject.SetActive(false);
	}
}
