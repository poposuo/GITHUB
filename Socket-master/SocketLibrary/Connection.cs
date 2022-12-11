using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace SocketLibrary
{
    /// <summary>
    /// Connection ��ժҪ˵����
    /// </summary>
    public class Connection
    {
        #region ����

        private NetworkStream _networkStream;
        /// <summary>
        ///  �ṩ����������ʵĻ���������
        /// </summary>
        public NetworkStream NetworkStream
        {
            get { return _networkStream; }
            private set { _networkStream = value; }
        }
        private string _connectionName;
        /// <summary>
        /// ���ӵ�Key
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            private set { _connectionName = value; }
        }
        private string _nickName;
        /// <summary>
        /// ���ӱ���
        /// </summary>
        public string NickName
        {
            get { return _nickName; }
            set { _nickName = value; }
        }
        /// <summary>
        /// �����ӵ���Ϣ����
        /// </summary>
        public ConcurrentQueue<Message> messageQueue
        {
            get { return _messageQueue; }
            private set { _messageQueue = value; }
        }
        protected ConcurrentQueue<Message> _messageQueue;

        private TcpClient _tcpClient;
        /// <summary>
        /// TcpClient
        /// </summary>
        public TcpClient TcpClient
        {
            get { return _tcpClient; }
            private set { _tcpClient = value; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient">�ѽ������ӵ�TcpClient</param>
        /// <param name="connectionName">������</param>
        public Connection(TcpClient tcpClient, string connectionName)
        {
            this._tcpClient = tcpClient;
            this._connectionName = connectionName;
            this.NickName = this._connectionName.Split(':')[0];
            this._networkStream = this._tcpClient.GetStream();
            this._messageQueue = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// �ж����Ӳ��ͷ���Դ
        /// </summary>
        public void Stop()
        {
            _tcpClient.Client.Disconnect(false);
            _networkStream.Close();
            _tcpClient.Close();
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <returns></returns>
        public Message Parse()
        {
            Message message = new Message();
            //�ȶ���ǰ�ĸ��ֽڣ���Message����
            byte[] buffer = new byte[4];
            if (this._networkStream.DataAvailable)
            {
                int count = this._networkStream.Read(buffer, 0, 4);
                if (count == 4)
                {
                    message.MessageLength = BitConverter.ToInt32(buffer, 0);
                }
                else
                    throw new Exception("���������Ȳ���ȷ");
            }
            else
                throw new Exception("Ŀǰ���粻�ɶ�");
            //������Ϣ�������ֽ�
            buffer = new byte[message.MessageLength - 4];
            if (this._networkStream.DataAvailable)
            {
                int count = this._networkStream.Read(buffer, 0, buffer.Length);
                if (count == message.MessageLength - 4)
                {
                    message.Command = (Message.CommandType)buffer[0];
                    message.MainVersion = buffer[1];
                    message.SecondVersion = buffer[2];

                    //������Ϣ��
                    message.MessageBody = SocketFactory.DefaultEncoding.GetString(buffer, 3, buffer.Length - 3);
                    message.ConnectionName = this._connectionName;

                    return message;
                }
                else
                    throw new Exception("���������Ȳ���ȷ");
            }
            else
                throw new Exception("Ŀǰ���粻�ɶ�");
        }


    }
}
