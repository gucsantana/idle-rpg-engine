using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

/// control class that handles the creation of every item type in the game
public class ArmoryManager : MonoBehaviour
{
	// weapons: id, name, atk, def, icon, description
	public Weapon[]		_WEAPONLIST;
	public Headgear[] 		_HEADGEARLIST;
	public Armor[] 			_ARMORLIST;
	public Boots[] 			_BOOTSLIST;
	public Accessory[] 		_ACCLIST;
	public Skill[] 			_SKILLLIST;
	
	public bool _finishedLoad = false;
	
	private string _armoryStringDatabase = "ArmoryText";
	private StringTable _armoryTable;
	
	// -------------------------------------------------
	
	public void Awake()
	{
		StartCoroutine(BuildDatabase());
	}
	
	IEnumerator BuildDatabase ()
	{
		// wait until the localization system is finished loading
		var _loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(_armoryStringDatabase);
		yield return _loadingOperation;
		
		// throw log error on failure to load
		if (_loadingOperation.Status == AsyncOperationStatus.Succeeded)
			_armoryTable = _loadingOperation.Result;
		else
			Debug.LogError("Could not load String Table\n" + _loadingOperation.OperationException.ToString());
		
		_WEAPONLIST = new Weapon[] {
			// new Weapon ( _id : 0, _name : GetString("WEAPON_NAME0"), _atkValue : 0, _defValue :  0, _critValue : 0, _icon : "Items/remove_equipment_icon", _description : GetString("WEAPON_DESC0") ),	// remove item
			new Weapon ( _id : 1, _name : GetString("WEAPON_NAME1"), _atkValue : 10, _defValue :  0, _critValue : 0, _icon : "Items/w1_bronze_blade", _description : GetString("WEAPON_DESC1") ),	// bronze blade
			new Weapon ( _id : 2, _name : GetString("WEAPON_NAME2"), _atkValue : 18, _defValue :  0, _critValue : 0, _icon : "Items/w2_nomadic_sabre", _description : GetString("WEAPON_DESC2") ),	// nomadic sabre
			new Weapon ( _id : 3, _name : GetString("WEAPON_NAME3"), _atkValue : 25, _defValue :  0, _critValue : 0, _icon : "Items/w3_shamshir", _description : GetString("WEAPON_DESC3") )		// shamshir
		};
		
		_HEADGEARLIST = new Headgear[] {
			// new Headgear ( _id : 0, _name : GetString("HEADG_NAME0"), 	_atkValue : 0, _defValue :  0, _evaValue : 0, _critValue : 0, _icon : "Items/remove_equipment_icon", _description : GetString("HEADG_DESC0") ),	// remove item
			new Headgear ( _id : 1, _name : GetString("HEADG_NAME1"), 	_atkValue : 0, _defValue :  1, _evaValue : 0, _critValue : 0, _icon : "Items/h1_food_tin", _description : GetString("HEADG_DESC1") ),	// food tin
			new Headgear ( _id : 2, _name : GetString("HEADG_NAME2"), 	_atkValue : 0, _defValue :  3, _evaValue : 0, _critValue : 0, _icon : "Items/h2_barbut", _description : GetString("HEADG_DESC2")),		// barbut
			new Headgear ( _id : 3, _name : GetString("HEADG_NAME3"), 	_atkValue : 0, _defValue :  4, _evaValue : 1, _critValue : 0, _icon : "Items/h3_sallet", _description : GetString("HEADG_DESC3") )		// sallet
		};
		
		_ARMORLIST = new Armor[] {
			// new Armor ( _id : 0, _name : GetString("ARMOR_NAME1"), 	_atkValue : 0, _defValue :  0, _evaValue : 0, _icon : "Items/ar1_leather_cuirass",	 _description : GetString("ARMOR_DESC0") ),		// remove item
			new Armor ( _id : 1, _name : GetString("ARMOR_NAME1"), 	_atkValue : 0, _defValue :  3, _evaValue : 0, _icon : "Items/ar1_leather_cuirass",	 _description : GetString("ARMOR_DESC1") ),		// leather cuirass
			new Armor ( _id : 2, _name : GetString("ARMOR_NAME2"), 	_atkValue : 0, _defValue :  6, _evaValue : 0, _icon : "Items/ar2_brigandine",		 _description : GetString("ARMOR_DESC2") ),		// brigandine
			new Armor ( _id : 3, _name : GetString("ARMOR_NAME3"), 	_atkValue : 0, _defValue :  10, _evaValue : 0, _icon : "Items/ar3_plate_armor", 	 _description : GetString("ARMOR_DESC3") )		// plate armor
		};
		
		_BOOTSLIST = new Boots[] {
			new Boots ( _id : 1, _name : GetString("BOOTS_NAME1"), 	_atkValue : 0, _defValue :  0, _evaValue : 5, _icon : "Items/b1_padded_boots", _description : GetString("BOOTS_DESC1") ),		// padded boots
			new Boots ( _id : 2, _name : GetString("BOOTS_NAME2"), 	_atkValue : 0, _defValue :  1, _evaValue : 3, _icon : "Items/b2_steel_toe_boots", _description : GetString("BOOTS_DESC2") ),	// steel-toe boots
			new Boots ( _id : 3, _name : GetString("BOOTS_NAME3"), 	_atkValue : 1, _defValue :  4, _evaValue : 0, _icon : "Items/b3_cobalt_greaves", _description : GetString("BOOTS_DESC3") )		// cobalt greaves
		};
		
		_ACCLIST = new Accessory[] {
			new Accessory ( _id : 1, _name : GetString("ACC_NAME1"), 	_atkValue : 0, _defValue :  0, _evaValue : 0, _critValue : 0, _icon : "Items/ac1_peace_necklace", _description : GetString("ACC_DESC1") ),	// peace necklace
			new Accessory ( _id : 2, _name : GetString("ACC_NAME2"), 	_atkValue : 0, _defValue :  0, _evaValue : 4, _critValue : 0, _icon : "Items/ac2_ring_of_agility", _description : GetString("ACC_DESC2") ),	// ring of agility
			new Accessory ( _id : 3, _name : GetString("ACC_NAME3"), 	_atkValue : 2, _defValue :  0, _evaValue : 0, _critValue : 3, _icon : "Items/ac3_heros_bandanna", _description : GetString("ACC_DESC3") )	// hero's bandanna
		};
		
		_SKILLLIST = new Skill[] {
			new Skill ( _id : 1, _name : GetString("SKILL_NAME1"), 		_effect : "damage", _cooldown :  5, _multiplier : 1.8f, _icon : "Items/s1_triple_stab", _description : GetString("SKILL_DESC1") ),				// TRIPLE STAB
			new Skill ( _id : 2, _name : GetString("SKILL_NAME2"), 		_effect : "heal", _cooldown :  4, _multiplier : 2.5f, _icon : "Items/s2_mend_wounds", _description : GetString("SKILL_DESC2") ),				// MEND WOUNDS
			new Skill ( _id : 3, _name : GetString("SKILL_NAME3"), 		_effect : "flying-mastery", _cooldown :  0, _multiplier : 1.4f, _icon : "Items/s3_elemental_killer", _description : GetString("SKILL_DESC3") ),	// ELEMENTAL KILLER
			new Skill ( _id : 4, _name : GetString("SKILL_NAME4"), 		_effect : "magdamage", _cooldown :  6, _multiplier : 4f, _icon : "Items/s4_thundershock", _description : GetString("SKILL_DESC4") ),			// THUNDERSHOCK
			new Skill ( _id : 5, _name : GetString("SKILL_NAME5"), 		_effect : "damage", _cooldown :  8, _multiplier : 3.2f, _icon : "Items/s5_catastrophe", _description : GetString("SKILL_DESC5") )			// CATASTROPHE
		};
		
		_finishedLoad = true;	// signals that the item database has finished being built
	}
	
	// ------------------------------------------------
	
	/// Get the appropriate localized string from the given table
	private string GetString(string _entryName)
        {
		// Get the table entry. The entry contains the localized string and Metadata
		var _entry = _armoryTable.GetEntry(_entryName);
		return _entry.GetLocalizedString();  // We can pass in optional arguments for Smart Format or String.Format here
        }
}
