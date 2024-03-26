using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Numerics;

namespace Multiplayer_Games_Programming_Packet_Library
{
	public enum PacketType
		{
        ConfirmedConnection,
        
		Message,
        PlayerPaddle,
        Ball,
        EncryptedPacket,
        GameStateUpdate,
        LoginPacket,
    }
	public class Packet
	{
		[JsonPropertyName("Type")]
		public PacketType m_PacketType { get; set; }


        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new PacketConverter() },
                IncludeFields = true,
            };

            return JsonSerializer.Serialize(this, options);
        }
        public static Packet? Deserialise(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new PacketConverter() },
                IncludeFields = true,
            };

            return JsonSerializer.Deserialize<Packet>(json, options);
        }
    }

    public class ConfirmedConnection : Packet
    {
        [JsonPropertyName("ConfirmedConnection")]

        public int m_index { get; set; }

        public ConfirmedConnection()
        {
            m_PacketType = PacketType.ConfirmedConnection;
        }

        public ConfirmedConnection(int index)
        {
            m_PacketType = PacketType.ConfirmedConnection;
            m_index = index;
        }
    }

    public class MessagePacket : Packet
    {
        [JsonPropertyName("Message")]
        public string m_Message { get; set; }

        public MessagePacket()
        {
            m_PacketType = PacketType.Message;
        }

        public MessagePacket(string Message)
        {
            m_PacketType = PacketType.Message;
            m_Message = Message;
            

        }
    }

    public class PlayerPaddle : Packet
    {
        [JsonPropertyName("PlayerPaddle")]
        public int m_direction { get; set; }
        public int m_id { get; set; }
        public float m_positionX { get; set; }
        public float m_positionY { get; set; }

        public PlayerPaddle()
        {
            m_PacketType = PacketType.PlayerPaddle;
        }

        public PlayerPaddle(int direction, int id, float positionX, float positionY)
        {
            m_PacketType = PacketType.PlayerPaddle;
            m_direction = direction;
            m_id = id;
            m_positionX = positionX;
            m_positionY = positionY;

        }
    }

    public class BallPacket : Packet
    {
        [JsonPropertyName("Ball")]
        public int m_id { get; set; }
        public float m_ballPositionX { get; set; }
        public float m_ballPositionY { get; set; }
        public float m_ballLinearVelocityX { get; set; }
        public float m_ballLinearVelocityY { get; set; }

        public BallPacket()
        {
            m_PacketType = PacketType.Ball;
        }

        public BallPacket(int id, float ballPositionX, float ballPositionY, float ballVelocityX, float ballVelocityY)
        {
            m_id = id;
            m_PacketType = PacketType.Ball;
            m_ballPositionX = ballPositionX;
            m_ballPositionY = ballPositionY;
            m_ballLinearVelocityX = ballVelocityX;
            m_ballLinearVelocityY = ballVelocityY;
        }
    }

    public class EncryptedPacket : Packet
    {
        [JsonPropertyName("Encryption")]
        public byte[] m_encryption { get; set; }
        public int m_id { get; set; }

        public EncryptedPacket()
        {
            m_PacketType = PacketType.EncryptedPacket;
            m_encryption = new byte[0];
        }

        public EncryptedPacket(int id, byte[] encryption)
        {
            m_PacketType = PacketType.EncryptedPacket;
            m_id = id;
            m_encryption = encryption;
            
        }
    }

    public class LoginPacket : Packet
    {
        public RSAParameters m_key;
        public int m_id { get; set; }

        public LoginPacket()
        {
            m_PacketType = PacketType.LoginPacket;
        }

        public LoginPacket(int id, RSAParameters key)
        {
            m_key = key;
            m_id = id;
            m_PacketType = PacketType.LoginPacket;
        }
    }

    class PacketConverter : JsonConverter<Packet>
    {
        public override Packet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("Type", out var typeProperty))
                {                    
                    if (typeProperty.GetByte() == (byte)PacketType.ConfirmedConnection)
                    {
                        return JsonSerializer.Deserialize<ConfirmedConnection>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.Message)
                    {
                        return JsonSerializer.Deserialize<MessagePacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.PlayerPaddle)
                    {
                        return JsonSerializer.Deserialize<PlayerPaddle>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.Ball)
                    {
                        return JsonSerializer.Deserialize<BallPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.EncryptedPacket)
                    {
                        return JsonSerializer.Deserialize<EncryptedPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.LoginPacket)
                    {
                        return JsonSerializer.Deserialize<LoginPacket>(root.GetRawText(), options);
                    }
                }
            }

            throw new JsonException("Unknown type");
        }

        public override void Write(Utf8JsonWriter writer, Packet value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}