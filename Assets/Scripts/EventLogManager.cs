using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// manages the events that are pushed to the event log
public class EventLogManager : MonoBehaviour
{
	public TMP_Text _logElement;

	private List<string> _eventLog = new List<string>();
	private string _formattedLog = "";

	private const int _MAXLINES = 10;

	/* --------------------------------------------------------------------- */

	public void PushEvent (string _eventString)
	{
		_eventLog.Add(_eventString);

		if (_eventLog.Count >= _MAXLINES)
			_eventLog.RemoveAt(0);

		_formattedLog = "<color=#dddddd>";

		for (int i = 0; i < _eventLog.Count - 1; i++)
		{
			_formattedLog += _eventLog[i];
			_formattedLog += "\n";
		}
		
		_formattedLog += "</color><color=#ffffff>";
		_formattedLog += _eventLog[_eventLog.Count - 1];
		_formattedLog += "\n</color>";
		
		_logElement.text = _formattedLog;
	}
}
