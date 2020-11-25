using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShortContent : MonoBehaviour {
    [SerializeField] private Image _background;
    [SerializeField] private Color32 _color;
    [SerializeField] private Color32 _activeColor;
    [SerializeField] private Color32 _clickColor;
    [SerializeField] private Text nameText;
    [SerializeField] private Text mailText;
    [SerializeField] private Text themeText;
    [SerializeField] private Text dateText;

    public int letterNumber = 0;

	private void Start () {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(475, 75);
        //collider.transform.localPosition;
    }

    private void Update()
    {
        /*if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
        {
            _background.color = _clickColor;
        }
        else if (EventSystem.current.IsPointerOverGameObject())
            _background.color = _activeColor;
        else _background.color = _color;*/
    }

    public void PointerDown()
    {
        _background.color = _clickColor;
    }

    public void PointerEnter()
    {
        _background.color = _activeColor;
    }

    public void PointerExit()
    {
        _background.color = _color;
    }

    public void SetColor(Color32 newColor)
    {
        _background.color = newColor;
        _color = newColor;

        _activeColor.r = newColor.r < 245 ? (byte)(newColor.r + 10) : (byte)255;
        _activeColor.g = newColor.g < 245 ? (byte)(newColor.g + 10) : (byte)255;
        _activeColor.b = newColor.b < 245 ? (byte)(newColor.b + 10) : (byte)255;
        _activeColor.a = 255;

        _clickColor.r = newColor.r < 235 ? (byte)(newColor.r + 20) : (byte)255;
        _clickColor.g = newColor.g < 235 ? (byte)(newColor.g + 20) : (byte)255;
        _clickColor.b = newColor.b < 235 ? (byte)(newColor.b + 20) : (byte)255;
        _clickColor.a = 255;
    }

    public void SetText(string senderName, string senderMail, string sendDate, string senderedSubject)
    {
        nameText.text = senderName;
        mailText.text = senderMail;
        themeText.text = sendDate;
        dateText.text = senderedSubject;
    }

    public void FormLetter()
    {
        Messenger<int>.Broadcast(PostClientEvents.FORM_LETTER, letterNumber);
    }

    public void DeleteMail()
    {
        Messenger<int>.Broadcast(PostClientEvents.DELETE_LETTER, letterNumber);
    }
}
