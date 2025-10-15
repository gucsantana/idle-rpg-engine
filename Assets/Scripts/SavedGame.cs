using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGame
{
	[Header("Player Equipped Gear")]
	public Weapon _equipWeapon;
	public Headgear _equipHead;
	public Armor _equipArmor;
	public Boots _equipBoots;
	public Accessory _equipAcc;
	public Skill _equipSkill;
	
	[Header("Gear Inventory")]
	public List<Weapon> _weaponList;
	public List<Headgear> _headgearList;
	public List<Armor> _armorList;
	public List<Boots> _bootsList;
	public List<Accessory> _accList;
	public List<Skill> _skillList;
	
	/// --- player stat values ---
	[Header("Player Stat Values")]
	public int _playerLevel;
	
	public int _statStrength;
	public int _statIntelligence;
	public int _statAgility;
	public int _statStamina;
	
	public int _experience;
	
	public List<LootTableElement> _lootBox = new List<LootTableElement>();
	
	public int _currentArea;
	
	public bool _tacticianMode = false;
}
