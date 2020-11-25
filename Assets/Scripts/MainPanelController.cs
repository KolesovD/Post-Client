using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour {
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject mailShorts;
    [SerializeField] private Text mailSubjectText;
    [SerializeField] private RectTransform contentField;
    [SerializeField] private Text mailText;

    [SerializeField] private GameObject attachment;
    [SerializeField] private GameObject attachPanel;

    private List<GameObject> _shortMailInfo;
    private List<GameObject> _attachments;

    private const int _attachHeight = 100;
    private const int _attachWidth = 100;
    private const int _attachBound = 15;
    private const int _attachNameHeight = 20;

    public int numberOfMails { get
        {
            return _shortMailInfo.Count;
        } }

    private void Start () {
        _shortMailInfo = new List<GameObject>();
        _attachments = new List<GameObject>();
	}

    private void Awake()
    {
        Messenger<List<MailShortInfo>>.AddListener(PostClientEvents.FORM_LIST_OF_MAILS, FormListOfMails);
        Messenger<List<string[]>>.AddListener(PostClientEvents.SHOW_LETTER, ShowLetter);
    }

    private void OnDestroy()
    {
        Messenger<List<MailShortInfo>>.RemoveListener(PostClientEvents.FORM_LIST_OF_MAILS, FormListOfMails);
        Messenger<List<string[]>>.RemoveListener(PostClientEvents.SHOW_LETTER, ShowLetter);
    }

    //Функция формирования графического списка с темами писем
    public void FormListOfMails(List<MailShortInfo> mailsShortInfo)
    {
        foreach (GameObject mailShort in _shortMailInfo)
            Destroy(mailShort);
        _shortMailInfo.Clear();

        int numberOfLists = mailsShortInfo.Count;
        content.GetComponent<RectTransform>().sizeDelta = new Vector2 (mailShorts.GetComponent<RectTransform>().sizeDelta.x, 0);

        for (int i = 0; i < numberOfLists; i++)
        {
            if (!mailsShortInfo[i].sendDate.Equals(""))
            {
                Vector2 contentSize = content.GetComponent<RectTransform>().sizeDelta;
                contentSize.y += mailShorts.GetComponent<RectTransform>().sizeDelta.y;
                content.GetComponent<RectTransform>().sizeDelta = contentSize;

                GameObject nextMailShort = Instantiate(mailShorts) as GameObject;
                nextMailShort.transform.SetParent(content.transform);

                Vector3 contentPos = nextMailShort.transform.localPosition;
                contentPos.x = mailShorts.transform.position.x;
                contentPos.y = -mailShorts.GetComponent<RectTransform>().sizeDelta.y * i;
                nextMailShort.transform.localPosition = contentPos;

                nextMailShort.GetComponent<RectTransform>().sizeDelta = mailShorts.GetComponent<RectTransform>().sizeDelta;

                nextMailShort.GetComponent<ShortContent>().SetText(mailsShortInfo[i].senderName, mailsShortInfo[i].senderMail, mailsShortInfo[i].sendDate, mailsShortInfo[i].senderedSubject);

                if (i % 2 == 0)
                    nextMailShort.GetComponent<ShortContent>().SetColor(new Color32(150, 150, 255, 255));
                else nextMailShort.GetComponent<ShortContent>().SetColor(new Color32(165, 165, 235, 255));

                nextMailShort.GetComponent<ShortContent>().letterNumber = i + 1;

                _shortMailInfo.Add(nextMailShort);
            }
            else Debug.Log(mailsShortInfo[i].senderMail + " equals null");
        }

        Messenger.Broadcast(PostClientEvents.CLOSE_DIALOG_WINDOW);
    }

    public void ShowLetter(List<string[]> letter)
    {
        if (letter == null)
        {
            mailSubjectText.text = "Ошибка чтения письма";
        }
        else
        {
            int letterSize = letter.Count;

            int attachInARow = (int) (attachPanel.GetComponent<RectTransform>().rect.width - _attachBound)/(_attachWidth + _attachBound);

            foreach (GameObject attach in _attachments)
                Destroy(attach);
            _attachments.Clear();

            for (int i = 0; i < letterSize; i++)
            {
                if (letter[i][0] == AppConstants.subjectOfMessage)
                    mailSubjectText.text = letter[i][1];
                else if (letter[i][0] == AppConstants.textPartOfMessage)
                {
                    TextGenerator textGen = new TextGenerator();
                    TextGenerationSettings rectSetting = mailText.GetGenerationSettings(mailText.rectTransform.rect.size);
                    contentField.sizeDelta = new Vector2(0, textGen.GetPreferredHeight(letter[i][1], rectSetting) + 16);

                    mailText.text = letter[i][1];
                }
                else
                {
                    attachPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (_attachments.Count / 7 + 1) * (_attachHeight + _attachBound + _attachNameHeight) + _attachBound);

                    Vector3 attachPos = new Vector3(_attachments.Count % attachInARow * (_attachWidth + _attachBound) + _attachBound + _attachWidth / 2, -(_attachments.Count / 7 * (_attachHeight + _attachBound + _attachNameHeight) + _attachBound + _attachHeight / 2), 0);

                    GameObject nextAttachment = Instantiate(attachment) as GameObject;
                    nextAttachment.transform.SetParent(attachPanel.transform);
                    nextAttachment.transform.localPosition = attachPos;

                    nextAttachment.GetComponent<AttachmentGet>().SetContent(letter[i]);

                    _attachments.Add(nextAttachment);
                }
            }

            Messenger.Broadcast(PostClientEvents.CLOSE_DIALOG_WINDOW);
        }
        
    }
}
