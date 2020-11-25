using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Text.RegularExpressions;

public class POPSocket : MonoBehaviour {

    private TcpClient _popClient;
    private SslStream _popStream;

    private byte[] _buff = new byte[5000];
    private string _stringBuff;
    private int _numberOfBytesInLastReading;
    private int _readMailIterator;
    private string _stringsReadedInLetter;
    private string _lastLogin;
    private string _lastPassword;
    private IEnumerator updateConnection;

    private const int _numberOfStringsToReadPerOneFrame = 50;

    public int numberOfLettersInMailbox { get; private set; }
    public bool IsConnected { get { return _popClient==null ? false : _popClient.Connected; } }
    public bool IsLogged { get; private set; }
    public bool IsWorking { get; private set; }

    private void Awake()
    {
        _numberOfBytesInLastReading = 0;
        updateConnection = UpdateConnection();
    }

    private void OnDestroy()
    {
        CloseSocket();
    }

    void Start()
    {
        numberOfLettersInMailbox = 0;
        IsLogged = false;
        IsWorking = false;

        StartCoroutine(StartConnection());
    }

    //Устанавливаем соединение
    public IEnumerator StartConnection()
    {
        Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Выполняется подключение");
        yield return new WaitForSeconds(.5f);
        Connect();
    }

    //Устанавливаем соединение
    public void Connect()
    {
        try
        {
            if (_popClient != null)
            {
                _popStream.Close();
                _popClient.Close();
            }
            _popClient = new TcpClient(AppConstants.pop3ServerAdress, 995);
            _popStream = new SslStream(_popClient.GetStream());
            _popStream.AuthenticateAsClient(AppConstants.pop3ServerAdress);
            Debug.Log(_popStream.IsAuthenticated + " " + _popStream.IsSigned);
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            Debug.Log("POP3 CONNECTED");
            Messenger.Broadcast(PostClientEvents.CLEAR_STATUS);
        }
        catch
        {
            Messenger<bool>.Broadcast(PostClientEvents.CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE, false);
            Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Попытка подключения POP3 сокета закончилась неудачей");
        }
    }

    //Функция начала POP3-сессии
    public bool StartPOPSession(string login, string password)
    {
        _lastLogin = login;
        _lastPassword = password;
        //Debug.Log(_lastLogin + " " + _lastPassword);

        if (IsConnected)
        {

            string answer;

            //Отправляем логин
            _popStream.Write(System.Text.Encoding.UTF8.GetBytes("USER " + login + "\n"));
            ReceiveFromSocket();
            answer = GetStringFromSocket();
            Debug.Log(answer);
            if (!answer[0].Equals('+'))
                return IsLogged;

            //Отправляем пароль
            _popStream.Write(System.Text.Encoding.UTF8.GetBytes("PASS " + password + "\n"));
            ReceiveFromSocket();
            answer = GetStringFromSocket();
            Debug.Log(answer);
            if (!answer[0].Equals('+'))
                return IsLogged;

            IsLogged = true;
            StartCoroutine(updateConnection);

            return IsLogged;
        }
        else
        {
            return false;
        }
    }

    //Функция получения количества писем в почтовом ящике
    public void GetNumberOfLetters()
    {
        if (IsLogged)
        {
            _popStream.Write(System.Text.Encoding.UTF8.GetBytes("STAT\n"));
            ReceiveFromSocket();
            _stringBuff = GetStringFromSocket();

            int numberOfLetters = 0;
            int.TryParse(_stringBuff.Split(' ')[1], out numberOfLetters);
            numberOfLettersInMailbox = numberOfLetters;
        }
    }

    //Функция формирования списка писем
    public void FormMailList()
    {
        StartCoroutine(FormMailListCoroutine());
    }

    public IEnumerator FormMailListCoroutine()
    {
        if (IsLogged)
        {
            IsWorking = true;
            Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Подождите...\n\nИдёт обновление списка писем.");

            //Обновляем количество писем в почтовом ящике
            GetNumberOfLetters();
            Debug.Log(numberOfLettersInMailbox);
            yield return 0;

            //Формируем новый список короткой информации о письмах
            List<MailShortInfo> mailsShortInfo = new List<MailShortInfo>(numberOfLettersInMailbox);

            for (int i = 0; i < numberOfLettersInMailbox; i++)
            {
                MailShortInfo nextInfo = new MailShortInfo() { senderName = "", senderMail = "", senderedSubject = "", sendDate = "" };

                //Собираем заголовки писем
                _popStream.Write(System.Text.Encoding.UTF8.GetBytes("TOP " + (i + 1) + " 0\n"));
                yield return StartCoroutine(ReadSocketList());

                _stringsReadedInLetter = string.Join("", _stringsReadedInLetter.Split(new char[] { '\0', '\r' }));
                string[] stringArray = _stringsReadedInLetter.Split('\n');

                for (int j = 0; j < stringArray.Length; j++)
                {
                    //From
                    if (stringArray[j].IndexOf("From:") == 0)
                    {
                        string senderMail;
                        string senderName;
                        if (stringArray[j].Contains("<"))
                        {
                            senderMail = stringArray[j].Substring(stringArray[j].IndexOf('<') + 1, stringArray[j].IndexOf('>') - stringArray[j].IndexOf('<') - 1);
                            senderName = stringArray[j].IndexOf('<') == 6 ? senderMail : NormalizeString(stringArray[j].Substring(6, stringArray[j].IndexOf('<') - 7));
                        }
                        else
                        {
                            senderName = NormalizeString(stringArray[j].Substring(6, stringArray[j].Length - 6));
                            senderMail = senderName;
                        }

                        nextInfo.senderName = senderName;
                        nextInfo.senderMail = senderMail;
                    }

                    //Subject
                    if (stringArray[j].IndexOf("Subject: ") == 0)
                    {
                        string subject = stringArray[j].Substring(9);
                        for (int k = j; k < stringArray.Length - 1; k++)
                            if (!stringArray[k + 1].Contains(":"))
                                subject += stringArray[k + 1];
                            else break;

                        subject = Regex.Replace(subject, @"=\?\S*\?=", (Match match) =>
                        {
                            return NormalizeString(match.Value);
                        });

                        nextInfo.senderedSubject = subject;
                    }

                    //Date
                    if (stringArray[j].IndexOf("Date: ") == 0)
                    {
                        string sendDate;
                        if (stringArray[j].Contains("+"))
                            sendDate = stringArray[j].Substring(stringArray[j].IndexOf(',') + 2, stringArray[j].IndexOf('+') - (stringArray[j].IndexOf(',') + 3));
                        else sendDate = stringArray[j].Substring(stringArray[j].IndexOf(',') + 2, stringArray[j].IndexOf('-') - (stringArray[j].IndexOf(',') + 3));

                        nextInfo.sendDate = sendDate;
                    }
                }

                mailsShortInfo.Add(nextInfo);
            }
            IsWorking = false;

            Messenger<List<MailShortInfo>>.Broadcast(PostClientEvents.FORM_LIST_OF_MAILS, mailsShortInfo);
        }
    }

    //Чтение письма по номеру
    public void ReadLetter(int number)
    {
        StartCoroutine(ReadLetterCoroutine(number));
    }

    private IEnumerator ReadLetterCoroutine(int number)
    {
        if (IsLogged)
        {
            IsWorking = true;
            GetNumberOfLetters();
            IsWorking = false;

            if (number <= numberOfLettersInMailbox && number >= 1)
            {
                IsWorking = true;

                Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Подождите...\n\nИдёт чтение письма.");

                _popStream.Write(System.Text.Encoding.UTF8.GetBytes("RETR " + number + "\n"));
                yield return StartCoroutine(ReadSocketList());

                Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Подождите...\n\nИдёт обработка письма.");
                /////////////
                string filename = Path.Combine(Application.persistentDataPath, "mail.txt");
                FileStream fs = File.Create(filename);
                byte[] input = System.Text.Encoding.UTF8.GetBytes(_stringsReadedInLetter);
                fs.Write(input, 0, input.Length);
                fs.Close();
                /////////////

                IsWorking = false;

                string[] mailSplitString = _stringsReadedInLetter.Split('\n');
                List<string[]> parsedMail = new List<string[]>();
                yield return StartCoroutine(ReadMail(mailSplitString, parsedMail));
            }
        }
    }

    //Функция чтения текста из сокета до "." или пока не пройдёт 50 тысяч итераций
    private IEnumerator ReadSocketList()
    {
        _stringsReadedInLetter = "";
        int numberOfReading = 50000;

        do
        {
            ReceiveFromSocket();
            _stringsReadedInLetter += GetStringFromSocket();
            numberOfReading--;

            if (numberOfReading % 1000 == 0)
                Debug.Log(numberOfReading);

            if (numberOfReading % _numberOfStringsToReadPerOneFrame == 0)
                yield return 0;

        } while (!_stringsReadedInLetter.Contains("\n.\r\n") && (numberOfReading>0));

    }

    //Обновить список писем
    public void RefreshMailList()
    {
        StopCoroutine(updateConnection);
        IsLogged = false;

        _popStream.Write(System.Text.Encoding.UTF8.GetBytes("QUIT\n"));
        ReceiveFromSocket();
        Debug.Log(GetStringFromSocket());

        Connect();

        string answer;
        _popStream.Write(System.Text.Encoding.UTF8.GetBytes("USER " + _lastLogin + "\n"));
        ReceiveFromSocket();
        answer = GetStringFromSocket();
        Debug.Log(answer);
        if (!answer[0].Equals('+'))
        {
            Messenger.Broadcast(PostClientEvents.CONNECTION_PROBLEMS);
            Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Повторите данные для входа");
            return;
        }

        _popStream.Write(System.Text.Encoding.UTF8.GetBytes("PASS " + _lastPassword + "\n"));
        ReceiveFromSocket();
        answer = GetStringFromSocket();
        Debug.Log(answer);
        if (!answer[0].Equals('+'))
        {
            Messenger.Broadcast(PostClientEvents.CONNECTION_PROBLEMS);
            Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Повторите данные для входа");
            return;
        }

        IsLogged = true;
        StartCoroutine(updateConnection);

        StartCoroutine(FormMailListCoroutine());
    }

    //Удалить письмо
    public void DeleteMail(int number)
    {
        if (IsLogged)
        {
            IsWorking = true;
            GetNumberOfLetters();
            IsWorking = false;

            if (number <= numberOfLettersInMailbox && number >= 1)
            {
                IsWorking = true;

                Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Подождите...\n\nИдёт удаление письма.");

                _popStream.Write(System.Text.Encoding.UTF8.GetBytes("DELE " + number + "\n"));
                ReceiveFromSocket();

                IsWorking = false;

                RefreshMailList();
            }
        }
    }

    //Функция закрытия сокета
    public void CloseSocket()
    {
        if (IsConnected)
        {
            if (IsLogged)
            {
                IsWorking = true;
                _popStream.Write(System.Text.Encoding.UTF8.GetBytes("QUIT\n"));
                ReceiveFromSocket();
                IsWorking = false;
                IsLogged = false;
            }
            _popStream.Close();
            _popClient.Close();
        }
    }

    //Функция чтения строки из сокета
    private void ReceiveFromSocket()
    {
        Array.Clear(_buff, 0, _buff.Length);
        _numberOfBytesInLastReading = _popStream.Read(_buff, 0, _buff.Length);
    }

    //Функция перевода байтов в буфере в строку по количеству принятых байтов
    private string GetStringFromSocket()
    {
        return System.Text.Encoding.UTF8.GetString(_buff, 0, _numberOfBytesInLastReading);
    }

    //Функция перевода строки в нужную кодировку
    private string NormalizeString (string oldString)
    {
        Regex contentRegex = new Regex(@"=\?(?<convertTo>\S*)\?(?<convertFrom>\S*)\?(?<content>\S*)\?=");
        Match contentMatch = contentRegex.Match(oldString);
        if (contentMatch.Success)
        {
            string convertFrom = contentMatch.Groups["convertFrom"].Value;
            string convertTo = contentMatch.Groups["convertTo"].Value;
            string content = contentMatch.Groups["content"].Value;

            if (convertFrom.Equals("Q") || convertFrom.Equals("q") || convertFrom.Equals("quoted-printable"))
            {
                if (content[0].Equals('_'))
                    content = content.Substring(1);
                content = string.Join(" ", content.Split('_'));
            }
            return content.Contains("\n") ? DecodeString(content.Split('\n'), convertFrom, convertTo) : DecodeString(content, convertFrom, convertTo);
        }   

        return oldString;
    }

    private string DecodeString(string codedString, string convertFrom, string convertTo)
    {
        byte[] encodedBytes;
        string senderedSubject;

        switch (convertFrom)
        {
            case "Q":
            case "q":
            case "quoted-printable":
                encodedBytes = DecodeQuotedPrintable(codedString);
                break;
            case "B":
            case "b":
            case "base64":
                encodedBytes = Convert.FromBase64String(codedString);
                break;
            default:
                encodedBytes = System.Text.Encoding.UTF8.GetBytes(codedString);
                break;
        }

        switch (convertTo)
        {
            case "UTF-8":
            case "utf-8":
                senderedSubject = System.Text.Encoding.UTF8.GetString(encodedBytes);
                break;
            case "KOI8-R":
            case "koi8-r":
                senderedSubject = System.Text.Encoding.GetEncoding("koi8-r").GetString(encodedBytes);
                break;
            default:
                senderedSubject = System.Text.Encoding.UTF8.GetString(encodedBytes);
                break;
        }

        return senderedSubject;
    }

    private string DecodeString(string[] codedString, string convertFrom, string convertTo)
    {
        string senderedSubject = "";
        for (int i = 0; i < codedString.Length; i++)
            senderedSubject += DecodeString(codedString[i], convertFrom, convertTo) + '\n';

        return senderedSubject;
    }

    private byte[] DecodeQuotedPrintable(string stringToDecode)
    {
        List<byte> byteList = new List<byte>();

        for (int i = 0; i < stringToDecode.Length; i++)
        {
            char symbol = stringToDecode[i];
            switch (symbol)
            {
                case '=':
                    if (i != stringToDecode.Length - 1)
                    {
                        byte nextByte = Convert.ToByte(stringToDecode.Substring(i + 1, 2), 16);
                        byteList.Add(nextByte);
                        i += 2;
                    }
                    break;
                default:
                    byteList.Add((byte)symbol);
                    break;
            }
        }

        return byteList.ToArray();
    }

    private IEnumerator ReadMail(string[] mailContent, List<string[]> parseContent)
    {
        parseContent = new List<string[]>();

        int splitLength = mailContent.Length;
        
        for (_readMailIterator = 0; _readMailIterator < splitLength; _readMailIterator++)
        {
            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' })); //Выглядит как костыли, но на самом деле быстрее, чем Replace

            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                yield return 0;

            if (mailContent[_readMailIterator].IndexOf("Subject: ") == 0)
            {
                string subject = mailContent[_readMailIterator].Substring(9);
                for (int i = _readMailIterator; i <splitLength - 1; i++)
                {
                    if (!mailContent[i + 1].Contains(":"))
                    {
                        mailContent[i + 1] = string.Join("", mailContent[i + 1].Split(new char[] { '\0', '\r' }));
                        subject += mailContent[i + 1];
                    }
                    else break;
                }
                subject = Regex.Replace(subject, @"=\?\S*\?=", (Match match) =>
                {
                    return NormalizeString(match.Value);
                });

                parseContent.Add(new string[2] { AppConstants.subjectOfMessage, subject });
            }
            else if (mailContent[_readMailIterator].Contains("Content-Type: multipart"))
            {
                string multipartBounder = mailContent[_readMailIterator].Contains("\"") ? new Regex("boundary=\"(?<bounder>.*)\"").Match(mailContent[_readMailIterator]).Groups["bounder"].Value : new Regex("boundary=(?<bounder>.*)").Match(mailContent[_readMailIterator]).Groups["bounder"].Value;
                while (multipartBounder.Equals(""))
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                    multipartBounder = mailContent[_readMailIterator].Contains("\"") ? new Regex("boundary=\"(?<bounder>.*)\"").Match(mailContent[_readMailIterator]).Groups["bounder"].Value : new Regex("boundary=(?<bounder>.*)").Match(mailContent[_readMailIterator]).Groups["bounder"].Value;
                }

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                } while (!mailContent[_readMailIterator].Equals("--" + multipartBounder));

                yield return StartCoroutine(ReadMailMultipart(mailContent, splitLength, multipartBounder, parseContent));
            }
            else if (mailContent[_readMailIterator].Contains("Content-Type: text/plain"))
            {
                string codingTo = new Regex("charset=\"?(?<codingTo>\\S*)\"?").Match(mailContent[_readMailIterator]).Groups["codingTo"].Value;
                _readMailIterator++;
                mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                string codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                while (codingFrom.Equals(""))
                {
                    if (mailContent[_readMailIterator].Equals(""))
                        while (codingFrom.Equals(""))
                        {
                            _readMailIterator--;
                            codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                        }
                    else
                    {
                        _readMailIterator++;
                        mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                        codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                    }
                }

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                }
                while (!mailContent[_readMailIterator].Equals(string.Empty));

                _readMailIterator++;
                
                string[] textContent = new string[2] { AppConstants.textPartOfMessage, "" };

                switch (codingFrom)
                {
                    case "Q":
                    case "q":
                    case "quoted-printable":
                    default:
                        while (_readMailIterator < splitLength)
                        {
                            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                            if (mailContent[_readMailIterator].Equals("."))
                                break;
                            else textContent[1] = (mailContent[_readMailIterator].Length != 0 && mailContent[_readMailIterator][mailContent[_readMailIterator].Length - 1].Equals('=')) ? string.Concat(textContent[1], DecodeString(mailContent[_readMailIterator], codingFrom, codingTo)) : string.Concat(textContent[1], DecodeString(mailContent[_readMailIterator], codingFrom, codingTo), "\n");
                            _readMailIterator++;
                            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                                yield return 0;
                        }
                        break;
                    case "B":
                    case "b":
                    case "base64":
                        string base64content = "";
                        while (_readMailIterator < splitLength)
                        {
                            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                            if (mailContent[_readMailIterator].Equals("."))
                                break;
                            else base64content += mailContent[_readMailIterator];
                            _readMailIterator++;
                            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                                yield return 0;
                        }
                        textContent[1] = DecodeString(base64content, codingFrom, codingTo);
                        break;
                }
                textContent[1] = textContent[1].Trim('\n');
                parseContent.Add(textContent);
            }
            else if (mailContent[_readMailIterator].Contains("Content-Disposition: attachment"))
            {
                string attachName = new Regex("filename=\"(?<filename>.*)\"").Match(mailContent[_readMailIterator]).Groups["filename"].Value;
                while (attachName.Equals(""))
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                    attachName = new Regex("filename=\"(?<filename>.*)\"").Match(mailContent[_readMailIterator]).Groups["filename"].Value;
                }
                attachName = NormalizeString(attachName);
                string[] attachContent = new string[2] { attachName , "" };

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                }
                while (!mailContent[_readMailIterator].Equals(string.Empty));

                _readMailIterator++;
                
                while (_readMailIterator < splitLength)
                {
                    attachContent[1] += string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' , ' '}));
                    _readMailIterator++;

                    if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                        yield return 0;
                }

                parseContent.Add(attachContent);
            }
        }

        Messenger<List<string[]>>.Broadcast(PostClientEvents.SHOW_LETTER, parseContent);
    }

    private IEnumerator ReadMailMultipart(string[] mailContent, string bound, List<string[]> parseContent)
    {
        int contentLength = mailContent.Length;
        yield return StartCoroutine(ReadMailMultipart(mailContent, contentLength, bound, parseContent));
    }

    private IEnumerator ReadMailMultipart(string[] mailContent, int contentLength, string bound, List<string[]> parseContent)
    {
        for (_readMailIterator++; _readMailIterator < contentLength; _readMailIterator++)
        {
            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));

            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                yield return 0;

            if (mailContent[_readMailIterator].Contains("Content-Type: multipart"))
            {
                string multipartBounder = mailContent[_readMailIterator].Contains("\"") ? new Regex("boundary=\"(?<bounder>.*)\"").Match(mailContent[_readMailIterator]).Groups["bounder"].Value : new Regex("boundary=(?<bounder>.*)").Match(mailContent[_readMailIterator]).Groups["bounder"].Value;
                while (multipartBounder.Equals(""))
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                    multipartBounder = mailContent[_readMailIterator].Contains("\"") ? new Regex("boundary=\"(?<bounder>.*)\"").Match(mailContent[_readMailIterator]).Groups["bounder"].Value : new Regex("boundary=(?<bounder>.*)").Match(mailContent[_readMailIterator]).Groups["bounder"].Value;
                }

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                } while (!mailContent[_readMailIterator].Equals("--" + multipartBounder));
                
                yield return StartCoroutine(ReadMailMultipart(mailContent, contentLength, multipartBounder, parseContent));
            }
            else if (mailContent[_readMailIterator].Contains("Content-Type: text/plain"))
            {
                string codingTo = new Regex("charset=\"?(?<codingTo>\\S*)\"?").Match(mailContent[_readMailIterator]).Groups["codingTo"].Value; //Content-Transfer-Encoding: quoted-printable
                _readMailIterator++;
                mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                string codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                while (codingFrom.Equals(""))
                {
                    if (mailContent[_readMailIterator].Equals(""))
                        while (codingFrom.Equals(""))
                        {
                            _readMailIterator--;
                            codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                        }
                    else
                    {
                        _readMailIterator++;
                        mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                        codingFrom = new Regex(@"Content-Transfer-Encoding: (?<codingFrom>\S*)").Match(mailContent[_readMailIterator]).Groups["codingFrom"].Value;
                    }
                }

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                }
                while (!mailContent[_readMailIterator].Equals(string.Empty));

                _readMailIterator++;

                string[] textContent = new string[2] { AppConstants.textPartOfMessage, "" };

                switch (codingFrom)
                {
                    case "Q":
                    case "q":
                    case "quoted-printable":
                    default:
                        while (_readMailIterator < contentLength)
                        {
                            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                            if (mailContent[_readMailIterator].Contains("--" + bound))
                                break;
                            else textContent[1] = (mailContent[_readMailIterator].Length != 0 && mailContent[_readMailIterator][mailContent[_readMailIterator].Length - 1].Equals('=')) ? string.Concat(textContent[1], DecodeString(mailContent[_readMailIterator], codingFrom, codingTo)) : string.Concat(textContent[1], DecodeString(mailContent[_readMailIterator], codingFrom, codingTo), "\n");
                            //Debug.Log(mailContent[_readMailIterator] + " " + mailContent[_readMailIterator].Length + " ");
                            _readMailIterator++;
                            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                                yield return 0;
                        }
                        break;
                    case "B":
                    case "b":
                    case "base64":
                        string base64content = "";
                        while (_readMailIterator < contentLength)
                        {
                            mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                            if (mailContent[_readMailIterator].Contains("--" + bound))
                                break;
                            else base64content += mailContent[_readMailIterator];
                            _readMailIterator++;
                            if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                                yield return 0;
                        }
                        textContent[1] = DecodeString(base64content, codingFrom, codingTo);
                        break;
                }
                textContent[1] = textContent[1].Trim('\n');
                parseContent.Add(textContent);
            }
            else if (mailContent[_readMailIterator].Contains("Content-Disposition: attachment"))
            {
                string attachName = new Regex("filename=\"(?<filename>.*)\"").Match(mailContent[_readMailIterator]).Groups["filename"].Value;
                while (attachName.Equals(""))
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                    attachName = new Regex("filename=\"(?<filename>.*)\"").Match(mailContent[_readMailIterator]).Groups["filename"].Value;
                }
                attachName = NormalizeString(attachName);
                string[] attachContent = new string[2] { attachName, "" };

                do
                {
                    _readMailIterator++;
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                }
                while (!mailContent[_readMailIterator].Equals(string.Empty));

                _readMailIterator++;

                while (_readMailIterator < contentLength)
                {
                    mailContent[_readMailIterator] = string.Join("", mailContent[_readMailIterator].Split(new char[] { '\0', '\r' }));
                    if (mailContent[_readMailIterator].Contains("--" + bound))
                        break;

                    attachContent[1] += string.Join("", mailContent[_readMailIterator].Split(' '));
                    _readMailIterator++;

                    if (_readMailIterator % _numberOfStringsToReadPerOneFrame == 0)
                        yield return 0;
                }

                parseContent.Add(attachContent);
            }

            if (mailContent[_readMailIterator].Equals("--" + bound + "--"))
            {
                break;
            }
        }
    }

    //Функция поддержки соединения
    public IEnumerator UpdateConnection()
    {
        while (IsLogged)
        {
            if (!IsWorking)
            {
                if (IsConnected)
                {
                    _popStream.Write(System.Text.Encoding.UTF8.GetBytes("NOOP\n"));
                    ReceiveFromSocket();
                    Debug.Log(GetStringFromSocket());
                }
                else Messenger.Broadcast(PostClientEvents.CONNECTION_PROBLEMS);
            }
            yield return new WaitForSeconds(5);
        }
    }
}
