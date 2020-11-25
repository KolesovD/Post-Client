using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(POPSocket))]
[RequireComponent(typeof(SMTPSocket))]
public class SocketManagment : MonoBehaviour {
    [SerializeField] private GameObject enteringPanel;
    [SerializeField] private GameObject readMailPanel;
    [SerializeField] private GameObject sendMailPanel;
    [SerializeField] private GameObject dialogWindow;

    private POPSocket _pop;
    private SMTPSocket _smtp;

    private void Awake()
    {
        _pop = GetComponent<POPSocket>();
        _smtp = GetComponent<SMTPSocket>();
        Messenger<string, string>.AddListener(PostClientEvents.START_POP3_SESSION, InializeSockets);
        Messenger<int>.AddListener(PostClientEvents.FORM_LETTER, ShowMailInformation);
        Messenger<int>.AddListener(PostClientEvents.DELETE_LETTER, DeleteMail);
        Messenger.AddListener(PostClientEvents.OPEN_SENDMAIL_PANEL, SwitchFromViewToSend);
        Messenger.AddListener(PostClientEvents.CLOSE_SENDMAIL_PANEL, SwitchFromSendToView);
        Messenger<string>.AddListener(PostClientEvents.SHOW_DIALOG_WINDOW, OpenDialogWindow);
        Messenger.AddListener(PostClientEvents.CLOSE_DIALOG_WINDOW, CloseDialogWindow);
        Messenger.AddListener(PostClientEvents.CONNECTION_PROBLEMS, ConnectionProblems);
        Messenger<bool>.AddListener(PostClientEvents.CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE, ConnectionProblems);
        Messenger<string, string>.AddListener(PostClientEvents.SEND_LETTER, SendLetter);
    }

    void Start()
    {
        readMailPanel.SetActive(false);
        sendMailPanel.SetActive(false);
        dialogWindow.SetActive(false);
    }

    private void OnDestroy()
    {
        _pop.CloseSocket();
        _smtp.CloseSocket();
        Messenger<string, string>.RemoveListener(PostClientEvents.START_POP3_SESSION, InializeSockets);
        Messenger<int>.RemoveListener(PostClientEvents.FORM_LETTER, ShowMailInformation);
        Messenger<int>.RemoveListener(PostClientEvents.DELETE_LETTER, DeleteMail);
        Messenger.RemoveListener(PostClientEvents.OPEN_SENDMAIL_PANEL, SwitchFromViewToSend);
        Messenger.RemoveListener(PostClientEvents.CLOSE_SENDMAIL_PANEL, SwitchFromSendToView);
        Messenger<string>.RemoveListener(PostClientEvents.SHOW_DIALOG_WINDOW, OpenDialogWindow);
        Messenger.RemoveListener(PostClientEvents.CLOSE_DIALOG_WINDOW, CloseDialogWindow);
        Messenger.RemoveListener(PostClientEvents.CONNECTION_PROBLEMS, ConnectionProblems);
        Messenger<bool>.RemoveListener(PostClientEvents.CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE, ConnectionProblems);
        Messenger<string, string>.RemoveListener(PostClientEvents.SEND_LETTER, SendLetter);
    }

    //Функция инициализации сокетов и отправки данных для формирования основного меню программы
    public void InializeSockets(string login, string password)
    {
        if (!_pop.IsConnected)
            _pop.Connect();
        if (!_smtp.IsConnected)
            _smtp.Connect();

        if (_pop.IsConnected && _smtp.IsConnected)
        {
            bool popWorking = _pop.StartPOPSession(login, password);
            bool smtpWorking = _smtp.CheckConnection(login, password);
            if (popWorking && smtpWorking)
            {
                enteringPanel.SetActive(false);
                readMailPanel.SetActive(true);
                UpdateMailList();
            }
            else if (popWorking || smtpWorking)
            {
                _pop.CloseSocket();
                _smtp.CloseSocket();
            }
            else
            {
                if (_pop.IsConnected) Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Введены неверные данные для входа");
            }
        }
    }

    public void ShowMailInformation(int number)
    {
        _pop.ReadLetter(number);
    }

    public void UpdateMailList()
    {
        _pop.FormMailList();
    }

    public void DeleteMail (int number)
    {
        _pop.DeleteMail(number);
    }

    public void SwitchFromViewToSend()
    {
        readMailPanel.SetActive(false);
        sendMailPanel.SetActive(true);
    }

    public void SwitchFromSendToView()
    {
        readMailPanel.SetActive(true);
        sendMailPanel.SetActive(false);
    }

    public void SendLetter(string receiver, string letter)
    {
        _smtp.WriteALetter(receiver, letter);
    }

    public void ConnectionProblems()
    {
        _smtp.CloseSocket();
        _pop.CloseSocket();

        enteringPanel.SetActive(true);
        readMailPanel.SetActive(false);
        sendMailPanel.SetActive(false);
        dialogWindow.SetActive(false);

        enteringPanel.GetComponent<EnteringPanelController>().UpdateStatusString("Проблемы с соединением");
    }

    public void ConnectionProblems(bool needMessage)
    {
        _smtp.CloseSocket();
        _pop.CloseSocket();

        enteringPanel.SetActive(true);
        readMailPanel.SetActive(false);
        sendMailPanel.SetActive(false);
        dialogWindow.SetActive(false);

        if (needMessage) enteringPanel.GetComponent<EnteringPanelController>().UpdateStatusString("Проблемы с соединением");
    }

    public void OpenDialogWindow(string message)
    {
        dialogWindow.GetComponent<DialogWindowController>().SetDialogWindowText(message);
        dialogWindow.SetActive(true);
    }

    public void CloseDialogWindow()
    {
        dialogWindow.SetActive(false);
    }
}
