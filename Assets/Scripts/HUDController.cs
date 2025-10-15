using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
	/// --- Singleton definitions ---
	private static HUDController _instance;
	public static HUDController Instance { get { return _instance; } }
	
	/// --- Element References ---
	[Header("Element References")]
	public GameObject _enemyBlock;
	
	private GameManagerScript _gameManager { get { return GameManagerScript.Instance; } }
	public EventLogManager _eventLog;
	
	private Image _playerHpDamaged;
	private Image _playerHpFilled;
	public Image _enemyHpDamaged;
	public Image _enemyHpFilled;
	
	public Animator _eqChangeQueuedObj;
	public GameObject _eqChangeMarkerWpn;
	public GameObject _eqChangeMarkerHdg;
	public GameObject _eqChangeMarkerArm;
	public GameObject _eqChangeMarkerBts;
	public GameObject _eqChangeMarkerAcc;
	public GameObject _eqChangeMarkerSki;
	
	private Text _playerName;
	public Text _enemyName;
	private TMP_Text _playerLevelText;
	
	public Text _errorMessage;
	
	private Text _atkValue;
	private Text _magValue;
	private Text _evaValue;
	private Text _defValue;
	
	private Text _expValue;
	
	public GameObject _lootBoxBlock;
	public GameObject[] _normalItemBoxes = new GameObject[10];
	public GameObject[] _rareItemBoxes = new GameObject[10];
	public GameObject[] _itemBoxImages = new GameObject[10];
	private Text _lootBoxAmount;
	
	public Image _eqpWeaponIcon;
	public Image _eqpHeadIcon;
	public Image _eqpArmorIcon;
	public Image _eqpBootsIcon;
	public Image _eqpAccIcon;
	public Image _eqpSkillIcon;
	
	public GameObject _itemListView;
	
	public GameObject _prevAreaButton;
	public GameObject _nextAreaButton;
	public GameObject _finalBlowButton;
	
	/// --------- constants ---------
	const float _FILLRATEDAMAGED = 0.5f;
	const float _FILLRATEREFILL = 1.3f;
	const float _FILLRATEFAINT = (1f / 60f);
	
	const float _ERRORFADERATE = 1f;
	const float _ERRORDISPLAYTIMERMAX = 2f;
	
	const float _MESSAGETIMING = 1f;
	
	const int _ITEMENTRYSPACING = 17;
	
	/// --------- control variables ---------
	private float _errorDisplayTimer = 0f;
	private bool _errorDisplayed = false;
	
	public bool _finishedLoad = false;
	private bool _elementsLocked = false;		// when true, the player can't click on most screen elements
	// private bool _isNextAreaButtonLocked = true;
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
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
		
		if(SceneManager.GetActiveScene().name == "MainMenu")
			this.gameObject.SetActive(false);
	}
	
	void FixedUpdate()
	{
		// ticks down the "damaged" HP bar for the player and enemy if necessary, or fills them back up in case of healing
		if( _playerHpDamaged.fillAmount > _playerHpFilled.fillAmount )
			_playerHpDamaged.fillAmount = Mathf.Max(_playerHpDamaged.fillAmount - _FILLRATEDAMAGED * Time.deltaTime, _playerHpFilled.fillAmount);
		else if( _playerHpDamaged.fillAmount < _playerHpFilled.fillAmount )
			_playerHpDamaged.fillAmount = _playerHpFilled.fillAmount;

		if(_playerHpFilled.fillAmount < 1f && !_gameManager._facingEnemy )
		{
			if(!_gameManager._fainted)
				_playerHpFilled.fillAmount = Mathf.Min(_playerHpFilled.fillAmount + (_FILLRATEREFILL * Time.deltaTime), 1.0f);
			else
				_playerHpFilled.fillAmount = Mathf.Min(_playerHpFilled.fillAmount + (
					((_gameManager._equipAcc?._name == "Peace Necklace") ? _FILLRATEFAINT * 2 : _FILLRATEFAINT)
					* Time.deltaTime), 1.0f);
		}
		
		if( _enemyHpDamaged.fillAmount > _enemyHpFilled.fillAmount )
			_enemyHpDamaged.fillAmount = Mathf.Max(_enemyHpDamaged.fillAmount - _FILLRATEDAMAGED * Time.deltaTime, _enemyHpFilled.fillAmount);

		
		// winds down the error message and related timers
		if(_errorDisplayed && _errorDisplayTimer < _ERRORDISPLAYTIMERMAX)
			_errorDisplayTimer += Time.deltaTime;
		
		Color _textColor = _errorMessage.color;	// the text color of the error message
		
		if(_errorDisplayed && _errorDisplayTimer >= _ERRORDISPLAYTIMERMAX && _textColor.a > 0)
		{
			_textColor.a = Mathf.Max( _textColor.a - (Time.deltaTime * _ERRORFADERATE), 0 );
			_errorMessage.color = _textColor;
		}
		
		if(_errorDisplayed && _errorDisplayTimer >= _ERRORDISPLAYTIMERMAX && _textColor.a <= 0)
			_errorDisplayed = false;
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// fills the player's health bar
	public void SetPlayerHpToMax ()
	{
		_playerHpFilled.fillAmount = 1f;
		_playerHpDamaged.fillAmount = 1f;
	}
	
	// fills the enemy's health bar
	public void SetEnemyHpToMax ()
	{
		_enemyHpFilled.fillAmount = 1f;
		_enemyHpDamaged.fillAmount = 1f;
	}
	
	// sets the current fill amount for the player HP bar
	public void SetPlayerHpFill (float _maxPlayerHp, float _currentPlayerHp)
	{
		_playerHpFilled.fillAmount = (_currentPlayerHp / _maxPlayerHp);
	}
	
	// sets the current fill amount for the enemy HP bar
	public void SetEnemyHpFill (float _maxEnemyHp, float _currentEnemyHp)
	{
		
		_enemyHpFilled.fillAmount = (_currentEnemyHp / _maxEnemyHp);
	}
	
	// turns the enemy data block (name, HP, etc) on and off
	public void ToggleEnemyData (bool _toggle)
	{
		_enemyBlock.SetActive(_toggle);
	}
	
	public void SetEnemyName (string _name)
	{
		_enemyName.text = _name;
	}
	
	// displays an error message, zeroing the timer and setting the required variables
	public void DisplayErrorMessage (string _errorText)
	{
		_errorDisplayTimer = 0f;
		_errorMessage.text = _errorText;
		
		Color _color = _errorMessage.color;
		_color.a = 1f;
		_errorMessage.color = _color;
		
		_errorDisplayed = true;
	}
	
	public void TogglePrevAreaButton(bool toggle)
	{
		_prevAreaButton.SetActive(toggle);
	}
	
	public void ToggleNextAreaButton(bool toggle)
	{
		_nextAreaButton.SetActive(toggle);
	}

	// sets next area button to open or close (true is open); only has effect if open on locked, or close on unlocked
	public void UnlockNextAreaButton(bool toggle)
	{
		Debug.Log("Unlock next area button: "+toggle);
		_nextAreaButton.GetComponent<Button>().interactable = toggle;
		// if(_isNextAreaButtonLocked && toggle){
		// 	_nextAreaButton.GetComponent<Animator>().SetBool("OpenAnim",true);
		// 	_isNextAreaButtonLocked = false;
		// }
		// else if(!_isNextAreaButtonLocked && !toggle){
		// 	_nextAreaButton.GetComponent<Animator>().SetBool("Lock",true);
		// 	_isNextAreaButtonLocked = true;
		// }
	}

	// when a boss is defeated, close all windows, lock controls and turn on the final blow button
	public void ToggleFinalBlowButton(bool toggle)
	{
		_elementsLocked = toggle;
		_finalBlowButton.SetActive(toggle);
		if(toggle){
			CloseItemList();
			CloseLootBox();
		}
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	public void SetPlayerLevel (int _level)
	{
		_playerLevelText.text = "Level " + _level;
	}
	
	public void SetPlayerStats (int _atkVal, int _magVal, int _evaVal, int _defVal)
	{
		_atkValue.text = _atkVal.ToString();
		_magValue.text = _magVal.ToString();
		_evaValue.text = _evaVal.ToString();
		_defValue.text = _defVal.ToString();
	}
	
	public void SetPlayerExp (int _expCurrent, int _expNext)
	{
		_expValue.text = _expCurrent + "/" + _expNext;
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	public void UpdateLootButtonText (int _lootAmt)
	{
		_lootBoxAmount.text = "Loot Drops ("+ _lootAmt + ")";
	}
	
	/// opens the window that contains the obtained and unopened item chests
	public void OpenLootBox ()
	{
		if(_elementsLocked)		return;		// if the elements are locked (i.e. during final blow)

		CloseItemList(); 	// to avoid overlapping windows
		
		for(int i = 0; i < _gameManager._lootBox.Count; i++)
		{
			if( _gameManager._lootBox[i]._rare)
				_rareItemBoxes[i].SetActive(true);
			else
				_normalItemBoxes[i].SetActive(true);
		}
		
		_lootBoxBlock.SetActive(true);
		
		Time.timeScale = 0f;
	}
	
	/// closes the loot box
	public void CloseLootBox ()
	{
		_gameManager.ClearOpenedBoxes();
		
		_lootBoxBlock.SetActive(false);
		
		foreach(GameObject _obj in _normalItemBoxes)		_obj.SetActive(false);
		foreach(GameObject _obj in _rareItemBoxes)		_obj.SetActive(false);
		foreach(GameObject _obj in _itemBoxImages)		_obj.SetActive(false);
		
		UpdateLootButtonText(_gameManager._lootBox.Count);
		
		Time.timeScale = 1f;
	}
	
	/// sends control over to the game manager to open a loot chest and collect the item within
	public void UnpackItem(int _index)
	{
		_gameManager.UnpackItem( _index );
		
		_normalItemBoxes[_index].SetActive(false);
		_rareItemBoxes[_index].SetActive(false);
	}
	
	public void DisplayItemImage(int _index, string _icon)
	{
		Sprite _itemIcon = Resources.Load<Sprite>(_icon);
		_itemBoxImages[_index].GetComponent<Image>().sprite = _itemIcon;
		
		_itemBoxImages[_index].SetActive(true);
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// changes the icon on a given item category to the passed parameter
	public void ChangeEquippedItemIcon (string _category, string _icon)
	{
		switch(_category)
		{
			case "weapon":
				_eqpWeaponIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
			case "headgear":
				_eqpHeadIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
			case "armor":
				_eqpArmorIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
			case "boots":
				_eqpBootsIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
			case "accessory":
				_eqpAccIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
			case "skill":
				_eqpSkillIcon.sprite = Resources.Load<Sprite>(_icon);
				break;
		}
	}
	
	// builds the list of equipment
	public void ShowItemList (string _category)
	{
		if(_elementsLocked)		return;		// if the elements are locked (i.e. during final blow)

		CloseItemList();	// to avoid overlapping elements
		CloseLootBox();
		
		int _offset = 0;

		Button _itemEntryPrefab = Resources.Load<Button>("HUD Prefabs/Equipment List Entry");
		
		Button _removeEqEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
		_removeEqEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
		_offset += _ITEMENTRYSPACING;

		RectTransform _contentHolder = _itemListView.transform.Find("Viewport/Content").GetComponent<RectTransform>();
		
		switch(_category)
		{
			case "weapon":
				_contentHolder.sizeDelta = new Vector2(70, (_gameManager._weaponList.Count + 1) * _ITEMENTRYSPACING);
				
				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Weapon";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "weapon";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._weapon._icon = "HUD/placeholder_spear_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._weapon._name = "remove";

				foreach(Weapon _weapon in _gameManager._weaponList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _weapon._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_weapon._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "weapon";
					_newEntry.GetComponent<EquipmentListEntry>()._weapon = _weapon;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
			case "headgear":
				_contentHolder.sizeDelta = new Vector2(70, _gameManager._headgearList.Count * _ITEMENTRYSPACING);
				
				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Headgear";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "headgear";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._headgear._icon = "HUD/placeholder_head_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._headgear._name = "remove";

				foreach(Headgear _headgear in _gameManager._headgearList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _headgear._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_headgear._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "headgear";
					_newEntry.GetComponent<EquipmentListEntry>()._headgear = _headgear;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
			case "armor":
				_contentHolder.sizeDelta = new Vector2(70, _gameManager._armorList.Count * _ITEMENTRYSPACING);

				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Armor";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "armor";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._armor._icon = "HUD/placeholder_armor_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._armor._name = "remove";
				
				foreach(Armor _armor in _gameManager._armorList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _armor._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_armor._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "armor";
					_newEntry.GetComponent<EquipmentListEntry>()._armor = _armor;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
			case "boots":
				_contentHolder.sizeDelta = new Vector2(70, _gameManager._bootsList.Count * _ITEMENTRYSPACING);
				
				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Boots";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "boots";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._boots._icon = "HUD/placeholder_boots_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._boots._name = "remove";
				
				foreach(Boots _boots in _gameManager._bootsList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _boots._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_boots._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "boots";
					_newEntry.GetComponent<EquipmentListEntry>()._boots = _boots;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
			case "skill":
				_contentHolder.sizeDelta = new Vector2(70, _gameManager._skillList.Count * _ITEMENTRYSPACING);
				
				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Skill";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "skill";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._skill._icon = "HUD/placeholder_skill_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._skill._name = "remove";
				
				foreach(Skill _skill in _gameManager._skillList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _skill._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_skill._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "skill";
					_newEntry.GetComponent<EquipmentListEntry>()._skill = _skill;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
			case "accessory":
				_contentHolder.sizeDelta = new Vector2(70, _gameManager._accList.Count * _ITEMENTRYSPACING);
				
				_removeEqEntry.transform.Find("Name").GetComponent<Text>().text = "Remove Accessory";
				_removeEqEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("Items/remove_equipment_icon");
				
				_removeEqEntry.GetComponent<EquipmentListEntry>()._itemType = "accessory";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._removal = true;
				_removeEqEntry.GetComponent<EquipmentListEntry>()._accessory._icon = "HUD/placeholder_acc_new";
				_removeEqEntry.GetComponent<EquipmentListEntry>()._accessory._name = "remove";
				
				foreach(Accessory _accessory in _gameManager._accList)
				{
					Button _newEntry = Instantiate(_itemEntryPrefab, new Vector2(0, - _offset), _itemListView.transform.rotation);
					_newEntry.transform.SetParent( _itemListView.transform.Find("Viewport/Content"), worldPositionStays : false );
					
					_newEntry.transform.Find("Name").GetComponent<Text>().text = _accessory._name;
					_newEntry.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(_accessory._icon);
					
					_newEntry.GetComponent<EquipmentListEntry>()._itemType = "accessory";
					_newEntry.GetComponent<EquipmentListEntry>()._accessory = _accessory;
					
					_offset += _ITEMENTRYSPACING;
				}
				break;
		}
		
		_itemListView.SetActive(true);
	}
	
	// purges the current item list and closes the window
	public void CloseItemList ()
	{
		foreach(Transform _child in _itemListView.transform.Find("Viewport/Content"))
			Destroy(_child.gameObject);
		
		_itemListView.SetActive(false);
	}
	
	// generic call to show the item tooltip, passing the required parameters
	public void ShowItemTooltip(string _category, Weapon _weapon = null, Headgear _headgear = null, Armor _armor = null, Boots _boots = null, Accessory _accessory = null, Skill _skill = null)
	{
		string _name = ""; 
		string _attributes = "";
		string _description = "";
		
		// no need to display a tooltip for no item
		if(_weapon == null && _headgear == null && _armor == null && _boots == null && _accessory == null && _skill == null)
		{ return; }
		
		switch(_category)
		{
			case "weapon":
				_name = _weapon._name;
				if(_weapon._atkValue > 0)  _attributes += "ATK : +"+ _weapon._atkValue + ", ";
				if(_weapon._defValue > 0)  _attributes += "DEF : +"+ _weapon._defValue + ", ";
				if(_weapon._critValue > 0)  _attributes += "CRIT : +"+ _weapon._critValue + "%"; 
					else if(_attributes.Length >= 2) _attributes = _attributes.Remove( _attributes.Length - 2);
				_description = _weapon._description;
				break;
			case "headgear":
				_name = _headgear._name;
				if(_headgear._atkValue > 0)  _attributes += "ATK : +"+ _headgear._atkValue + ", ";
				if(_headgear._defValue > 0)  _attributes += "DEF : +"+ _headgear._defValue + ", ";
				if(_headgear._evaValue > 0)  _attributes += "EVA : +"+ _headgear._evaValue + "%, ";
				if(_headgear._critValue > 0)  _attributes += "CRIT : +"+ _headgear._critValue + "%"; 
					else if(_attributes.Length >= 2) _attributes = _attributes.Remove( _attributes.Length - 2);
				_description = _headgear._description;
				break;
			case "armor":
				_name = _armor._name;
				if(_armor._atkValue > 0)  _attributes += "ATK : +"+ _armor._atkValue + ", ";
				if(_armor._defValue > 0)  _attributes += "DEF : +"+ _armor._defValue + ", ";
				if(_armor._evaValue > 0)  _attributes += "EVA : +"+ _armor._evaValue + "% ";
					else if(_attributes.Length >= 2) _attributes = _attributes.Remove( _attributes.Length - 2);
				_description = _armor._description;
				break;
			case "boots":
				_name = _boots._name;
				if(_boots._atkValue > 0)  _attributes += "ATK : +"+ _boots._atkValue + ", ";
				if(_boots._defValue > 0)  _attributes += "DEF : +"+ _boots._defValue + ", ";
				if(_boots._evaValue > 0)  _attributes += "EVA : +"+ _boots._evaValue + "% ";
					else if(_attributes.Length >= 2) _attributes = _attributes.Remove( _attributes.Length - 2);
				_description = _boots._description;
				break;
			case "skill":
				_name = _skill._name;
				_attributes = _skill._cooldown + " turns Cooldown";
				_description = _skill._description;
				break;
			case "accessory":
				_name = _accessory._name;
				if(_accessory._atkValue > 0)  _attributes += "ATK : +"+ _accessory._atkValue + ", ";
				if(_accessory._defValue > 0)  _attributes += "DEF : +"+ _accessory._defValue + ", ";
				if(_accessory._evaValue > 0)  _attributes += "EVA : +"+ _accessory._evaValue + "%, ";
				if(_accessory._critValue > 0)  _attributes += "CRIT : +"+ _accessory._critValue + "%"; 
					else if(_attributes.Length >= 2) _attributes = _attributes.Remove( _attributes.Length - 2);
				_description = _accessory._description;
				break;
		}
		
		Tooltip.Instance.UpdateFields(_name, _category, _attributes, _description);
		Tooltip.Instance.gameObject.SetActive(true);
		Tooltip.Instance.TurnOn();
	}
	
	// tooltip opening call from the gear equipped buttons in the gear block
	public void TooltipCallGearBlock (string _category)
	{
		switch(_category)
		{
			case "weapon":
				ShowItemTooltip(_category, _weapon : _gameManager._equipWeapon);
				break;
			case "headgear":
				ShowItemTooltip(_category, _headgear : _gameManager._equipHead);
				break;
			case "armor":
				ShowItemTooltip(_category, _armor : _gameManager._equipArmor);
				break;
			case "boots":
				ShowItemTooltip(_category, _boots : _gameManager._equipBoots);
				break;
			case "skill":
				ShowItemTooltip(_category, _skill : _gameManager._equipSkill);
				break;
			case "accessory":
				ShowItemTooltip(_category, _accessory : _gameManager._equipAcc);
				break;
		}
	}
	
	// tooltip opening call from the opened item chests in the loot drops screen
	public void TooltipCallLootBox (int _index)
	{
		string _category = _gameManager._lootBox[_index]._itemType;
		
		switch(_category)
		{
			case "weapon":
				ShowItemTooltip(_category, _weapon : _gameManager._lootBox[_index]._weapon);
				break;
			case "headgear":
				ShowItemTooltip(_category, _headgear : _gameManager._lootBox[_index]._headgear);
				break;
			case "armor":
				ShowItemTooltip(_category, _armor : _gameManager._lootBox[_index]._armor);
				break;
			case "boots":
				ShowItemTooltip(_category, _boots : _gameManager._lootBox[_index]._boots);
				break;
			case "skill":
				ShowItemTooltip(_category, _skill : _gameManager._lootBox[_index]._skill);
				break;
			case "accessory":
				ShowItemTooltip(_category, _accessory : _gameManager._lootBox[_index]._accessory);
				break;
		}
	}
	
	// hides the item tooltip
	public void HideItemTooltip()
	{
		Tooltip.Instance._isActive = false;
		Tooltip.Instance.gameObject.SetActive(false);
	}

	// turns the equipment change queue indicator on and off
	public void SetEquipQueueVisibility(bool val)
	{
		_eqChangeQueuedObj.SetBool("IsQueued",val);
		if(val == false)
			SetAllEquipChangeMarkersToFalse();
	}
	public void SetEquipChangeMarkerVisibility(bool val, string category)
	{
		switch(category){
			case "weapon":
				_eqChangeMarkerWpn.SetActive(val);
				break;
			case "headgear":
				_eqChangeMarkerHdg.SetActive(val);
				break;
			case "armor":
				_eqChangeMarkerArm.SetActive(val);
				break;
			case "boots":
				_eqChangeMarkerBts.SetActive(val);
				break;
			case "accessory":
				_eqChangeMarkerAcc.SetActive(val);
				break;
			case "skill":
				_eqChangeMarkerSki.SetActive(val);
				break;
			default:
				Debug.Log("Wrong switch entry at SetEquipChangeMarkerVisibility");
				break;
		}
	}
	private void SetAllEquipChangeMarkersToFalse()
	{
		_eqChangeMarkerWpn.SetActive(false);
		_eqChangeMarkerHdg.SetActive(false);
		_eqChangeMarkerArm.SetActive(false);
		_eqChangeMarkerBts.SetActive(false);
		_eqChangeMarkerAcc.SetActive(false);
		_eqChangeMarkerSki.SetActive(false);
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// creates an instance of floating text over either the player or enemy, according to parameters
	public void CreateFloatingText (string _text, string _variant, string _target)
	{
		GameObject _textPrefab = Resources.Load<GameObject>("HUD Prefabs/FloatingTextPrefab");
		GameObject _textPos;
		
		switch (_target)
		{
			case "player":
				_textPos = GameObject.Find("PlayerNumAnchor");
				break;
			case "enemy":
				_textPos = GameObject.Find("EnemyNumAnchor");
				break;
			case "info":
				_textPos = GameObject.Find("InfoBoxAnchor");
				break;
			default:
				_textPos = this.gameObject;
				Debug.Log("Bug on CreateFloatingText, check switch");
				break;
		}
		
		GameObject _textObj = Instantiate(_textPrefab, _textPos.transform.position, _textPos.transform.rotation);
		_textObj.transform.SetParent( parent : this.transform, worldPositionStays : true );
		
		_textObj.GetComponent<FloatingText>().SetText(_text, _variant);
	}
	
	// waits an interval before creating an instance of floating text
	public IEnumerator CreateDelayedFloatingText (string _text, string _variant, string _target)
	{
		yield return new WaitForSeconds( _MESSAGETIMING );
		CreateFloatingText (_text, _variant, _target);
	}
	
	// push a message to the event log
	public void PushEventLog (string _event)
	{
		_eventLog.PushEvent(_event);
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// set component references
	private void SetReferences ()
	{		
		_playerHpDamaged = GameObject.Find("Player Block/HP Bar Player Damaged").GetComponent<Image>();
		_playerHpFilled = GameObject.Find("Player Block/HP Bar Player Full").GetComponent<Image>();
		
		_playerName = GameObject.Find("Player Block/Player Name").GetComponent<Text>();
		_playerLevelText = GameObject.Find("Player Block/Level Text").GetComponent<TMP_Text>();
		
		_atkValue = GameObject.Find("Stats Block/'ATK' Value").GetComponent<Text>();
		_magValue = GameObject.Find("Stats Block/'MAG' Value").GetComponent<Text>();
		_evaValue = GameObject.Find("Stats Block/'EVA' Value").GetComponent<Text>();
		_defValue = GameObject.Find("Stats Block/'DEF' Value").GetComponent<Text>();
		
		_expValue = GameObject.Find("Stats Block/'EXP' Value").GetComponent<Text>();
		
		_lootBoxAmount = GameObject.Find("Loot Button/Loot Text").GetComponent<Text>();
		
		_finishedLoad = true;
	}
}
