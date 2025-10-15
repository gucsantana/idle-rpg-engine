using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

/// defines a type of enemy that may be encountered
public class EnemyType
{
	public int _id;
	public string _name;
	public int _level;
	public int _hpValue;
	public int _atkValue;
	public int  _defValue;
	public int _evaValue;
	public int _expValue;
	
	public int _battleQuoteAmt;
	public string _encounterQuote;
	public string[] _battleQuote;
	public string _missQuote;
	
	public string _tag;
	public bool _boss;
	public string _specialId;
	
	public string _modelName;			// filename for the prefab character
	public float _modelHeight;			// height of the character model, for placing attack effects on
	public float _attackTiming;			// how long the enemy's standard attack takes to 'hit'
	
	public List<LootTableElement> _lootTable = new List<LootTableElement>();
	
	public EnemyType (int _id, int _level, int _hpValue, int _atkValue, int _defValue, int _evaValue, int _expValue, bool _boss, string _tag, string _modelName, int _battleQuoteAmt, ref StringTable _table,  float _modelHeight, float _attackTiming, string _specialId = "")
	{
		this._id = _id;
		this._name = GetLocalizedString(_table, "ENEMY_NAME_"+_id);
		this._level = _level;
		this._hpValue = _hpValue;
		this._atkValue = _atkValue;
		this._defValue = _defValue;
		this._evaValue = _evaValue;
		this._expValue = _expValue;
		this._tag = _tag;
		this._boss = _boss;
		this._specialId = _specialId;
		this._modelName = _modelName;
		this._modelHeight = _modelHeight;
		this._attackTiming = _attackTiming;
		
		// combat quotes
		this._encounterQuote = GetLocalizedString(_table, "ENCOUNTER_" + _id);
		string[] _battleQuoteList = new string[_battleQuoteAmt];
		for(int i = 0; i < _battleQuoteAmt; i++)
			_battleQuoteList[i] = GetLocalizedString(_table, "ATKHIT_" + _id + "_" + (i) );
		this._battleQuote = _battleQuoteList;
		this._missQuote = GetLocalizedString(_table, "ATKMISS_" + _id);
	}
	
	public LootTableElement GenerateLootRoll ()
	{
		int _lootRoll = Random.Range(1, 1001);
		Debug.Log("Loot roll: "+_lootRoll);
		
		foreach(LootTableElement _element in _lootTable)
		{
			if(_lootRoll <= _element._dropChance)
				return _element;
		}
	
		return null;
	}
	
	public string GenerateRandomBattleQuote (int _damage)
	{
		int _roll = Random.Range(0, _battleQuote.Length);
		
		return _battleQuote[_roll].Replace ("$dmg", _damage.ToString() );
	}
	
	/// Get the appropriate localized string from the given table
	private string GetLocalizedString(StringTable _table, string _entryName)
	{
		// Get the table entry. The entry contains the localized string and Metadata
		var _entry = _table.GetEntry(_entryName);
		return _entry.GetLocalizedString();  // We can pass in optional arguments for Smart Format or String.Format here
	}
}
