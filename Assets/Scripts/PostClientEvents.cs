using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PostClientEvents {
    public const string START_POP3_SESSION = "START_POP3_SESSION";
    public const string UPDATE_STATUS = "UPDATE_STATUS";
    public const string CLEAR_STATUS = "CLEAR_STATUS";
    public const string CONNECTION_PROBLEMS = "CONNECTION_PROBLEMS";
    public const string CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE = "CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE";

    public const string FORM_LETTER = "FORM_LETTER";
    public const string FORM_LIST_OF_MAILS = "FORM_LIST_OF_MAILS";
    public const string SHOW_LETTER = "SHOW_LETTER";
    public const string DELETE_LETTER = "DELETE_LETTER";

    public const string SEND_LETTER = "SEND_LETTER";
    public const string SMTP_CLEAR_INPUTS = "SMTP_CLEAR_INPUTS";

    public const string UPDATE_SENDER_MAIL = "UPDATE_SENDER_MAIL";
    public const string DELETE_ATTACHMENT_TO_SEND = "DELETE_ATTACHMENT_TO_SEND";

    public const string OPEN_SENDMAIL_PANEL = "OPEN_SENDMAIL_PANEL";
    public const string CLOSE_SENDMAIL_PANEL = "CLOSE_SENDMAIL_PANEL";

    public const string SHOW_DIALOG_WINDOW = "SHOW_DIALOG_WINDOW";
    public const string CLOSE_DIALOG_WINDOW = "CLOSE_DIALOG_WINDOW";

    public const string SET_DIALOG_WINDOW_TEXT = "SET_DIALOG_WINDOW_TEXT";
}
