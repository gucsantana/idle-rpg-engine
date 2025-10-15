using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// instance that defines a weapon that can be equipped
[System.Serializable]
public class Weapon
{
	public int _id;
	public string _name;
	public int _atkValue;
	public int  _defValue;
	public int  _critValue;
	
	public string _icon;
	public string _description;
	
	public Weapon (int _id, string _name, int _atkValue, int _defValue, int _critValue, string _icon, string _description)
	{
		this._id = _id;
		this._name = _name;
		this._atkValue = _atkValue;
		this._defValue = _defValue;
		this._critValue = _critValue;
		this._icon = _icon;
		this._description = _description;
	}

	public Weapon ()
	{
		this._id = 0;
		this._name = "";
		this._atkValue = 0;
		this._defValue = 0;
		this._critValue = 0;
		this._icon = "";
		this._description = "";
	}
}
