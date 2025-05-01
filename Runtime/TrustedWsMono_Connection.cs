using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Eloi.TrustedWss {


    public class TrustedWsMono_Connection : MonoBehaviour
    {
        public string m_serverUrl;
        public TrustedWs_Connection m_connection;

        void Awake()
        {
            m_connection = new TrustedWs_Connection(m_serverUrl);
        }

        void Update()
        {
            m_connection?.Update();
        }

        [ContextMenu("SetServerAsApintDefaultWSS")]
        public void SetServerAsApintDefaultWSS()
        {
            m_serverUrl = "wss://apint.ddns.net:4725";
        }
        [ContextMenu("SetServerAsApintDefaultWS")]
        public void SetServerAsApintDefaultWS()
        {
            m_serverUrl = "ws://apint.ddns.net:4625";
        }

        public void CloseAndRelanch()
        {
            m_connection?.CloseConnection();
            m_connection = new TrustedWs_Connection(m_serverUrl);
        }

        public void PushBytes(byte[] bytes)
        {
            m_connection.PushBytesToSend(bytes);
        }
        public void PushBytes(byte byteValue)
        {
            m_connection.PushBytesToSend(new byte[] { byteValue });
        }

        public void PushStringUTF8(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            m_connection.PushBytesToSend(bytes);
        }
        public void PushChar(char character)
        {
            m_connection.PushBytesToSend(BitConverter.GetBytes(character));
        }

        public void PushInteger(int value)
        {
            byte[] bytes = new byte[4];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            m_connection.PushBytesToSend(bytes);
        }

        public void PushIndexInteger(int index, int value)
        {
            byte[] bytes = new byte[8];
            BitConverter.GetBytes(index).CopyTo(bytes, 0);
            BitConverter.GetBytes(value).CopyTo(bytes, 4);
            m_connection.PushBytesToSend(bytes);
        }
        public void PushIndexIntegerDate(int index, int value, ulong date)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(index).CopyTo(bytes, 0);
            BitConverter.GetBytes(value).CopyTo(bytes, 4);
            BitConverter.GetBytes(date).CopyTo(bytes, 8);
            m_connection.PushBytesToSend(bytes);
        }
        public void PushIndexIntegerDate( int value, ulong date)
        {
            byte[] bytes = new byte[12];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            BitConverter.GetBytes(date).CopyTo(bytes, 4);
            m_connection.PushBytesToSend(bytes);
        }
    }

    [System.Serializable]
    public class TrustedWs_Connection 
    {

        public WssTrustedWebsocketIID m_client = null;

        public UnityEvent<byte[]> m_onBytesReceived;
        public UnityEvent<string> m_onStringReceived;
        public ulong m_bytesReceivedCount = 0;
        public ulong m_stringCharReceivedCount = 0;

        public ulong m_pushBytesCount = 0;
        public ulong m_pushStringCharCount = 0;


      


        public void Update()
        {

            m_client.FlushReceivedWaitingInQueue((bytes) =>
            {
                if (bytes == null) return;
                if (bytes.Length <= 0) return;

                m_bytesReceivedCount += (ulong)bytes.Length;
                m_onBytesReceived?.Invoke(bytes);
            });
            m_client.FlushReceivedWaitingInQueue((str) =>
            {
                if (string.IsNullOrEmpty(str)) return;
                
                m_stringCharReceivedCount += (ulong)str.Length;
                m_onStringReceived?.Invoke(str);
            });
        }

        
        public TrustedWs_Connection(string serverUrl)
        {
            m_client = new WssTrustedWebsocketIID(false);
            m_client.SetServerUrl(serverUrl);
            m_client.StartConnectionThread();
            m_client.m_onPrint+= (str =>
            {
                Debug.Log(str);
            });
        }


        public void PushBytesToSend(byte[] bytes)
        {
            if (bytes == null) return;
            m_pushBytesCount += (ulong)bytes.Length;
            m_client.AddQueueBytes(bytes);
        }
        public void PushStringToSend(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            m_pushStringCharCount += (ulong)text.Length;

            m_client.AddToQueueString(text);
        }

        public void CloseConnection()
        {
            m_client.m_kill = true;
            m_client.m_clientWebSocket?.Dispose();
            m_client.m_clientWebSocket = null;
            m_client = null;
        }
    }

}