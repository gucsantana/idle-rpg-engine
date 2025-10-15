using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boots
{
	public int _id;
	public string _name;
	public int _atkValue;
	public int  _defValue;
	public int _evaValue;
	
	public string _icon;
	public string _description;
	
	public Boots (int _id, string _name, int _atkValue, int _defValue, int _evaValue, string _icon, string _description)
	{
		this._id = _id;
		this._name = _name;
		this._atkValue = _atkValue;
		this._defValue = _defValue;
		this._evaValue = _evaValue;
		this._icon = _icon;
		this._description = _description;
	}

	public Boots ()
	{
		this._id = 0;
		this._name = "";
		this._atkValue = 0;
		this._defValue = 0;
		this._evaValue = 0;
		this._icon = "";
		this._description = "";
	}
}
