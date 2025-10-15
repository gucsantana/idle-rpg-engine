using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// defines an element on a monster's loot table
[System.Serializable]
public class LootTableElement
{
	/*
		HOW TO USE
		
		Item type is a string that defines the type of item dropped: "weapon", "armor", "headgear", "accessory", "boots" or "skill"
		Weapon/Armor/Headgear/Accessory/Boots/Skill hold items of that type, but only the type defined in "item type" matters
		Drop chance is the chance (out of 1000) that the item drops from the given monster
		Rare is a flag that identifies that an item is rare (using when representing said item on drop lists and the loot box)
		Opened is only used by the game manager later, to decide which boxes to discard from the loot box after closing the window
	*/
	
	public string _itemType;
	public int _dropChance;
	public bool _rare;
	
	public Weapon _weapon;
	public Headgear _headgear;
	public Armor _armor;
	public Boots _boots;
	public Accessory _accessory;
	public Skill _skill;
	
	public bool _opened = false;
	
	public LootTableElement (string _itemType, int _dropChance, bool _rare, Weapon _weapon = null, Headgear _headgear = null, Armor _armor = null, Boots _boots = null, Accessory _accessory = null, Skill _skill = null)
	{
		this._itemType = _itemType;
		this._dropChance = _dropChance;
		this._rare = _rare;
		
		this._weapon = _weapon;
		this._headgear = _headgear;
		this._armor = _armor;
		this._boots = _boots;
		this._accessory = _accessory;
		this._skill = _skill;
	}
}
