using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttachmentSend : MonoBehaviour {
    public Text attachName;
    public Text attachSize;

    [SerializeField] private GameObject attachmentObject;

    private byte[] attachmentContent;
    
    public void SetContent (string name, int size, byte[] content)
    {
        attachName.text = name;
        attachSize.text = size.ToString();
        attachmentContent = content;
    }

    public void DeleteAttach ()
    {
        Messenger<GameObject>.Broadcast(PostClientEvents.DELETE_ATTACHMENT_TO_SEND, attachmentObject);
    }

    public string GetBase64Content()
    {
        return System.Convert.ToBase64String(attachmentContent);
    }

    public string GetBase64Name()
    {
        byte[] name = System.Text.Encoding.UTF8.GetBytes(attachName.text);
        return System.Convert.ToBase64String(name);
    }
}
