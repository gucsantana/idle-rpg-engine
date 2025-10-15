using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

/// control class that handles the types of enemies found in the game
public class BestiaryManager : MonoBehaviour
{
	private string _bestiaryStringDatabase = "BestiaryText";
	private StringTable _bestiaryTable;
	
	private EnemyType[] _ENEMYLIST;
	
	private Dictionary<EnemyType,int> _AREA1ENEMIES = new Dictionary<EnemyType,int>();
	private Dictionary<EnemyType,int> _AREA2ENEMIES = new Dictionary<EnemyType,int>();
	private Dictionary<EnemyType,int> _AREA3ENEMIES = new Dictionary<EnemyType,int>();
	private Dictionary<EnemyType,int> _AREA4ENEMIES = new Dictionary<EnemyType,int>();
	
	private ArmoryManager _armory;
	
	/* ----------------------------------------------------- */
	
	public void Awake()
	{
		_armory = GetComponent<ArmoryManager>();
		
		StartCoroutine(BuildDatabases());
	}
	
	private IEnumerator BuildDatabases ()
	{
		// wait until the localization system is finished loading
		var _loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(_bestiaryStringDatabase);
		yield return _loadingOperation;
		
		// throw log error on failure to load
		if (_loadingOperation.Status == AsyncOperationStatus.Succeeded)
			_bestiaryTable = _loadingOperation.Result;
		else
			Debug.LogError("Could not load String Table\n" + _loadingOperation.OperationException.ToString());
		
		BuildEnemyDatabase ();
		
		BuildAreaDatabase ();
		
		// waits for the Armory to finish loading the item database before attempting to build the enemy loot tables
		yield return new WaitUntil( ( ) => _armory._finishedLoad == true);
	
		BuildEnemyLootTable();
	}
	
	private void BuildEnemyDatabase ()
	{
		_ENEMYLIST = new EnemyType[] {
			// 1: IMP
			new EnemyType ( _id : 1, _hpValue : 200, _level : 2, _atkValue : 28, _defValue :  4, _evaValue : 0, _expValue : 100, _tag : "humanoid", _modelName : "imp", _boss : false, _specialId : "", _battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 1.3f, _attackTiming : 0.2f),
			// 2: RENTAUR
			new EnemyType ( _id : 2, _hpValue : 200, _level : 3, _atkValue : 15, _defValue :  6, _evaValue : 0, _expValue : 100, _tag : "beast", _modelName : "rentaur", _boss : false, _specialId : "", _battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 2f, _attackTiming : 0.15f),
			// 3: MECHANICAL SENTRY
			new EnemyType ( _id : 3, _hpValue : 420, _level : 3, _atkValue : 30, _defValue :  9, _evaValue : 0, _expValue : 30, _tag : "machine", _modelName : "mechsentry", _boss : false, _specialId : "", _battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 1.3f, _attackTiming : 0.25f),
			// 4: HALL MONITOR
			new EnemyType ( _id : 4, _hpValue : 630, _level : 4, _atkValue : 24, _defValue :  7, _evaValue : 2, _expValue : 40, _tag : "beast", _modelName : "monitor", _boss : false, _specialId : "" ,_battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 1.8f, _attackTiming : 0.75f),
			// 5: MERMAN
			new EnemyType ( _id : 5, _hpValue : 1000, _level : 6, _atkValue : 48, _defValue :  7, _evaValue : 5, _expValue : 60, _tag : "humanoid", _modelName : "merman", _boss : false, _specialId : ""  ,_battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 1.8f, _attackTiming : 0.25f),
			// // 6: WATER ELEMENTAL
			new EnemyType ( _id : 6, _hpValue : 1450, _level : 7, _atkValue : 43, _defValue :  5, _evaValue : 10, _expValue : 80, _tag : "elemental", _modelName : "waterelem", _boss : false, _specialId : "",_battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 1.8f, _attackTiming : 0.64f),
			// // 7: THE GOD OF WISHES
			new EnemyType ( _id : 7, _hpValue : 40, _level : 10, _atkValue : 40, _defValue :  8, _evaValue : 0, _expValue : 2000, _tag : "avatar", _modelName : "god", _boss : true, _specialId : "",_battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 2f, _attackTiming : 0.5f)
			// new EnemyType ( _id : 7, _hpValue : 4000, _level : 10, _atkValue : 40, _defValue :  8, _evaValue : 0, _expValue : 2000, _tag : "avatar", _modelName : "god", _boss : true, _specialId : "",_battleQuoteAmt : 2, _table : ref _bestiaryTable, _modelHeight : 2f, _attackTiming : 0.5f)
		};
	}
	
	private void BuildEnemyLootTable ()
	{
		// ------------------------------------- DEFAULT ENEMY SET -------------------------------------
		// 0 - imp
		_ENEMYLIST[0]._lootTable.Add( new LootTableElement( _itemType : "headgear", _dropChance : 150, _rare : false, _headgear : _armory._HEADGEARLIST[0] ) );	// food tin
		_ENEMYLIST[0]._lootTable.Add( new LootTableElement( _itemType : "accessory", _dropChance : 200, _rare : true, _accessory : _armory._ACCLIST[0] ) );			// peace necklace
		_ENEMYLIST[0]._lootTable.Add( new LootTableElement( _itemType : "skill", _dropChance : 300, _rare : false, _skill : _armory._SKILLLIST[0] ) );					// triple stab
		// 1 - rentaur
		_ENEMYLIST[1]._lootTable.Add( new LootTableElement( _itemType : "boots", _dropChance : 150, _rare : false, _boots : _armory._BOOTSLIST[1] ) );				// steel-toed boots
		_ENEMYLIST[1]._lootTable.Add( new LootTableElement( _itemType : "armor", _dropChance : 200, _rare : true, _armor : _armory._ARMORLIST[1] ) );			// brigandine
		_ENEMYLIST[1]._lootTable.Add( new LootTableElement( _itemType : "skill", _dropChance : 300, _rare : false, _skill : _armory._SKILLLIST[1] ) );					// mend wounds
		// 2 - mechanical sentry
		_ENEMYLIST[2]._lootTable.Add( new LootTableElement( _itemType : "headgear", _dropChance : 120, _rare : false, _headgear : _armory._HEADGEARLIST[1] ) );	// barbut
		_ENEMYLIST[2]._lootTable.Add( new LootTableElement( _itemType : "armor", _dropChance : 160, _rare : true, _armor : _armory._ARMORLIST[2] ) );			// plate armor
		_ENEMYLIST[2]._lootTable.Add( new LootTableElement( _itemType : "skill", _dropChance : 220, _rare : false, _skill : _armory._SKILLLIST[2] ) );					// bird killer
		// 3 - hall monitor
		_ENEMYLIST[3]._lootTable.Add( new LootTableElement( _itemType : "weapon", _dropChance : 100, _rare : false, _weapon : _armory._WEAPONLIST[1] ) );		// winged spear
		_ENEMYLIST[3]._lootTable.Add( new LootTableElement( _itemType : "accessory", _dropChance : 140, _rare : true, _accessory : _armory._ACCLIST[1] ) );			// ring of agility
		_ENEMYLIST[3]._lootTable.Add( new LootTableElement( _itemType : "skill", _dropChance : 220, _rare : false, _skill : _armory._SKILLLIST[3] ) );					// thundershock
		// 4 - merman
		_ENEMYLIST[4]._lootTable.Add( new LootTableElement( _itemType : "accessory", _dropChance : 90, _rare : false, _accessory : _armory._ACCLIST[2] ) );		// hero bandanna
		_ENEMYLIST[4]._lootTable.Add( new LootTableElement( _itemType : "skill", _dropChance : 130, _rare : true, _skill : _armory._SKILLLIST[4] ) );				// catastrophe
		// 5 - water elemental
		_ENEMYLIST[5]._lootTable.Add( new LootTableElement( _itemType : "headgear", _dropChance : 80, _rare : false, _headgear : _armory._HEADGEARLIST[2] ) );	// sallet
		_ENEMYLIST[5]._lootTable.Add( new LootTableElement( _itemType : "weapon", _dropChance : 100, _rare : true, _weapon : _armory._WEAPONLIST[2] ) );		// red bolt
	}
	
	// DEFAULT VERSION
	private void BuildAreaDatabase ()
	{
		// ------ AREA 1 ------
		_AREA1ENEMIES.Add(_ENEMYLIST[0], 65);	// IMP
		_AREA1ENEMIES.Add(_ENEMYLIST[1], 100);  // RENTAUR
		// ------ AREA 2 ------
		_AREA2ENEMIES.Add(_ENEMYLIST[2], 60);	// MECHANICAL SENTRY
		_AREA2ENEMIES.Add(_ENEMYLIST[3], 100);  // HALL MONITOR
		// ------ AREA 3 ------
		_AREA3ENEMIES.Add(_ENEMYLIST[4], 99);	// MERMAN
		_AREA3ENEMIES.Add(_ENEMYLIST[5], 100);  // WATER ELEMENTAL
		// ------ AREA 4 ------
		_AREA4ENEMIES.Add(_ENEMYLIST[6], 100);  // GOD OF WISHES
	}
	
	public EnemyType GetNextEnemy (int _area)
	{
		int _rng = Random.Range(1,101);
		
		switch (_area)
		{
			case 1:
				return GetEnemyFromSet(_AREA1ENEMIES, _rng);
			case 2:
				return GetEnemyFromSet(_AREA2ENEMIES, _rng);
			case 3:
				return GetEnemyFromSet(_AREA3ENEMIES, _rng);
			case 4:
				return GetEnemyFromSet(_AREA4ENEMIES, _rng);
			default:
				break;
		}
		
		return _ENEMYLIST[0];
	}
	
	private EnemyType GetEnemyFromSet (Dictionary<EnemyType,int> _set, int _rngRoll)
	{
		foreach(KeyValuePair<EnemyType,int> _entry in _set)
		{
			if(_rngRoll <= _entry.Value)
				return _entry.Key;
		}

		// failsafe returns an Imp
		return _ENEMYLIST[0];
	}
}
