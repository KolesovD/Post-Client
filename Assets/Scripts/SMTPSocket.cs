using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Text.RegularExpressions;

public class SMTPSocket : MonoBehaviour {

    //private Socket _smtpSocket;
    private TcpClient _smtpClient;
    private SslStream _smtpStream;

    private byte[] _buff = new byte[1000];
    private string _stringBuff;
    private int _numberOfBytesInLastReading;
    private string _lastLogin;
    private string _lastPassword;

    public bool IsConnected { get { return _smtpClient == null ? false : _smtpClient.Connected; } }
    public bool IsLogged { get; private set; }
    public bool IsWorking { get; private set; }

    private void Awake()
    {
        _numberOfBytesInLastReading = 0;
    }

    private void OnDestroy()
    {
        CloseSocket();
    }

    void Start()
    {
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
        CloseSocket();
    }

    //Устанавливаем соединение
    public void Connect()
    {
        try
        {
            if (_smtpClient != null)
            {
                _smtpStream.Close();
                _smtpClient.Close();
            }
            _smtpClient = new TcpClient(AppConstants.smtpServerAdress, 465);
            _smtpStream = new SslStream(_smtpClient.GetStream());
            _smtpStream.AuthenticateAsClient(AppConstants.smtpServerAdress);
            Debug.Log(_smtpStream.IsAuthenticated + " " + _smtpStream.IsSigned);
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            Debug.Log("SMTP CONNECTED");
            Messenger.Broadcast(PostClientEvents.CLEAR_STATUS);
        }
        catch
        {
            Messenger<bool>.Broadcast(PostClientEvents.CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE, false);
            Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Попытка подключения SMTP сокета закончилась неудачей");
        }
    }

    //Функция начала SMTP-сессии
    public bool StartSMTPSession(string login, string password)
    {
        _lastLogin = login;
        _lastPassword = password;
        //Debug.Log(_lastLogin + " " + _lastPassword);

        if (IsConnected)
        {
            string answer;

            WriteToSocket("HELO world\n");
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());

            WriteToSocket("AUTH LOGIN\n");
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());

            //Отправляем логин
            byte[] loginBytes = System.Text.Encoding.UTF8.GetBytes(login);
            login = Convert.ToBase64String(loginBytes);
            WriteToSocket(login + "\n");
            ReceiveFromSocket();
            answer = GetStringFromSocket();
            Debug.Log(answer);
            if (!answer[0].Equals('3'))
                return IsLogged;

            //Отправляем пароль
            byte[] passBytes = System.Text.Encoding.UTF8.GetBytes(password);
            password = Convert.ToBase64String(passBytes);
            WriteToSocket(password + "\n");
            ReceiveFromSocket();
            answer = GetStringFromSocket();
            Debug.Log(answer);
            if (!answer[0].Equals('2'))
                return IsLogged;

            IsLogged = true;
            return IsLogged;
        }
        else
        {
            return false;
        }
    }

    //Функция проверки соединения
    public bool CheckConnection(string login, string password)
    {
        Connect();
        bool connection = StartSMTPSession(login, password);
        CloseSocket();
        return connection;
    }

    public void WriteALetter(string sendTo, string letter)
    {
        Messenger<string>.Broadcast(PostClientEvents.SHOW_DIALOG_WINDOW, "Отправка письма.");
        StartCoroutine(WriteALetterCoroutine(sendTo, letter));
    }

    public IEnumerator WriteALetterCoroutine(string sendTo, string letter)
    {
        Connect();
        if (StartSMTPSession(_lastLogin, _lastPassword) == false)
        {
            CloseSocket();
            Messenger<bool>.Broadcast(PostClientEvents.CONNECTION_PROBLEMS_WITH_TURNING_MESSAGE, false);
            Messenger<string>.Broadcast(PostClientEvents.UPDATE_STATUS, "Сбой соединения SMTP сокета");
        }
        else
        {
            WriteToSocket("MAIL FROM: " + _lastLogin + "\n");
            Debug.Log("MAIL FROM: " + _lastLogin + "\n");
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            WriteToSocket("RCPT TO: " + sendTo + "\n");
            Debug.Log("RCPT TO: " + sendTo + "\n");
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            WriteToSocket("DATA\n");
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            int symbolsInOnePacket = 1000;
            int numberOfStrings = letter.Length / symbolsInOnePacket;
            Debug.Log(numberOfStrings + " " + letter.Length);
            for (int i = 0; i < numberOfStrings; i++)
            {
                WriteToSocket(letter.Substring(i * symbolsInOnePacket, symbolsInOnePacket));
                if (i % 5 == 0) yield return 0;
            }
            int lastSymbols = letter.Length % symbolsInOnePacket;
            if (lastSymbols != 0)
                WriteToSocket(letter.Substring(numberOfStrings * symbolsInOnePacket, lastSymbols));
            Debug.Log("Waiting for receive");
            yield return 0;
            ReceiveFromSocket();
            Debug.Log(GetStringFromSocket());
            CloseSocket();

            Messenger.Broadcast(PostClientEvents.SMTP_CLEAR_INPUTS);
            Messenger.Broadcast(PostClientEvents.CLOSE_DIALOG_WINDOW);
        }
    }

    //Функция отправки сообщения на сокет
    private void WriteToSocket(string stringToSend)
    {
        WriteToSocket(System.Text.Encoding.UTF8.GetBytes(stringToSend));
    }

    private void WriteToSocket(byte[] bytesToSend)
    {
        _smtpStream.Write(bytesToSend, 0, bytesToSend.Length);
    }

    //Функция чтения строки из сокета
    private void ReceiveFromSocket()
    {
        Array.Clear(_buff, 0, _buff.Length);
        _numberOfBytesInLastReading = _smtpStream.Read(_buff, 0, _buff.Length);
    }

    //Функция перевода байтов в буфере в строку по количеству принятых байтов
    private string GetStringFromSocket()
    {
        return System.Text.Encoding.UTF8.GetString(_buff, 0, _numberOfBytesInLastReading);
    }

    //Функция закрытия сокета
    public void CloseSocket()
    {
        if (IsConnected)
        {
            if (IsLogged)
            {
                IsWorking = true;
                WriteToSocket("RSET\n");
                ReceiveFromSocket();
                WriteToSocket("QUIT\n");
                ReceiveFromSocket();
                IsWorking = false;
                IsLogged = false;
            }
            _smtpStream.Close();
            _smtpClient.Close();
        }
    }
}
