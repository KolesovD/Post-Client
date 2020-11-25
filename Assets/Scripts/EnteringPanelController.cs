using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EnteringPanelController : MonoBehaviour {
    [SerializeField] private InputField loginInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Text statusLabel;

    private void Awake()
    {
        Messenger<string>.AddListener(PostClientEvents.UPDATE_STATUS, UpdateStatusString);
        Messenger.AddListener(PostClientEvents.CLEAR_STATUS, ClearStatusString);
    }

    private void OnDestroy()
    {
        Messenger<string>.RemoveListener(PostClientEvents.UPDATE_STATUS, UpdateStatusString);
        Messenger.RemoveListener(PostClientEvents.CLEAR_STATUS, ClearStatusString);
    }

    public void SendLoginAndPassword()
    {
        string login = loginInput.text;
        string password = passwordInput.text;

        /*loginInput.GetComponentInParent<InputField>().text = "";
        passwordInput.GetComponentInParent<InputField>().text = "";*/
        ClearStatusString();

        Messenger<string, string>.Broadcast(PostClientEvents.START_POP3_SESSION, login, password);
        Messenger<string>.Broadcast(PostClientEvents.UPDATE_SENDER_MAIL, login);
        //Debug.Log("Login: " + loginInput.text + " Password: " + passwordInput.text);
    }

    public void ClearStatusString()
    {
        statusLabel.text = "";
    }

    public void UpdateStatusString(string message)
    {
        statusLabel.text = message;
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
