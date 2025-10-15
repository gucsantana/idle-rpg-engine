using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// instance that defines a usable skill that can be equipped
[System.Serializable]
public class Skill
{
	public int _id;
	public string _name;
	public string _effect;
	public int _cooldown;
	public float _multiplier;
	
	public string _icon;
	public string _description;
	
	public Skill (int _id, string _name, string _effect, int _cooldown, float _multiplier, string _icon, string _description)
	{
		this._id = _id;
		this._name = _name;
		this._effect = _effect;
		this._cooldown = _cooldown;
		this._multiplier = _multiplier;
		this._icon = _icon;
		this._description = _description;
	}

	public Skill ()
	{
		this._id = 0;
		this._name = "";
		this._effect = "";
		this._cooldown = 0;
		this._multiplier = 0f;
		this._icon = "";
		this._description = "";
	}
}
