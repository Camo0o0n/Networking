using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Security.Cryptography; //This is the namespace that contains RSA Encryption
using Multiplayer_Games_Programming_Packet_Library;
using System.IO;
using System;

namespace Multiplayer_Games_Programming_Server
{
	internal class ConnectedClient
	{
		Socket socket;
		NetworkStream stream;
		StreamReader reader;
		StreamWriter writer;
		public int id;

		public RSAParameters m_publicKey;

		public ConnectedClient(Socket socket, int id)
		{
			this.socket = socket;
			this.id = id;
            stream = new NetworkStream(socket, false);
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);
        }

		public void Close()
		{
			socket.Close();
        } 
        public string Read()	
		{
			string rMessage = reader.ReadLine();
			Console.WriteLine(rMessage);
			Packet? a1 = Packet.Deserialise(rMessage);
			return rMessage;
		}

		public void Send(Packet Packet, RSACryptoServiceProvider m_RsaProvider ,bool sendEncrypted = true)
		{
            string messagePacketJson;
            if (sendEncrypted)
            {
                messagePacketJson = new EncryptedPacket(id, Encrypt(Packet, m_RsaProvider)).ToJson();
            }
            else { messagePacketJson = Packet.ToJson(); }
            writer.WriteLine(messagePacketJson);
            writer.Flush();
        }

		public byte[] Encrypt(Packet packet, RSACryptoServiceProvider m_RsaProvider) 
		{
			lock (m_RsaProvider)
			{
				m_RsaProvider.ImportParameters(m_publicKey);
				string json = packet.ToJson();
				byte[] encrypted = m_RsaProvider.Encrypt(Encoding.UTF8.GetBytes(json), false);
				return encrypted;
			}
		}

		public Packet Decrypt(byte[] packet, RSAParameters serverPrivateKey, RSACryptoServiceProvider m_RsaProvider)
		{
			lock (m_RsaProvider)
			{
				m_RsaProvider.ImportParameters(serverPrivateKey);
                byte[] decrypted = m_RsaProvider.Decrypt(packet, false);
                string json = Encoding.UTF8.GetString(decrypted);
                Packet decryptPacket = Packet.Deserialise(json);
				return decryptPacket;
            }
		}
	}
}
