using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// controls the item information tooltip
public class Tooltip : MonoBehaviour
{
	/// --- Singleton definitions ---
	private static Tooltip _instance;
	public static Tooltip Instance { get { return _instance; } }
	
	public bool _isActive = false;
	
	public Text _itemName;
	public Text _itemCategory;
	public Text _itemAtt;
	public Text _itemDesc;
	
	private RectTransform _rect;
	
	private const float _OFFSET = 15f;
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
		
		_rect = this.GetComponent<RectTransform>();
		
		// we need this little bit of gambiarra because the tooltip must start active
		if(!_isActive) this.gameObject.SetActive(false);	
	}
	
	private void Update()
	{
		if(_isActive)
		{
			transform.position = new Vector2(Input.mousePosition.x + _OFFSET, Input.mousePosition.y - _OFFSET);
		}
	}
	
	// ---------------------------------------------------------------------------------------------------------------------------------------- //
	
	public void TurnOn ()
	{
		transform.position = new Vector2(Input.mousePosition.x + _OFFSET, Input.mousePosition.y - _OFFSET);
		_isActive = true;
	}
	
	public void UpdateFields (string _name, string _category, string _attributes, string _description)
	{
		_itemName.text = _name;
		_itemCategory.text = _category;
		_itemAtt.text = _attributes;
		_itemDesc.text = _description;
	}
}
