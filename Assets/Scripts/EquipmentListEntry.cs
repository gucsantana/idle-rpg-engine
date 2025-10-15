using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentListEntry : MonoBehaviour
{
	public string _itemType;
	
	public Weapon _weapon;
	public Headgear _headgear;
	public Armor _armor;
	public Boots _boots;
	public Accessory _accessory;
	public Skill _skill;

	public bool _removal = false;
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	// calls the hud controller to show the tooltip to the currently hovered item
	public void CallTooltip()
	{
		// no tooltip is shown for Removal items
		if(_removal)
			return;

		// Debug.Log("EquipmentListEntry.CallTooltip() - _itemType = "+_itemType);
		switch(_itemType)
		{
			case "weapon":
				HUDController.Instance.ShowItemTooltip(_itemType, _weapon : _weapon);
				break;
			case "headgear":
				HUDController.Instance.ShowItemTooltip(_itemType, _headgear : _headgear);
				break;
			case "armor":
				HUDController.Instance.ShowItemTooltip(_itemType, _armor : _armor);
				break;
			case "boots":
				HUDController.Instance.ShowItemTooltip(_itemType, _boots : _boots);
				break;
			case "skill":
				HUDController.Instance.ShowItemTooltip(_itemType, _skill : _skill);
				break;
			case "accessory":
				HUDController.Instance.ShowItemTooltip(_itemType, _accessory : _accessory);
				break;
		}
	}
	
	// calls the hud controller to hide the tooltip
	public void HideTooltip()
	{
		// no tooltip is shown for Removal items
		if(_removal)
			return;

		HUDController.Instance.HideItemTooltip();
	}
	
	// calls the game manager to equip the item in this instance
	public void EquipItem ()
	{
		switch(_itemType)
		{
			case "weapon":
				GameManagerScript.Instance.TryEquipItem(_itemType, _weapon : _weapon);
				break;
			case "headgear":
				GameManagerScript.Instance.TryEquipItem(_itemType, _headgear : _headgear);
				break;
			case "armor":
				GameManagerScript.Instance.TryEquipItem(_itemType, _armor : _armor);
				break;
			case "boots":
				GameManagerScript.Instance.TryEquipItem(_itemType, _boots : _boots);
				break;
			case "skill":
				GameManagerScript.Instance.TryEquipItem(_itemType, _skill : _skill);
				break;
			case "accessory":
				GameManagerScript.Instance.TryEquipItem(_itemType, _accessory : _accessory);
				break;
		}
	}
}
