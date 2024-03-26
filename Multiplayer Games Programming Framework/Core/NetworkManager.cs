using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Multiplayer_Games_Programming_Packet_Library;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace Multiplayer_Games_Programming_Framework.Core
{
	internal class NetworkManager
	{
		private static NetworkManager Instance;

		public static NetworkManager m_Instance
		{
			get
			{
				if (Instance == null)
				{
					return Instance = new NetworkManager();
				}
			
				return Instance;
			}
		}

		TcpClient m_TcpClient;
		NetworkStream m_stream;
		StreamReader m_StreamReader;
		StreamWriter m_StreamWriter;
        public int m_Index;

        RSACryptoServiceProvider m_RsaProvider;
		RSAParameters m_ServerPublicKey;
        public RSAParameters m_PublicKey;
        RSAParameters m_PrivateKey;

        public Dictionary<int, Action<int>> m_RemotePaddleActions;
        public Dictionary<int, Action<Vector2>> m_RemotePaddlePositions;
		public Dictionary<int, Action<Vector2>> m_BallPosition;
		public Dictionary<int, Action<Vector2>> m_BallLinearVelocity;

        NetworkManager()
		{
			m_TcpClient = new TcpClient();
			m_RemotePaddleActions = new Dictionary<int, Action<int>>();
            m_RemotePaddlePositions = new Dictionary<int, Action<Vector2>>();
			m_BallPosition = new Dictionary<int, Action<Vector2>>();
			m_BallLinearVelocity = new Dictionary<int, Action<Vector2>>();
        }

		public bool Connect(string ip, int port)
		{
			try
			{
				m_TcpClient.Connect(ip, port);
				m_stream = m_TcpClient.GetStream();
				m_StreamReader = new StreamReader(m_stream, Encoding.UTF8);
				m_StreamWriter = new StreamWriter(m_stream, Encoding.UTF8);
				Run();
				return true;
			}
			catch(Exception ex) 
			{
				Debug.WriteLine(ex.Message);
			}
			return false;
		}

		public void Run()
		{
			Thread TcpThread = new Thread(new ThreadStart(TcpProcessServerResponse));
			TcpThread.Name = "TCP Thread";
            TcpThread.Start();

            m_RsaProvider = new RSACryptoServiceProvider(2048);
            m_PublicKey = m_RsaProvider.ExportParameters(false);
            m_PrivateKey = m_RsaProvider.ExportParameters(true);

			LoginPacket loginPacket = new LoginPacket(m_Index, m_PublicKey);
			TCPSendMessage(loginPacket, false);
        }

		private void TcpProcessServerResponse()
		{
			//try
			//{
				while (m_TcpClient.Connected)
				{
					string message = m_StreamReader.ReadLine();
                    Packet packet = Packet.Deserialise(message);
                    if (packet == null)

                        continue;
					HandleTCPPacket(m_Index, packet);
				}
			//}
			/*catch (Exception ex) 
			{ 
				Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" + ex.Message);
			}*/
		}

		private void HandleTCPPacket(int id, Packet packet)
		{
			switch (packet.m_PacketType)
			{
				case PacketType.EncryptedPacket:
					EncryptedPacket ep = (EncryptedPacket)packet;
					Packet? DecryptedPacket = Decrypt(ep.m_encryption, m_RsaProvider);

					if (DecryptedPacket == null) return;

					HandleTCPPacket(id, DecryptedPacket);

					break;
				case PacketType.LoginPacket:
					LoginPacket lp = (LoginPacket)packet;
					m_ServerPublicKey = lp.m_key;
					break;
                case PacketType.ConfirmedConnection:
                    ConfirmedConnection cc = (ConfirmedConnection)packet;
                    m_Instance.m_Index = cc.m_index;
                    break;
                case PacketType.Message:
                    MessagePacket mp = (MessagePacket)packet;
                    Debug.WriteLine("Received Message - " + mp.m_Message);
                    break;
                case PacketType.PlayerPaddle:
                    PlayerPaddle pp = (PlayerPaddle)packet;
                    m_RemotePaddlePositions[pp.m_id]?.Invoke(new Vector2(pp.m_positionX, pp.m_positionY));
                    m_RemotePaddleActions[pp.m_id]?.Invoke(pp.m_direction);

                    break;
                case PacketType.Ball:
                    try
                    {
                        BallPacket bp = (BallPacket)packet;
                        m_BallPosition[0]?.Invoke(new Vector2(bp.m_ballPositionX, bp.m_ballPositionY));
                        m_BallLinearVelocity[0]?.Invoke(new Vector2(bp.m_ballLinearVelocityX, bp.m_ballLinearVelocityY));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Ball had a bad time.");
                    }

                    break;
                default:
                    break;
            }
		}
			
		public void TCPSendMessage(Packet Message, bool sendEncrypted = true)
		{
			string messagePacketJson;
			if (sendEncrypted) 
			{
				messagePacketJson = new EncryptedPacket(m_Index, Encrypt(Message)).ToJson();
			}
			else { messagePacketJson = Message.ToJson(); }
            m_StreamWriter.WriteLine(messagePacketJson);
			m_StreamWriter.Flush();
		}

        public byte[] Encrypt(Packet packet)
        {
			lock(m_RsaProvider)
			{
				m_RsaProvider.ImportParameters(m_ServerPublicKey);
				string json = packet.ToJson();
				byte[] encrypted = m_RsaProvider.Encrypt(Encoding.UTF8.GetBytes(json), false);
				return encrypted;
			}
            
        }

        public Packet Decrypt(byte[] packet, RSACryptoServiceProvider m_RsaProvider)
        {
            lock (m_RsaProvider)
            {
                m_RsaProvider.ImportParameters(m_PrivateKey);
                byte[] decrypted = m_RsaProvider.Decrypt(packet, false); //decrypt the byte array
                string json = Encoding.UTF8.GetString(decrypted); //get the json string
                Packet decryptPacket = Packet.Deserialise(json); //Deserialise the json into an animal
                return decryptPacket;
            }
        }
        public void Login()
		{
            Packet Packet = new MessagePacket("Hello Server");
            TCPSendMessage(Packet, false);
		}
	}
}
