using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Multiplayer_Games_Programming_Packet_Library;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Multiplayer_Games_Programming_Server
{
	internal class Server
	{
		TcpListener m_TcpListener;
		int m_Index = 0;
		int playerOneID;
		int playerTwoID;

		ConcurrentDictionary<int, ConnectedClient> m_Clients;

		RSACryptoServiceProvider m_RsaProvider;
        public RSAParameters m_PublicKey;
        RSAParameters m_PrivateKey;

        public Server(string ipAddress, int port)
		{
			IPAddress ip = IPAddress.Parse(ipAddress);
			m_TcpListener = new TcpListener(ip, port);
			m_Clients = new ConcurrentDictionary<int, ConnectedClient>();

            m_RsaProvider = new RSACryptoServiceProvider(2048); 
            m_PublicKey = m_RsaProvider.ExportParameters(false); 
            m_PrivateKey = m_RsaProvider.ExportParameters(true); 
        }

		public void Start()
		{
			int incNum = 0;
			try
			{
				int dictionaryPosition = 0;
				Console.WriteLine("Server Started....");
				while (true)
				{
					m_TcpListener.Start();
					Socket socket = m_TcpListener.AcceptSocket();
					Console.WriteLine("Connection has been made");
					ConnectedClient client = new ConnectedClient(socket, incNum);
					m_Clients.TryAdd(dictionaryPosition, client);
					int i = dictionaryPosition;
					incNum++;
					Thread t = new Thread(() => ClientMethod(i));
					t.Start();
					dictionaryPosition++;
                }
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Stop()
		{
			m_TcpListener?.Stop();
		}

		private void ClientMethod(int id)
		{
			try
			{
                LoginPacket loginPacket = new LoginPacket(id, m_PublicKey);
                m_Clients[id].Send(loginPacket, m_RsaProvider, false);
                if (m_Index <= 1) { ConfirmedConnection cc = new ConfirmedConnection(m_Index); m_Clients[id].Send(cc, m_RsaProvider, false); m_Index += 1; }

				string? message;

				while ((message = m_Clients[id].Read()) != null)
				{
					Packet packet = Packet.Deserialise(message);
					if (packet == null) { continue; }

					HandleTCPPacket(id, packet);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				m_Clients[id].Close();
			}
		}	
		private void HandleTCPPacket(int id, Packet packet)
		{
			switch (packet.m_PacketType)
			{
				case PacketType.EncryptedPacket:
					EncryptedPacket ep = (EncryptedPacket)packet;
					Packet? DecryptedPacket = m_Clients[id].Decrypt(ep.m_encryption,m_PrivateKey, m_RsaProvider);

					if (DecryptedPacket == null) return;

					HandleTCPPacket(id, DecryptedPacket);

					break;
				case PacketType.LoginPacket:
					LoginPacket lp = (LoginPacket)packet;
					m_Clients[id].m_publicKey = lp.m_key;
					break;
				case PacketType.ConfirmedConnection:
					break;
				case PacketType.Message:
					MessagePacket mp = (MessagePacket)packet;
                    Console.WriteLine("Received Message - " + mp.m_Message);
                    break;
				case PacketType.PlayerPaddle:
					PlayerPaddle pp = (PlayerPaddle)packet;
					foreach(ConnectedClient client in m_Clients.Values)
					{
						if (client.id != id)
							client.Send(pp, m_RsaProvider);
					}
					break;
				case PacketType.Ball:
                    BallPacket bp = (BallPacket)packet;
                    foreach (ConnectedClient client in m_Clients.Values)
                    {
                        if (client.id != id)
                            client.Send(bp, m_RsaProvider);
                    }
                    break;
				default:
					break;
			}
		}
	}		
}
