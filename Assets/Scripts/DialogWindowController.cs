using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindowController : MonoBehaviour {
    [SerializeField] private Text dialogWindowText;

	public void SetDialogWindowText(string text)
    {
        dialogWindowText.text = text;
    }
}
