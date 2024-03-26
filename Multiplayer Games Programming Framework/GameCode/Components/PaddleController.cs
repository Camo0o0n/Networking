using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Packet_Library;


namespace Multiplayer_Games_Programming_Framework
{
	internal class PaddleController : Component
	{
		float m_Speed;
		Rigidbody m_Rigidbody;
		bool upBool = true;
		bool downBool = true;
		bool noPressBool = true;
		Vector2 input = Vector2.Zero;
		public PaddleController(GameObject gameObject) : base(gameObject)
		{
			m_Speed = 10;
		}

		protected override void Start(float deltaTime)
		{
			m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
		}

        protected override void Update(float deltaTime)
		{

			if (Keyboard.GetState().IsKeyDown(Keys.Up) && upBool) 
			{
				noPressBool = false;
				upBool = false;
				input.Y += -1;
				PlayerPaddle paddle = new PlayerPaddle(-1, NetworkManager.m_Instance.m_Index, m_Transform.Position.X, m_Transform.Position.Y);
				NetworkManager.m_Instance.TCPSendMessage(paddle);
			}
			else if (Keyboard.GetState().IsKeyUp(Keys.Up) && !upBool) { upBool = true; input.Y += 1; PlayerPaddle paddle = new PlayerPaddle(1, NetworkManager.m_Instance.m_Index, m_Transform.Position.X, m_Transform.Position.Y);
                NetworkManager.m_Instance.TCPSendMessage(paddle);
            }

			if (Keyboard.GetState().IsKeyDown(Keys.Down) && downBool) 
			{
				noPressBool = false;
				downBool = false;
				input.Y += 1;
                PlayerPaddle paddle = new PlayerPaddle(1, NetworkManager.m_Instance.m_Index, m_Transform.Position.X, m_Transform.Position.Y);
                NetworkManager.m_Instance.TCPSendMessage(paddle);
            }
			else if (Keyboard.GetState().IsKeyUp(Keys.Down) && !downBool) { downBool = true; input.Y += -1; PlayerPaddle paddle = new PlayerPaddle(-1, NetworkManager.m_Instance.m_Index, m_Transform.Position.X, m_Transform.Position.Y);
                NetworkManager.m_Instance.TCPSendMessage(paddle);
            }

			if (downBool && upBool && !noPressBool)
			{
				noPressBool = true;
				PlayerPaddle paddle = new PlayerPaddle(0, NetworkManager.m_Instance.m_Index, m_Transform.Position.X, m_Transform.Position.Y);
				NetworkManager.m_Instance.TCPSendMessage(paddle);
			}

			m_Rigidbody.m_Body.LinearVelocity = (m_Transform.Up * input.Y * m_Speed);
		}
	}
}