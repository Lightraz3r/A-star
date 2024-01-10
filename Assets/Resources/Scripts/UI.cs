using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public static UI Instance;
    public GameObject ShowPath;
    public GameObject PosSwitcher;

    private void Start()
    {
        Instance = this;
    }

    public void PathText(bool show)
    {//Change show path text 
        TMP_Text tmp = ShowPath.GetComponent<TMP_Text>();
        tmp.text = "Show path (" + (show ? "ON" : "OFF") + ")";
    }

    public void SwitcherText(bool lShiftPressed)
    {//Change text that indicates what posiition youre changing
        TMP_Text tmp = PosSwitcher.GetComponent<TMP_Text>();
        tmp.text = "Change " + (lShiftPressed ? "AI" : "Goal") + " Position";
    }
}
