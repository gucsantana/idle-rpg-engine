using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the logic for the main menu button features; middleman necessary to always relay info to the instance of the GM
/// </summary>
public class MainMenuHandler : MonoBehaviour
{
    private GameManagerScript _gameManager { get { return GameManagerScript.Instance; } }

    public void StartNewGame(){
        _gameManager.NewGameClicked();
    }

    public void LoadGame(){
        _gameManager.LoadGame();
    }

    public void QuitGame(){
        _gameManager.QuitGame();
    }

    // --------------------- debug mode ------------------------------
    
    public void SetStartingArea(float area){
        _gameManager.SetStartingArea(area);
    }

    public void SetStartingLevel(float level){
        _gameManager.SetStartingLevel(level);
    }

	public void SetUnlockAll(bool val) {
		_gameManager.SetUnlockAll(val);
	}

	public void SetSkipIntro(bool val) {
		_gameManager.SetSkipIntro(val);
	}
}
