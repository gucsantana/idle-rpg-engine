using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

/// Master script that handles all of the gameplay logic
public class GameManagerScript : MonoBehaviour
{
	/*
		Glossary of terms:
		
		Roam: time between fighting enemies, while the player is running around in the area
		Faint: the state of defeat if the player loses a combat
		
		Strength: physical damage
		Intelligence: magical damage and healing
		Agility: evasion
		Stamina: health and defense
	*/
	
	/// --- Singleton definitions ---
	private static GameManagerScript _instance;
	public static GameManagerScript Instance { get { return _instance; } }
	
	/// ------------------------------------------------------------
	/// ----------------------- constants -----------------------
	/// ------------------------------------------------------------
	// experience required for each level
	private int[] _EXPTABLE = new int[] {
		0, 100, 250, 600, 1080, 1600, 2250, 3000, 4060, 5800, 0 };	// level 1-10 (last digit is 0 for "no more levels")
		
	// strength gains per level
	private int[] _STRTABLE = new int[] {
		12, 1, 1, 1, 1, 1, 1, 2, 1, 1 };	// level 1-10
		
	// intelligence gains per level
	private int[] _INTTABLE = new int[] {
		10, 1, 2, 1, 1, 1, 2, 1, 1, 1 };	// level 1-10
		
	// agility gains per level
	private int[] _AGITABLE = new int[] {
		10, 2, 1, 1, 1, 2, 1, 1, 1, 2 };	// level 1-10
		
	// stamina gains per level
	private int[] _STATABLE = new int[] {
		10, 2, 3, 2, 2, 3, 4, 2, 3, 3 };	// level 1-10
		
	// the level at which you stop gaining experience from an area
	private int[] _AREALEVELCAPS = new int[] {
		4, 8, 10, 10 };	// areas 1-3
		
	// the minimum level to enter a given area
	private int[] _AREALEVELREQS = new int[] {
		// 0, 3, 6, 10 };	// areas 1-3
		// 0, 0, 0, 0 };	// areas 1-3
		0, 2, 3, 4 };	// areas 1-3
	
	private const int _LASTAREA = 4;							// id of the last area currently available
	private const int _MAXLEVEL = 10;							// maximum level the player can reach
	
	private const float _TIMEBETWEENENEMIES = 4.0f;				// how long between enemy fights on the same area
	private const float _TIMEBETWEENTURNS = 2.1f;				// how long between turns in a combat
	private const float _TIMERECOVERY = 60f;					// how long the player stays fainted after a defeat
	private const float _TIMEBEFORECOMBATSTARTS = 1.2f;			// the wait between an enemy spawning and combat starting
	private const float _TIMEBASICATKANIMHIT = 0.25f;			// the wait between the player's basic attack animation start and when it should 'hit' the enemy
	
	private const int _BASEPLAYERHP = 100;
	private const int _BASEPLAYERCRIT = 5;
	private const float _CRITMULTIPLIER = 1.5f;
	
	private const int _LOOTBOXMAXNUMBER = 10;				// maximum number of unopened loot boxes the player can amass
	
	private string _GAMEPLAYTEXT = "Gameplay";				// the name of the textcollection table that houses the gameplay messages
	
	/// ------------------------------------------------------------
	/// --- area data ---
	private string[] _areaNames = new string[] {
		"Santa Vapor Beach",		// 1
		"Pink Dolphin Mall", 		// 2
		"The Backrooms"			// 3
	};
	
	private string[] _areaSubtitles = new string[] {
		"A multitude of corridors into the unknown.",		// 1
		"Artificial canals carry fresh water to the depths.",	// 2
		"Remains of a holy place, scattered by the winds."	// 3
	};
	
	/// ------------------------------------------------------------
	/// --- element references ---
	private HUDController _hudControl { get { return HUDController.Instance; } }
	private AudioManager _audioMg { get { return AudioManager.Instance; } }
	private CutsceneManager _cutscenemg { get { return CutsceneManager.Instance; } }
	private ArmoryManager _armory;
	private BestiaryManager _bestiary;
	
	private GameObject _playerModel;
	private Animator _playerAnim;
	private GameObject _enemyAnchor;
	private Animator _enemyAnim;
	
	/// --- localization references---
	private StringTable _gameplayTextTable;
	
	/// --- player equipped gear ---
	[Header("Player Equipped Gear")]
	public Weapon _equipWeapon;
	public Headgear _equipHead;
	public Armor _equipArmor;
	public Boots _equipBoots;
	public Accessory _equipAcc;
	public Skill _equipSkill;
	
	/// --- player inventory ---
	[Header("Gear Inventory")]
	public List<Weapon> _weaponList = new List<Weapon>();
	public List<Headgear> _headgearList = new List<Headgear>();
	public List<Armor> _armorList = new List<Armor>();
	public List<Boots> _bootsList = new List<Boots>();
	public List<Accessory> _accList = new List<Accessory>();
	public List<Skill> _skillList = new List<Skill>();
	
	/// --- player stat values ---
	[Header("Player Stat Values")]
	public int _playerLevel;
	
	public int _statHpCurrent;
	public int _statHpMax;
	
	public int _statStrength = 12;
	public int _statIntelligence = 10;
	public int _statAgility = 10;
	public int _statStamina = 10;
	
	public int _statStrengthModded;
	public int _statIntelligenceModded;
	public int _statAgilityModded;
	public int _statStaminaModded;
	
	public int _statAttack;
	public int _statMagic;
	public int _statEvasion;
	public int _statDefense;
	
	public int _statCrit;
	
	public int _experience;
	
	/// --- loot elements ---
	public List<LootTableElement> _lootBox = new List<LootTableElement>();
	private int _lootBoxSize { get { return _lootBox.Count; } }
	
	/// --- enemy control variables ---
	public EnemyType _currentEnemyType;
	private GameObject _currentEnemyInstance;
	private Animator _currentEnemyAnim;
	private int _currentEnemyHealth;
	
	/// --- misc variables and toggles ---
	public int _currentArea = 0;
	private bool _inCombat = false;
	public bool _facingEnemy = false;
	public bool _fainted = false;
	private bool _isDuringFinalBlow = false;
	private float _currentRoamTimer = 0f;
	private float _currentFaintTimer = 0f;
	private Coroutine _combatCoroutine;
	private bool _inCutscene = false;

	///  --- queues ---
	public bool _equipChangeQueued = false;
	public bool _areaChangeQueued = false;
	private bool _areaChangeQueuedIsPrev = false;
	private bool _areaChangeQueuedIsNext = false;
	private Weapon _queuedWeapon;
	private Headgear _queuedHeadgear;
	private Armor _queuedArmor;
	private Boots _queuedBoots;
	private Accessory _queuedAcc;
	private Skill _queuedSkill;
	
	public bool _autoSave = true;
	public int _saveSlot = 1;
	public bool _testSave = false;

	/// --- debug block ---
	public bool _debugMode = true;
	public int _startingLevel = 1;
	public int _startingArea = 1;
	public bool _unlockAllGear = false;
	public bool _skipIntro = false;
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	
	/// --- Initialization ---
	IEnumerator Start()
	{
		GameObject.DontDestroyOnLoad(this);
		
		_armory = GetComponent<ArmoryManager>();
		_bestiary = GetComponent<BestiaryManager>();
		
		// wait until the localization system is finished loading
		var _loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(_GAMEPLAYTEXT);
		yield return _loadingOperation;
		
		// throw log error on failure to load
		if (_loadingOperation.Status == AsyncOperationStatus.Succeeded)
			_gameplayTextTable = _loadingOperation.Result;
		else
			Debug.LogError("Could not load String Table\n" + _loadingOperation.OperationException.ToString());
		
		// debug code
		if(_playerLevel == 1 && _experience == 0 && _currentArea >= 1)
			StartCoroutine(InitializePlayer());
		
	}
	
	void Update()
	{
		// avoids any changes while in the main menu or during a cutscene
		if(_currentArea == 0 || _inCutscene)
			return;
		
		// if not fainted and not currently against an enemy, tick down the clock and check when to spawn a new enemy and start combat
		if(!_fainted && !_facingEnemy)
		{
			// run the events in our queue whenever available
			if(_areaChangeQueued)
			{
				if(_areaChangeQueuedIsNext){
					_areaChangeQueuedIsNext = false;
					MoveToNextArea();
				} else if(_areaChangeQueuedIsPrev){
					_areaChangeQueuedIsPrev = false;
					MoveToPreviousArea();
				} else {
					Debug.Log("_areaChangeQueued was true, but neither prev nor next were set. Check code.");
				}
				_areaChangeQueued = false;
			}
			if(_equipChangeQueued)
			{
				ExecuteEquipQueueChanges();
			}

			_currentRoamTimer += Time.deltaTime;
			if(_currentRoamTimer >= _TIMEBETWEENENEMIES)
			{
				_currentRoamTimer = 0f;
				SpawnNextEnemy();
				_combatCoroutine = StartCoroutine(Combat());
			}
		}
		
		// if fainted, tick down the clock until not fainted anymore, recover HP and get back to searching for an enemy
		else if(_fainted)
		{
			_currentFaintTimer += Time.deltaTime;
			// the timer is halved if the player is equipping the Peace Necklace
			if((_equipAcc?._name == "Peace Necklace" && _currentFaintTimer >= _TIMERECOVERY * 0.5 ) || _currentFaintTimer >= _TIMERECOVERY)
			{
				_currentFaintTimer = 0;
				_hudControl.SetPlayerHpToMax();
				
				_fainted = false;
				
				_playerAnim.SetBool("Recover",true);
			}
		}
		
		if(_testSave)
		{
			_testSave = false;
			SaveGame();
		}
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	/// new game button was clicked; play intro cutscene or go straight to game
	public void NewGameClicked()
	{
		Debug.Log("New game clicked.");

		// regardless of path, we initialize the player and set the starting area
		StartCoroutine(InitializePlayer());
		_currentArea = _startingArea;

		if(_debugMode && _skipIntro) {
			StartCoroutine( LoadArea(_startingArea) );
		} else {
			_cutscenemg.PlayCutscene(0);
		}
	}
	
	/// saves the game to a local file
	private void SaveGame()
	{
		SavedGame _saveFile = PackSavedGame();
		
		BinaryFormatter _bf = new BinaryFormatter();
		FileStream _file = File.Create(Application.persistentDataPath + "/savefile" + _saveSlot + ".mcd");
		_bf.Serialize(_file, _saveFile);
		_file.Close();
		
		Debug.Log("Game saved.");
		// _hudControl.CreateFloatingText( _text : "Game saved.", _variant : "info", _target : "info" );
	}
	
	/// loads the game from the local save file
	public void LoadGame()
	{
		if (File.Exists(Application.persistentDataPath + "/savefile" + _saveSlot + ".mcd"))
		{
			// loads the save file into memory
			BinaryFormatter _bf = new BinaryFormatter();
			FileStream _file = File.Open(Application.persistentDataPath + "/savefile" + _saveSlot + ".mcd", FileMode.Open);
			SavedGame _saveFile = (SavedGame) _bf.Deserialize(_file);
			_file.Close();
			
			// equips the previously equipped gear
			_equipWeapon = _saveFile._equipWeapon;
			_equipHead = _saveFile._equipHead;
			_equipArmor = _saveFile._equipArmor;
			_equipBoots = _saveFile._equipBoots;
			_equipAcc = _saveFile._equipAcc;
			_equipSkill = _saveFile._equipSkill;
			
			// changes the equip box icons to the currently equipped items
			if(_equipWeapon._id != 0)
				_hudControl.ChangeEquippedItemIcon("weapon", _equipWeapon._icon);
			if(_equipHead._id != 0)
				_hudControl.ChangeEquippedItemIcon("headgear", _equipHead._icon);
			if(_equipArmor._id != 0)
				_hudControl.ChangeEquippedItemIcon("armor", _equipArmor._icon);
			if(_equipBoots._id != 0)
				_hudControl.ChangeEquippedItemIcon("boots", _equipBoots._icon);
			if(_equipAcc._id != 0)
				_hudControl.ChangeEquippedItemIcon("accessory", _equipAcc._icon);
			if(_equipSkill._id != 0)
				_hudControl.ChangeEquippedItemIcon("skill", _equipSkill._icon);
			
			// loads the list of equipment previously obtained
			_weaponList = new List<Weapon>(_saveFile._weaponList);
			_headgearList = new List<Headgear>(_saveFile._headgearList);
			_armorList = new List<Armor>(_saveFile._armorList);
			_bootsList = new List<Boots>(_saveFile._bootsList);
			_accList = new List<Accessory>(_saveFile._accList);
			_skillList = new List<Skill>(_saveFile._skillList);

			// sets the player stats 
			_playerLevel =_saveFile. _playerLevel;
			_hudControl.SetPlayerLevel( _playerLevel);
			
			_statStrength = _saveFile._statStrength;
			_statIntelligence = _saveFile._statIntelligence;
			_statAgility = _saveFile._statAgility;
			_statStamina = _saveFile._statStamina;
			
			_experience = _saveFile._experience;
			
			// reloads the item drop box
			_lootBox = new List<LootTableElement>(_saveFile._lootBox);
			
			UpdatePlayerExp();
			UpdatePlayerStats();
			_hudControl.UpdateLootButtonText( _lootBoxSize);
			
			// loads the scene the player last saved at
			_currentArea = _saveFile._currentArea;
			StartCoroutine( LoadArea(_currentArea) );
		}
		else
			Debug.Log("No save file on slot " + _saveSlot + ".");
	}
	
	/// creates a saved game instance copying from the data currently loaded
	private SavedGame PackSavedGame ()
	{
		SavedGame _saveFile = new SavedGame();
		
		_saveFile._equipWeapon = _equipWeapon;
		Debug.Log("Saved equipped weapon: "+_equipWeapon?._name);
		_saveFile._equipHead = _equipHead;
		_saveFile._equipArmor = _equipArmor;
		_saveFile._equipBoots = _equipBoots;
		_saveFile._equipAcc = _equipAcc;
		_saveFile._equipSkill = _equipSkill;
		
		_saveFile._weaponList = new List<Weapon>(_weaponList);
		_saveFile._headgearList = new List<Headgear>(_headgearList);
		_saveFile._armorList = new List<Armor>(_armorList);
		_saveFile._bootsList = new List<Boots>(_bootsList);
		_saveFile._accList = new List<Accessory>(_accList);
		_saveFile._skillList = new List<Skill>(_skillList);
		
		_saveFile._playerLevel = _playerLevel;
		
		_saveFile._statStrength = _statStrength;
		_saveFile._statIntelligence = _statIntelligence;
		_saveFile._statAgility = _statAgility;
		_saveFile._statStamina = _statStamina;
		
		_saveFile._experience = _experience;
		_saveFile._currentArea = _currentArea;
		
		_saveFile._lootBox = new List<LootTableElement>(_lootBox);
		
		_saveFile._tacticianMode = false;
		
		return _saveFile;
	}

	/// finishes the current cutscene and moves to the next appropriate area
	public void EndCutscene(string next) {
		if(_inCutscene){
			_inCutscene = false;
			switch(next){
				case "nextarea":
					StartCoroutine( LoadArea(_currentArea) );
					break;
				case "credits":
					_cutscenemg.PlayCutscene(3,next:"mainmenu");
					break;
				case "mainmenu":
					_hudControl.gameObject.SetActive(false);
					SceneManager.LoadScene("MainMenu");
					break;
				default:
					Debug.Log("Unreachable case "+next+" on the switch at EndCutscene(). Check your code.");
					break;
			}
		} else {
			Debug.Log("Attempted to call EndCutscene() on GameManager while not on cutscene. Check code.");
		}
	}
	
	/// quits the game
	public void QuitGame()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	// makes all of the checks to see if the player can move into the previous area
	public void AttemptPreviousArea ()
	{
		if(_currentArea == 1)
		{
			Debug.Log("Attempting to enter unreachable area, check code.");
			return;
		}

		if(_isDuringFinalBlow)	return;

		if(!_inCombat && !_fainted)
			MoveToPreviousArea();
		else {
			_areaChangeQueued = true;
			_areaChangeQueuedIsPrev = true;
			_areaChangeQueuedIsNext = false;
		}
	}

	// makes all of the checks to see if the player can move into the next area
	public void AttemptNextArea ()
	{
		if(_currentArea == _LASTAREA)
		{
			Debug.Log("Attempting to enter unreachable area, check code.");
			return;
		}

		if(_isDuringFinalBlow)	return;
		
		if( _playerLevel < _AREALEVELREQS[_currentArea])
		{
			_hudControl.DisplayErrorMessage( GetString(_gameplayTextTable,"ERR_INSUFF_LEVEL").Replace("$lvl",_AREALEVELREQS[_currentArea].ToString()) );
		}

		if(!_inCombat && !_fainted)
			MoveToNextArea();
		else {
			_areaChangeQueued = true;
			_areaChangeQueuedIsNext = true;
			_areaChangeQueuedIsPrev = false;
		}
	}

	// move the player into the previous area
	public void MoveToPreviousArea()
	{
		StartCoroutine( LoadArea (_currentArea - 1) );
	}
	
	// move the player into the next area
	public void MoveToNextArea()
	{
		StartCoroutine(LoadArea (_currentArea + 1));
	}
	
	// loads a given area
	private IEnumerator LoadArea(int _areaToLoad)
	{
		// avoids any changes if we're in the correct area already
		if(_areaToLoad == _currentArea)
		{
			Debug.Log("Load Area failed due to _areaToLoad == _currentArea");
			yield return 0;
		}
		
		_currentArea = _areaToLoad;
		_hudControl.gameObject.SetActive(true);
		
		SceneManager.LoadScene("Area_"+_areaToLoad);
		
		if(_currentArea >= 2)
			_hudControl.TogglePrevAreaButton(true);
		else
			_hudControl.TogglePrevAreaButton(false);

		// if we're not in the last area yet, we turn on the button for the next area, and check whether it's locked or unlocked
		if(_currentArea < _LASTAREA){
			Debug.Log("Player level is "+_playerLevel+", next area level req is "+_AREALEVELREQS[_currentArea]);
			_hudControl.ToggleNextAreaButton(true);
			if(_playerLevel >= _AREALEVELREQS[_currentArea])
				_hudControl.UnlockNextAreaButton(true);
			else
				_hudControl.UnlockNextAreaButton(false);
		}
		else
			_hudControl.ToggleNextAreaButton(false);
		
		yield return new WaitUntil( ( ) => GameObject.FindWithTag("PlayerAnchor") != null);	// wait for the area to finish loading
		
		if(_playerModel == null)
		{
			GameObject _playerPrefab = Resources.Load<GameObject>("Player Prefabs/PlayerInstance");
			GameObject _playerAnchor = GameObject.FindWithTag("PlayerAnchor");
			_playerModel = Instantiate( _playerPrefab, _playerAnchor.transform.position, _playerAnchor.transform.rotation);
			
			_playerAnim = _playerModel.GetComponent<Animator>();
			GameObject.DontDestroyOnLoad(_playerModel);
		}
		
		VoidCombat();
	}
	
	// resets all of the variables related to combat
	private void VoidCombat()
	{
		_hudControl.ToggleEnemyData(false);
		if(_currentEnemyInstance != null) Destroy(_currentEnemyInstance);
		
		_inCombat = false;
		_facingEnemy = false;
		if(_combatCoroutine != null) StopCoroutine(_combatCoroutine);
		
		ResetPlayerAnimation();
		
		_currentRoamTimer = 0f;
		_currentFaintTimer = 0f;
		
		//_hudControl.SetPlayerHpToMax();
		_statHpCurrent = _statHpMax;
	}

	// executes the "final blow" on a boss; this has a bunch of effects, mostly ending the game and rebooting to the main menu
	public void ExecuteFinalBlow()
	{
		if(!_isDuringFinalBlow) {
			Debug.Log("Attempted to execute Final Blow while not during Final Blow time. Check code.");
			return;
		}

		VoidCombat();
		_currentArea = 0;
		_isDuringFinalBlow = false;
		_hudControl.ToggleFinalBlowButton(false);

		// clean up all parameters
		Destroy(_playerModel);
		_equipWeapon = new Weapon();
		_equipHead = new Headgear();
		_equipArmor = new Armor();
		_equipBoots = new Boots();
		_equipAcc = new Accessory();
		_equipSkill = new Skill();
		_weaponList = new List<Weapon>();
		_headgearList = new List<Headgear>();
		_armorList = new List<Armor>();
		_bootsList = new List<Boots>();
		_accList = new List<Accessory>();
		_skillList = new List<Skill>();
		
		_hudControl.ChangeEquippedItemIcon("weapon", _equipWeapon._icon);
		_hudControl.ChangeEquippedItemIcon("headgear", _equipHead._icon);
		_hudControl.ChangeEquippedItemIcon("armor", _equipArmor._icon);
		_hudControl.ChangeEquippedItemIcon("boots", _equipBoots._icon);
		_hudControl.ChangeEquippedItemIcon("accessory", _equipAcc._icon);
		_hudControl.ChangeEquippedItemIcon("skill", _equipSkill._icon);

		_cutscenemg.PlayCutscene(2,next:"credits");

		// debug mode variables reset
		_startingArea = 1;
		_startingLevel = 1;
		_skipIntro = true;
		_unlockAllGear = false;
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// updates the player stat calculations after a level up or item change
	private void UpdatePlayerStats ()
	{
		_statHpMax = _BASEPLAYERHP + (_playerLevel * 20) + (_statStamina * 10);
		_statHpCurrent = _statHpMax;
		
		_statStrengthModded = _statStrength;
		_statIntelligenceModded = _statIntelligence;
		_statAgilityModded = _statAgility;
		_statStaminaModded = _statStamina;
		
		_statAttack = (int)(_statStrengthModded * 1.2) + (_equipWeapon != null ? _equipWeapon._atkValue : 0) + (_equipHead != null ? _equipHead._atkValue : 0) + (_equipArmor != null ? _equipArmor._atkValue : 0) 
			+ (_equipBoots != null ? _equipBoots._atkValue : 0) + (_equipAcc != null ? _equipAcc._atkValue : 0);
		_statMagic = _statIntelligenceModded;
		_statEvasion = _statAgilityModded / 2 + (_equipArmor != null ? _equipArmor._evaValue : 0) + (_equipHead != null ? _equipHead._evaValue : 0) + (_equipBoots != null ? _equipBoots._evaValue : 0) + (_equipAcc != null ? _equipAcc._evaValue : 0);
		_statDefense = (_statStaminaModded / 2) + (_equipWeapon != null ? _equipWeapon._defValue : 0) + (_equipHead != null ? _equipHead._defValue : 0) + (_equipArmor != null ? _equipArmor._defValue : 0) 
			+ (_equipBoots != null ? _equipBoots._defValue : 0) + (_equipAcc != null ? _equipAcc._defValue : 0);
		
		_statCrit = _BASEPLAYERCRIT + (_equipWeapon != null ? _equipWeapon._critValue : 0) + (_equipAcc != null ? _equipAcc._critValue : 0) + (_equipHead != null ? _equipHead._critValue : 0);
		
		_hudControl.SetPlayerStats( _statAttack, _statMagic, _statEvasion, _statDefense);
	}
	
	// tally the player's exp, and update the level if necessary
	private void UpdatePlayerExp ()
	{
		_hudControl.SetPlayerExp( _experience, _EXPTABLE[ _playerLevel ] );
		
		if(_playerLevel < _MAXLEVEL) UpdatePlayerLevel();
	}
	
	private void UpdatePlayerLevel ()
	{
		if(_playerLevel < _MAXLEVEL && _experience >= _EXPTABLE[_playerLevel])
		{
			_playerLevel++;
			Debug.Log("Level up! Advanced to level "+_playerLevel);
			
			_statStrength += _STRTABLE[_playerLevel -1];
			_statIntelligence += _INTTABLE[_playerLevel -1];
			_statAgility += _AGITABLE[_playerLevel -1];
			_statStamina +=  _STATABLE[_playerLevel -1];
			
			UpdatePlayerStats();
			
			_hudControl.SetPlayerLevel(_playerLevel);
			_hudControl.SetPlayerExp( _experience, _EXPTABLE[ _playerLevel ] );
			_hudControl.CreateFloatingText( _text : "Level up! Advanced to level "+_playerLevel, _variant : "info", _target : "info" );
			_hudControl.PushEventLog("Level up! Advanced to level "+_playerLevel+".");

			// check whether to unlock the next area button
			if(_playerLevel == _AREALEVELREQS[_currentArea])
				_hudControl.UnlockNextAreaButton(true);
			
			// recursively calls UpdatePlayerLevel, just in case there's a large enough exp gain to increase multiple levels
			UpdatePlayerLevel();
		}
	}

	// sets the starting stats for the player; usually just the first entry in the stat table, but we need to add up the stats and change the values around for a debug start
	private void SetPlayerStartingStats(){
		if(_debugMode && _startingLevel >= 2){
			_statStrength = _STRTABLE[0.._startingLevel].Sum();
			_statIntelligence = _INTTABLE[0.._startingLevel].Sum();
			_statAgility = _AGITABLE[0.._startingLevel].Sum();
			_statStamina =  _STATABLE[0.._startingLevel].Sum();

			// set level to starting level, and exp to the required for that level
			_playerLevel = _startingLevel;
			_experience = _EXPTABLE[_startingLevel - 1];
			
			_hudControl.SetPlayerLevel(_playerLevel);
			_hudControl.SetPlayerExp( _experience, _EXPTABLE[ _playerLevel ] );
		} else {
			_statStrength = _STRTABLE[0];
			_statIntelligence = _INTTABLE[0];
			_statAgility = _AGITABLE[0];
			_statStamina =  _STATABLE[0];
		}
	}
	
	// sets the basic equipment and stats for a new game player
	private IEnumerator InitializePlayer()
	{
		yield return new WaitUntil( ( ) => _hudControl._finishedLoad == true);	// wait for the HUD manager to get all of the references

		SetPlayerStartingStats();
		
		_weaponList.Add(_armory._WEAPONLIST[0]);
		_armorList.Add(_armory._ARMORLIST[0]);
		_bootsList.Add(_armory._BOOTSLIST[0]);
		
		if(_debugMode && _unlockAllGear)
			AddAllGearToInventory();
		
		EquipItem("weapon", _weapon : _weaponList[0]);
		EquipItem("armor", _armor : _armorList[0]);
		EquipItem("boots", _boots : _bootsList[0]);
		
		UpdatePlayerStats();
		Debug.Log(_equipHead);
	}
	
	// resets the player's animations to the idle state
	private void ResetPlayerAnimation()
	{
		_playerAnim?.Rebind();
		_playerAnim?.Update(0f);
	}
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	private void SpawnNextEnemy()
	{
		_enemyAnchor = GameObject.FindWithTag("EnemyAnchor");
		
		_currentEnemyType = _bestiary.GetNextEnemy(_currentArea);
		GameObject _enemyPrefab = Resources.Load<GameObject>("Enemy Prefabs/" + _currentEnemyType._modelName);
		
		_hudControl.PushEventLog(_currentEnemyType._encounterQuote);
		
		_hudControl.SetEnemyHpToMax();
		_hudControl.SetEnemyName(_currentEnemyType._name);
		_hudControl.ToggleEnemyData(true);
		
		_currentEnemyInstance = Instantiate( _enemyPrefab, _enemyAnchor.transform.position, _enemyAnchor.transform.rotation);
		_currentEnemyAnim = _currentEnemyInstance.GetComponent<Animator>();
		_currentEnemyHealth = _currentEnemyType._hpValue;
	}
	
	private void LootRoll()
	{
		LootTableElement _drop = _currentEnemyType.GenerateLootRoll();
		
		if( _drop != null && _lootBoxSize < _LOOTBOXMAXNUMBER)
		{
			Debug.Log("Dropped an item: "+ _drop._itemType);
			_lootBox.Add(_drop);
			_hudControl.UpdateLootButtonText( _lootBoxSize);
			
			string _itemDropString = GetString(_gameplayTextTable,"GET_ITEM"+ (_drop._rare == true ? "_RARE" : "") );
			StartCoroutine( _hudControl.CreateDelayedFloatingText(_text : _itemDropString , _variant : "info", _target : "info") );
			_hudControl.PushEventLog(_itemDropString);
		}
		else if(_drop != null && _lootBoxSize >= _LOOTBOXMAXNUMBER)
		{
			// loot box is full, items won't be obtained, send message to warn player
			Debug.Log("The loot box is full! Item discarded.");
			_hudControl.PushEventLog(GetString(_gameplayTextTable,"LOOTBOX_FULL"));
		}
	}
	
	// opens an item box and adds it to the list if it's a new item
	public void UnpackItem (int _index)
	{
		bool _found = false; 	// control variable for the item search
		
		LootTableElement _element = _lootBox[_index];
		_lootBox[_index]._opened = true;
		
		switch(_element._itemType)
		{
			case "weapon":
				// ---------------- weapon ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._weapon._icon);
				foreach(Weapon _item in _weaponList)
				{
					if (_item._id == _element._weapon._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._weapon._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_weaponList.Add(_element._weapon);
					Debug.Log("Added "+_element._weapon._name +" to the inventory.");
					SortItemList("weapon");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
			case "headgear":
				// ---------------- headgear ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._headgear._icon);
				foreach(Headgear _item in _headgearList)
				{
					if (_item._id == _element._headgear._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._headgear._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_headgearList.Add(_element._headgear);
					Debug.Log("Added "+_element._headgear._name +" to the inventory.");
					SortItemList("headgear");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
			case "armor":
				// ---------------- armor ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._armor._icon);
				foreach(Armor _item in _armorList)
				{
					if (_item._id == _element._armor._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._armor._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_armorList.Add(_element._armor);
					Debug.Log("Added "+_element._armor._name +" to the inventory.");
					SortItemList("armor");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
			case "boots":
				// ---------------- boots ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._boots._icon);
				foreach(Boots _item in _bootsList)
				{
					if (_item._id == _element._boots._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._boots._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_bootsList.Add(_element._boots);
					Debug.Log("Added "+_element._boots._name +" to the inventory.");
					SortItemList("boots");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
			case "skill":
				// ---------------- skill ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._skill._icon);
				foreach(Skill _item in _skillList)
				{
					if (_item._id == _element._skill._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._skill._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_skillList.Add(_element._skill);
					Debug.Log("Added "+_element._skill._name +" to the inventory.");
					SortItemList("skill");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
			case "accessory":
				// ---------------- accessory ---------------- //
				_hudControl.DisplayItemImage(_index, _icon : _element._accessory._icon);
				foreach(Accessory _item in _accList)
				{
					if (_item._id == _element._accessory._id)
					{
						_found = true;
						break;
					}
					if (_item._id > _element._accessory._id)
					{
						break;
					}
				}
				if(!_found)
				{
					_accList.Add(_element._accessory);
					Debug.Log("Added "+_element._accessory._name +" to the inventory.");
					SortItemList("accessory");
				}
				else
				{
					Debug.Log("Already have this item, discarding.");
				}
				break;
		}
	}
	
	/// checks current battle status to decide whether item is equipped immediately or equip is queued
	public void TryEquipItem (string _category, Weapon _weapon = null, Headgear _headgear = null, Armor _armor = null, Boots _boots = null, Accessory _accessory = null, Skill _skill = null)
	{
		// upon trying to equip an item, the item list is closed, along with the tooltip
		_hudControl.HideItemTooltip();
		_hudControl.CloseItemList();

		if(!_inCombat) {
			EquipItem(_category,_weapon,_headgear,_armor,_boots,_accessory,_skill);
		} else {
			//TODO: little icon indicating certain item being changed
			_equipChangeQueued = true;
			switch(_category)
			{
				case "weapon":
					if(_equipWeapon._name != _weapon._name)
					{
						_queuedWeapon = _weapon;
						_hudControl.SetEquipChangeMarkerVisibility(true,"weapon");
					} else {
						_queuedWeapon = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"weapon");
					}
					break;
				case "headgear":
					if(_equipHead._name != _headgear._name)
					{
						_queuedHeadgear = _headgear;
						_hudControl.SetEquipChangeMarkerVisibility(true,"headgear");
					} else {
						_queuedHeadgear = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"headgear");
					}
					break;
				case "armor":
					if(_equipArmor._name != _armor._name)
					{
						_queuedArmor = _armor;
						_hudControl.SetEquipChangeMarkerVisibility(true,"armor");
					} else {
						_queuedArmor = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"armor");
					}
					break;
				case "boots":
					if(_equipBoots._name != _boots._name)
					{
						_queuedBoots = _boots;
						_hudControl.SetEquipChangeMarkerVisibility(true,"boots");
					} else {
						_queuedBoots = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"boots");
					}
					break;
				case "accessory":
					if(_equipAcc._name != _accessory._name)
					{
						_queuedAcc = _accessory;
						_hudControl.SetEquipChangeMarkerVisibility(true,"accessory");
					} else {
						_queuedAcc = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"accessory");
					}
					break;
				case "skill":
					if(_equipSkill._name != _skill._name)
					{
						_queuedSkill = _skill;
						_hudControl.SetEquipChangeMarkerVisibility(true,"skill");
					} else {
						_queuedSkill = null;
						_hudControl.SetEquipChangeMarkerVisibility(false,"skill");
					}
					break;
			}
			Debug.Log("Queued weapon: "+_queuedWeapon?._name+", queued headgear: "+_queuedHeadgear?._name+", queued armor: "+_queuedArmor?._name+", queued boots: "+_queuedBoots?._name+", queued acc: "+_queuedAcc?._name+", queued skill: "+_queuedSkill?._name);
			// if the entire queue has been emptied, we remove the queue bool
			if(_queuedWeapon == null && _queuedHeadgear == null && _queuedArmor == null && _queuedBoots == null && _queuedAcc == null && _queuedSkill == null)
				_equipChangeQueued = false;
			_hudControl.SetEquipQueueVisibility(_equipChangeQueued);
		}
	}

	/// equips the passed item, according to the category
	public void EquipItem (string _category, Weapon _weapon = null, Headgear _headgear = null, Armor _armor = null, Boots _boots = null, Accessory _accessory = null, Skill _skill = null)
	{
		switch(_category)
		{
			case "weapon":
				if(_weapon._name == "remove")
					_equipWeapon = new Weapon();
				else
					_equipWeapon = _weapon;
				_hudControl.ChangeEquippedItemIcon("weapon", _weapon._icon);
				break;
			case "headgear":
				if(_headgear._name == "remove")
					_equipHead = new Headgear();
				else
					_equipHead = _headgear;
				_hudControl.ChangeEquippedItemIcon("headgear", _headgear._icon);
				break;
			case "armor":
				if(_armor._name == "remove")
					_equipArmor = new Armor();
				else
					_equipArmor = _armor;
				_hudControl.ChangeEquippedItemIcon("armor", _armor._icon);
				break;
			case "boots":
				if(_boots._name == "remove")
					_equipBoots = new Boots();
				else
					_equipBoots = _boots;
				_hudControl.ChangeEquippedItemIcon("boots", _boots._icon);
				break;
			case "accessory":
				if(_accessory._name == "remove")
					_equipAcc = new Accessory();
				else
					_equipAcc = _accessory;
				_hudControl.ChangeEquippedItemIcon("accessory", _accessory._icon);
				break;
			case "skill":
				if(_skill._name == "remove")
					_equipSkill = new Skill();
				else
					_equipSkill = _skill;
				_hudControl.ChangeEquippedItemIcon("skill", _skill._icon);
				break;
		}
		
		UpdatePlayerStats();
	}

	/// executes all equipment changes in the queue 
	private void ExecuteEquipQueueChanges()
	{
		if(_queuedWeapon?._name != null){
			EquipItem(_category:"weapon",_weapon:_queuedWeapon);
			_queuedWeapon = null;
		}
		if(_queuedHeadgear?._name != null){
			EquipItem(_category:"headgear",_headgear:_queuedHeadgear);
			_queuedHeadgear = null;
		}
		if(_queuedArmor?._name != null){
			EquipItem(_category:"armor",_armor:_queuedArmor);
			_queuedArmor = null;
		}
		if(_queuedBoots?._name != null){
			EquipItem(_category:"boots",_boots:_queuedBoots);
			_queuedBoots = null;
		}
		if(_queuedAcc?._name != null){
			EquipItem(_category:"accessory",_accessory:_queuedAcc);
			_queuedAcc = null;
		}
		if(_queuedSkill?._name != null){
			EquipItem(_category:"skill",_skill:_queuedSkill);
			_queuedSkill = null;
		}
		_equipChangeQueued = false;
		_hudControl.SetEquipQueueVisibility(_equipChangeQueued);
	}
	
	// sorts the items in the passed category
	private void SortItemList (string _category)
	{
		switch(_category)
		{
			case "weapon":
				_weaponList = _weaponList.OrderBy(x => x._id).ToList();
				break;
			case "headgear":
				_headgearList = _headgearList.OrderBy(x => x._id).ToList();
				break;
			case "armor":
				_armorList = _armorList.OrderBy(x => x._id).ToList();
				break;
			case "boots":
				_bootsList = _bootsList.OrderBy(x => x._id).ToList();
				break;
			case "skill":
				_skillList = _skillList.OrderBy(x => x._id).ToList();
				break;
			case "accessory":
				_accList = _accList.OrderBy(x => x._id).ToList();
				break;
		}
	}
	
	// removes all of the opened boxes from the loot box
	public void ClearOpenedBoxes()
	{
		_lootBox.RemoveAll(x => x._opened == true);
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	/// executes the combat between player and enemy, including all calculations and skills, and the results of the combat
	private IEnumerator Combat ()
	{
		_inCombat = true;
		_facingEnemy = true;
		int _calcPlayerDmg = 0;
		bool _bossCombat = _currentEnemyType._boss;
		if(_bossCombat) _hudControl.CreateFloatingText( _text : "Entering a boss fight!", _variant : "info", _target : "info" );
		
		// sets the player's skill stats and other variables
		int _curCd = 0;
		int _cooldown = (_equipSkill != null ? _equipSkill._cooldown : 0);
		float _modifier = (_equipSkill != null ? _equipSkill._multiplier : 0f);
		string _specialType = (_equipSkill != null ? _equipSkill._effect : string.Empty);
		string _enemyTag = _currentEnemyType._tag;
		bool _skillTurn = false;
		
		yield return new WaitForSeconds(_TIMEBEFORECOMBATSTARTS);
		
		// core combat loop
		while (_inCombat)
		{
			string _calcText = "";			// the text to be displayed as combat text
			string _variant = "damage";		// the type of text we'll call over at the HUD when displaying damage info, default being damage
			int _cureTotal = 0;				// the amount healed by a healing skill
			
			//---------------------------------------------------------------------------------------------------------------//
			// play player attack animation
			_playerAnim.SetBool("Basic Attack",true);
			Debug.Log("Player Attack!");
			
			// decided whether a skill will be used
			_curCd++;
			if(_curCd >= _cooldown && !string.IsNullOrWhiteSpace(_specialType)) {
				_curCd = 0; _skillTurn = true;
			}
			
			//---------------------------------------------------------------------------------------------------------------//
			// calculates the damage from the player against the enemy, following the type of attack
			if(_skillTurn)
			{
				switch(_specialType)
				{
					case "damage":
						Debug.Log("Player used a Damage-type skill!");
						_variant = "special";
						_calcPlayerDmg = CalculateDamageFF(_statAttack : _statAttack, _statLevel : _playerLevel, _targetDef : _currentEnemyType._defValue, _targetLevel : _currentEnemyType._level, _modifier : _modifier);
						break;
					case "heal":
						Debug.Log("Player used a Healing skill!");
						_variant = "healing";
						_calcPlayerDmg = 0;
						_cureTotal = (int) Mathf.Floor( _statMagic * 2 * _modifier );
						_statHpCurrent = Mathf.Min( _statHpCurrent + _cureTotal, _statHpMax );
						_hudControl.SetPlayerHpFill( _statHpMax, _statHpCurrent);
						break;
					case "flying-mastery":
						Debug.Log("Player used a Flying Mastery skill!");
						_variant = "damage";
						_calcPlayerDmg = CalculateDamageFF(_statAttack : _statAttack, _statLevel : _playerLevel, _targetDef : _currentEnemyType._defValue, _targetLevel : _currentEnemyType._level, _modifier : (_enemyTag.Equals("flying") ? _modifier : 1));
						break;
				}

				// write the skill name as floating text
				_hudControl.CreateFloatingText(
				_text : _equipSkill._name, 
				_variant : "damage", 
				_target : "player" );
			}
			else
				// default attack calculation
				_calcPlayerDmg = CalculateDamageFF(_statAttack : _statAttack, _statLevel : _playerLevel, _targetDef : _currentEnemyType._defValue, _targetLevel : _currentEnemyType._level);
			
			int _playerHit = Random.Range(1,101);
			
			//---------------------------------------------------------------------------------------------------------------//
			// calculates critical hits
			if(Random.Range(1,101) < _statCrit)
			{
				Debug.Log("CRITICAL!");
				_variant = "critical";
				_calcPlayerDmg = (int) Mathf.Floor( _calcPlayerDmg * _CRITMULTIPLIER );
			}
			
			//---------------------------------------------------------------------------------------------------------------//
			// deals the damage to the enemy's health, if the player managed to hit
			if(_playerHit > _currentEnemyType._evaValue && _facingEnemy)
				_currentEnemyHealth = Mathf.Max ( _currentEnemyHealth - _calcPlayerDmg , 0);
			else
			{
				Debug.Log("Player missed! Roll = " + _playerHit + ", Enemy Evasion = "+_currentEnemyType._evaValue);
				_variant = "miss";
			}
			
			//---------------------------------------------------------------------------------------------------------------//
			// decide the text to write, based on calculations
			if( _variant == "healing" ) _calcText = _cureTotal.ToString();
			else if( _variant == "miss" ) _calcText = "miss!";
			else _calcText = _calcPlayerDmg.ToString();
			
			// wait a few moments to account for the time the animation takes to reach the desired position
			float _playerAttackTiming = _TIMEBASICATKANIMHIT;
			yield return new WaitForSeconds( _playerAttackTiming );
			
			// sends the text to the screen to display the results of the calculations
			_hudControl.CreateFloatingText(
				_text :  _calcText, 
				_variant : _variant, 
				_target : (_variant != "healing" ? "enemy" : "player") );
				
			// pushes the event to the event log, and plays the appropriate effect animation
			string _eventText = string.Empty;
			switch (_variant)
			{
				case "healing":
					_eventText = GetString(_gameplayTextTable,"EVENT_HEAL").Replace("$curetotal",_cureTotal.ToString()).Replace("$skillname",_equipSkill._name);
					break;
				case "miss":
					_eventText = GetString(_gameplayTextTable,"EVENT_MISS");
					break;
				case "special":
					_eventText = GetString(_gameplayTextTable,"EVENT_SPECIAL").Replace("$dmg",_calcPlayerDmg.ToString()).Replace("$skillname",_equipSkill._name); 
					break;
				case "critical":
					_eventText = GetString(_gameplayTextTable,"EVENT_CRIT").Replace("$dmg",_calcPlayerDmg.ToString()); 
					break;
				case "damage":
					// text
					_eventText = GetString(_gameplayTextTable,"EVENT_DMG").Replace("$dmg",_calcPlayerDmg.ToString()); 
					// animation - we find the prefab for the correct animation and instantiate it over the enemy anchor, modified by the size of the target
					GameObject _animPrefab = Resources.Load<GameObject>("Animation Prefabs/BladeCut");
					GameObject _animObj = Instantiate( _animPrefab, new Vector3(_enemyAnchor.transform.position.x, _enemyAnchor.transform.position.y + _currentEnemyType._modelHeight, 0), _enemyAnchor.transform.rotation);
					// animation - play the 'take damage' animation on the enemy, if it exists
					_currentEnemyAnim.SetBool("Damage", true);
					break;
			}
			_hudControl.PushEventLog(_eventText);
			
			//---------------------------------------------------------------------------------------------------------------//
			Debug.Log("Enemy Max HP: "+_currentEnemyType._hpValue + ", Current HP: " + _currentEnemyHealth);
			_hudControl.SetEnemyHpFill( _currentEnemyType._hpValue, _currentEnemyHealth);
			
			// If the enemy is dead after the current round of calculations, we play their death animation and check out of combat
			if(_currentEnemyHealth <= 0)
			{
				Debug.Log("Enemy Defeated!");
				_currentEnemyAnim.SetBool("Death",true);
				_inCombat = false;
				break;
			}
			
			_skillTurn = false;
			
			yield return new WaitForSeconds(_TIMEBETWEENTURNS - _playerAttackTiming);
			
			/// --------------------------------------------------------------------------------------------------------------//
			/// --------------------------- ENEMY TURN ------------------------------------------------------------------//
			/// --------------------------------------------------------------------------------------------------------------//
			
			_variant = "damage";
			
			// play enemy attack animation
			_currentEnemyAnim.SetInteger("Attack Proc",Random.Range(0,101));
			_currentEnemyAnim.SetBool("Attack",true);
			Debug.Log("Enemy Attack!");
			
			// calculates the damage from the enemy against the player
			int _calcEnemyDmg =  CalculateDamageFF(_statAttack : _currentEnemyType._atkValue, _statLevel : _currentEnemyType._level, _targetDef : _statDefense, _targetLevel : _playerLevel);
			int _enemyHit = Random.Range(1,101);
			
			//---------------------------------------------------------------------------------------------------------------//
			// wait for the time the enemy's attack animation takes to 'hit' the player, to sync up effects
			float _enemyAttackTiming = _currentEnemyType._attackTiming;
			yield return new WaitForSeconds( _enemyAttackTiming );
			
			//---------------------------------------------------------------------------------------------------------------//
			// deals the damage to the player's health, if the enemy managed to hit
			if(_enemyHit > _statEvasion && _facingEnemy)
			{	
				_statHpCurrent = Mathf.Max ( _statHpCurrent - _calcEnemyDmg , 0);
				// animation - display the 'being hit' animation for the player character
				_playerAnim.SetBool("Damaged",true);
			}	
			else
			{
				Debug.Log("Enemy missed! Roll = " + _enemyHit + ", Player Evasion = "+_statEvasion);
				_variant = "miss";
			}
			
			//---------------------------------------------------------------------------------------------------------------//
			// sends the text to the screen to display the results of the calculations
			_hudControl.CreateFloatingText(
				_text :  _variant == "damage" ? _calcEnemyDmg.ToString() : "miss!", 
				_variant : _variant, 
				_target : "player" );
			
			// sends the text to the event log
			_hudControl.PushEventLog( _variant == "damage" ? _currentEnemyType.GenerateRandomBattleQuote(_damage : _calcEnemyDmg) : _currentEnemyType._missQuote );
			
			Debug.Log("Player Max HP: "+_statHpMax + ", Current HP: " + _statHpCurrent);
			_hudControl.SetPlayerHpFill( _statHpMax, _statHpCurrent);
			
			if(_statHpCurrent <= 0)
			{
				_inCombat = false;
				break;
			}
			
			// wait for the next turn - standard time between turns, minus the time 'wasted' by the enemy's attack timing
			yield return new WaitForSeconds(_TIMEBETWEENTURNS - _enemyAttackTiming);
		}
		
		yield return new WaitForSeconds(1.0f);
		
		///---------------------------------------------------------------------------------------------------------------//
		///--------------------------- CLEANUP -----------------------------------------------------------------------//
		///---------------------------------------------------------------------------------------------------------------//
		
		// victory over normal enemy!
		if (_currentEnemyHealth <= 0 && _facingEnemy && !_bossCombat)
		{
			ResetPlayerAnimation();
			
			Debug.Log("Obtained "+ _currentEnemyType._expValue +" experience.");
			_hudControl.CreateFloatingText( GetString(_gameplayTextTable,"EXP_OBTAINED").Replace("$exp",_currentEnemyType._expValue.ToString() ), "info", "info");
			_hudControl.PushEventLog( GetString(_gameplayTextTable,"EXP_OBTAINED_LOG").Replace("$exp",_currentEnemyType._expValue.ToString() ) );
			if(_playerLevel < _AREALEVELCAPS[_currentArea - 1])
				_experience += _currentEnemyType._expValue;
			UpdatePlayerExp();
			
			_hudControl.ToggleEnemyData(false);
			
			_statHpCurrent = _statHpMax;
			
			LootRoll();

			_facingEnemy = false;
		} 
		// victory over boss!
		else if (_currentEnemyHealth <= 0 && _facingEnemy && _bossCombat)
		{
			// make sure nothing else can happen while we're waiting for the final blow
			ResetPlayerAnimation();
			_isDuringFinalBlow = true;
			_areaChangeQueued = false;
			_equipChangeQueued = false;

			_hudControl.ToggleFinalBlowButton(true);
		}
		// defeat...
		else if (_statHpCurrent <= 0)
		{
			Debug.Log("Player defeated...");
			string _defeatedStr = GetString(_gameplayTextTable, "EVENT_DEFEAT");
			_hudControl.CreateFloatingText(_defeatedStr, "info", "info");
			_hudControl.PushEventLog(_defeatedStr);
			_fainted = true;
			
			// plays the "player defeated" animation
			_playerAnim.SetBool("Defeat",true);
			
			Destroy(_currentEnemyInstance);
			_hudControl.ToggleEnemyData(false);
			
			_statHpCurrent = _statHpMax;

			_facingEnemy = false;
		}
		
		// if it's not boss combat, perform end-of-combat cleanup; on a boss fight we do this after the 'final blow' button
		if(!_bossCombat) {
			if (_autoSave)
				SaveGame ();
			
			// failsafe to remove enemies that don't have animations yet or somehow got stuck on the screen
			if (_currentEnemyInstance != null) Destroy(_currentEnemyInstance);
		}
	}
	
	// standard Exvius damage calculation
	// (atk² * modifier) / defense
	// minimum damage: 1
	private int CalculateDamageExv (int _statAttack, int _targetDef, float _modifier = 1f)
	{
		int _calc = (int) Mathf.Floor( ( Mathf.Pow(_statAttack, 2) * _modifier / Mathf.Max( _targetDef, 1) ) );
		return Mathf.Max( _calc, 1);
	}
	
	// damage calculation loosely based on the FFVII formula
	private int CalculateDamageFF(int _statAttack, int _statLevel, int _targetDef, int _targetLevel, float _variance = 0.2f, float _modifier = 1f)
	{
		// step 1: base damage calculation
		float _baseCalc = ((_statAttack +  ( (_statAttack * _statLevel) / 4 ) ) * _modifier) * 8;
		// step 2: variance calculation
		_baseCalc = _baseCalc * Random.Range(1f - _variance, 1f + _variance);
		// step 3: defense mitigation
		_baseCalc = _baseCalc / (_targetDef + (Mathf.Clamp(_targetLevel - _statLevel, -5, 5) * 0.1f) * _targetDef);
		
		return (int) Mathf.Max( _baseCalc, 1);
	}
	
	// Get the appropriate localized string from the given table
	private string GetString(StringTable _table, string _entryName)
	{
		// Get the table entry. The entry contains the localized string and Metadata
		var _entry = _table.GetEntry(_entryName);
		return _entry.GetLocalizedString();  // We can pass in optional arguments for Smart Format or String.Format here
	}

	// define whether we are currently in a cutscene or not
	public void SetInCutscene(bool val){
		this._inCutscene = val;
	}

	// --------------------- debug block ----------------------------
	public void SetStartingLevel(float level) {
		if(_debugMode)
			_startingLevel = (int)level;
	}

	public void SetStartingArea(float area) {
		if(_debugMode)
			_startingArea = (int)area;
	}

	// sets whether to start a new game with all gear
	public void SetUnlockAll(bool val) {
		if(_debugMode)
			_unlockAllGear = val;
	}

	// sets whether to skip the intro cinematic on debug
	public void SetSkipIntro(bool val) {
		if(_debugMode)
			_skipIntro = val;
	}

	// adds every single item to the inventory for debug purposes
	public void AddAllGearToInventory() {
		Debug.Log("Adding all possible gear to inventory");
		_weaponList = new List<Weapon>(_armory._WEAPONLIST);
		_headgearList = new List<Headgear>(_armory._HEADGEARLIST);
		_armorList = new List<Armor>(_armory._ARMORLIST);
		_bootsList = new List<Boots>(_armory._BOOTSLIST);
		_accList = new List<Accessory>(_armory._ACCLIST);
		_skillList =new List<Skill>( _armory._SKILLLIST);
	}
}
