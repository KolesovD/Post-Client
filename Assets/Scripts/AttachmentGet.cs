using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class AttachmentGet : MonoBehaviour {
    [SerializeField] private Sprite nonActiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite clickedSprite;
    [SerializeField] private Text attachmentName;

    private Image _attachmentImage;
    private string _fullName;

    private string _base64Content;

    void Start()
    {
        _attachmentImage = GetComponent<Image>();
    }

    public void PointerDown()
    {
        _attachmentImage.sprite = clickedSprite;
    }

    public void PointerEnter()
    {
        _attachmentImage.sprite = activeSprite;
    }

    public void PointerExit()
    {
        _attachmentImage.sprite = nonActiveSprite;
    }

    public void SetContent(string[] content)
    {
        //Debug.Log(attachmentName.text);

        if (content.Length == 2)
        {
            if (content[0].Length > 12)
                attachmentName.text = string.Concat(content[0].Substring(0, 9), "...");
            else attachmentName.text = content[0];
            _fullName = content[0];
            _base64Content = content[1];
        }
        else attachmentName.text = "I AM ERROR";
    }

    public void DownloadAttachment()
    {
        string path = EditorUtility.SaveFilePanel("Сохранить файл", "", _fullName, _fullName.Substring(_fullName.LastIndexOf(".") + 1, _fullName.Length - _fullName.LastIndexOf(".") - 1));

        if (path.Length != 0)
        {
            File.WriteAllBytes(path, System.Convert.FromBase64String(_base64Content));
        }
    }
}
