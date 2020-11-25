using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Text.RegularExpressions;

public class SendPanelController : MonoBehaviour {
    [SerializeField] private Text receiver;
    [SerializeField] private Text mailTheme;
    [SerializeField] private Text mailText;
    [SerializeField] private Text statusText;

    [SerializeField] private RectTransform attachView;
    [SerializeField] private GameObject attachPanel;
    [SerializeField] private GameObject attachExample;

    private List<GameObject> _attachToSendList;
    private string senderMail;

    void Awake()
    {
        Messenger<GameObject>.AddListener(PostClientEvents.DELETE_ATTACHMENT_TO_SEND, DeleteAttach);
        Messenger<string>.AddListener(PostClientEvents.UPDATE_SENDER_MAIL, SetSenderMail);
        Messenger.AddListener(PostClientEvents.SMTP_CLEAR_INPUTS, ClearInputs);
    }

    void Start()
    {
        _attachToSendList = new List<GameObject>();
        senderMail = "default@none.no";
    }

    void OnDestroy()
    {
        Messenger<GameObject>.RemoveListener(PostClientEvents.DELETE_ATTACHMENT_TO_SEND, DeleteAttach);
        Messenger<string>.RemoveListener(PostClientEvents.UPDATE_SENDER_MAIL, SetSenderMail);
        Messenger.RemoveListener(PostClientEvents.SMTP_CLEAR_INPUTS, ClearInputs);
    }

    public void SendLetter()
    {
        StartCoroutine(SendLetterCoroutine());
    }

    public IEnumerator SendLetterCoroutine()
    {
        if (receiver.text.Length == 0)
            statusText.text = "Не выбран получатель.";
        else if (!new Regex("^.*@.*\\..*").Match(receiver.text).Success)
            statusText.text = "Введите корректную почту.";
        else if (mailTheme.text.Length == 0)
            statusText.text = "Не заполнена тема.";
        else if (mailText.text.Length == 0)
            statusText.text = "Не заполнен текст письма.";
        else
        {
            statusText.text = "";
            Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Подождите...\n\nПисьмо обрабатывается.");

            //byte[] mail = System.Text.Encoding.UTF8.GetBytes(senderMail);
            byte[] theme = System.Text.Encoding.UTF8.GetBytes(mailTheme.text);
            byte[] text = System.Text.Encoding.UTF8.GetBytes(mailText.text);
            string sendingContent = string.Concat("Subject: =?UTF-8?B?" + System.Convert.ToBase64String(theme) + "?=\n", "From: <" + senderMail + ">\n",
                "To: <" + receiver.text + ">\n", "X-Mailer: DanPostClient\n", "Content-Type: multipart/mixed; boundary=\"globalbound\"\n",
                "\n--globalbound\n", "Content-Type: text/plain; charset=\"UTF-8\"\n", "Content-Transfer-Encoding: base64\n",
                "\n" + System.Convert.ToBase64String(text) + "\n");
            if (_attachToSendList.Count != 0)
            {
                foreach (GameObject attach in _attachToSendList)
                {
                    yield return 0;

                    sendingContent = string.Concat(sendingContent, "--globalbound\n",
                        "Content-Type: application/octet-stream; name=\"=?UTF-8?B?" + attach.GetComponent<AttachmentSend>().GetBase64Name() + "?=\"\n",
                        "Content-Disposition: attachment; filename=\"=?UTF-8?B?" + attach.GetComponent<AttachmentSend>().GetBase64Name() + "?=\"\n",
                        "Content-Transfer-Encoding: base64\n", "\n" + attach.GetComponent<AttachmentSend>().GetBase64Content() + "\n");
                }
            }
            sendingContent = string.Concat(sendingContent, "--globalbound--\n", ".\n");

            /////////////
            /*string filename = Path.Combine(Application.persistentDataPath, "send mail.txt");
            FileStream fs = File.Create(filename);
            byte[] input = System.Text.Encoding.UTF8.GetBytes(sendingContent);
            fs.Write(input, 0, input.Length);
            fs.Close();*/
            /////////////

            Messenger<string, string>.Broadcast(PostClientEvents.SEND_LETTER, receiver.text, sendingContent);
        }
    }

    public void AddAttach()
    {
        statusText.text = "";

        string path = EditorUtility.OpenFilePanel("Открыть файл", "", "");

        if (path.Length != 0)
        {
            byte[] fileContent = File.ReadAllBytes(path);

            GameObject nextAttachment = Instantiate(attachExample) as GameObject;
            nextAttachment.transform.SetParent(attachPanel.transform);
            
            
            Vector2 panelSize = attachPanel.GetComponent<RectTransform>().sizeDelta;

            nextAttachment.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, attachExample.GetComponent<RectTransform>().sizeDelta.y);
            if (panelSize.y < attachView.sizeDelta.y) nextAttachment.transform.localPosition = new Vector3(482.5f, -10 - (20 * _attachToSendList.Count), 0);
            else nextAttachment.transform.localPosition = new Vector3(474, -10 - (20 * _attachToSendList.Count), 0);

            panelSize.y = nextAttachment.GetComponent<RectTransform>().sizeDelta.y * (_attachToSendList.Count + 1);
            attachPanel.GetComponent<RectTransform>().sizeDelta = panelSize;

            if (_attachToSendList.Count % 2 == 0)
                nextAttachment.GetComponent<Image>().color = new Color32(215, 247, 255, 255);
            else nextAttachment.GetComponent<Image>().color = new Color32(153, 198, 226, 255);

            nextAttachment.GetComponent<AttachmentSend>().SetContent(path.Substring(path.LastIndexOf('/') + 1, path.Length - (path.LastIndexOf('/') + 1)), fileContent.Length, fileContent);

            _attachToSendList.Add(nextAttachment);
        }
    }

    public void RefreshAttaches()
    {
        statusText.text = "";

        float panelSizeY = attachPanel.GetComponent<RectTransform>().sizeDelta.y;
        
        for (int i = 0; i < _attachToSendList.Count; i++)
        {
            if (panelSizeY < attachView.sizeDelta.y) _attachToSendList[i].transform.localPosition = new Vector3(482.5f, -10 - (20 * i), 0);
            else _attachToSendList[i].transform.localPosition = new Vector3(474, -10 - (20 * i), 0);

            if (i % 2 == 0)
                _attachToSendList[i].GetComponent<Image>().color = new Color32(215, 247, 255, 255);
            else _attachToSendList[i].GetComponent<Image>().color = new Color32(153, 198, 226, 255);
        }

        attachPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(attachPanel.GetComponent<RectTransform>().sizeDelta.x, attachExample.GetComponent<RectTransform>().sizeDelta.y * _attachToSendList.Count);
    }

    public void DeleteAttach(GameObject attach)
    {
        statusText.text = "";

        _attachToSendList.Remove(attach);
        Destroy(attach);
        RefreshAttaches();
    }

    public void SetSenderMail(string lastSenderMail)
    {
        senderMail = lastSenderMail;
    }

    public void ClosePanel()
    {
        ClearInputs();
        Messenger.Broadcast(PostClientEvents.CLOSE_SENDMAIL_PANEL);
    }
    
    public void ClearInputs()
    {
        receiver.GetComponentInParent<InputField>().text = "";
        mailTheme.GetComponentInParent<InputField>().text = "";
        mailText.GetComponentInParent<InputField>().text = "";

        statusText.text = "";

        foreach (GameObject attach in _attachToSendList)
            Destroy(attach);
        _attachToSendList.Clear();
    }
}
