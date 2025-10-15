using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextComponent : MonoBehaviour
{
    public void SetFloatAsTextValue(float val){
        this.GetComponent<Text>().text = val.ToString("0");
    }
}
