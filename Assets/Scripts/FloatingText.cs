using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// controls the behavior of the generated pop-up texts
public class FloatingText : MonoBehaviour
{
	// ---- constants ----
	private const float _RUNTIME = 1f;
	private const float _MOVESPEED = 25f;
	private const float _FADESPEED = 6f;
	
	private const int _EXTRASMALLTEXTSIZE = 60;
	private const int _SMALLTEXTSIZE = 36;
	private const int _NORMALTEXTSIZE = 28;
	private const int _LARGETEXTSIZE = 24;
	
	private Color32 _WHITETEXT = new Color32(255, 255, 255, 255);
	private Color32 _YELLOWTEXT = new Color32(243, 217, 88, 255);
	private Color32 _REDTEXT = new Color32(246, 87, 71, 255);
	private Color32 _GREENTEXT = new Color32(137, 233, 109, 255);
	
	private float _currTime = 0f;
	public TMP_Text _textCmp;
	
	// ------------------------------------------------------------------------------------------------- //
	
	/// sets the text color and other parameters
	public void SetText (string _text, string _variant)
	{
		switch(_variant)
		{
			case "damage":
				_textCmp.color = _WHITETEXT;
				_textCmp.fontSize = Screen.width / _NORMALTEXTSIZE;
				break;
			case "special":
				_textCmp.color = _YELLOWTEXT;
				_textCmp.fontSize = Screen.width / _LARGETEXTSIZE;
				break;
			case "critical":
				_textCmp.color = _REDTEXT;
				_textCmp.fontSize = Screen.width / _LARGETEXTSIZE;
				break;
			case "miss":
				_textCmp.color = _WHITETEXT;
				_textCmp.fontSize = Screen.width / _SMALLTEXTSIZE;
				break;
			case "healing":
				_textCmp.color = _GREENTEXT;
				_textCmp.fontSize = Screen.width / _NORMALTEXTSIZE;
				break;
			case "info":
				_textCmp.color = _WHITETEXT;
				_textCmp.fontSize = Screen.width / _EXTRASMALLTEXTSIZE;
				break;
		}
		
		_textCmp.text = _text;
	}

	void Update()
	{
		// while on the timer, nudges the text upwards according to the constants
		_currTime += Time.deltaTime;
		
		this.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + (Time.deltaTime * _MOVESPEED));
		
		if (_currTime >= _RUNTIME && _textCmp?.color.a >= 0)
		{
			Color _color = _textCmp.color;
			_color.a = _color.a - (Time.deltaTime * _FADESPEED);
			_textCmp.color = _color;
		}	
		
		else if (_currTime >= _RUNTIME && _textCmp?.color.a <= 0)
			Destroy(this.gameObject);
	}
}
