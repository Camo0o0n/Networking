using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Packet_Library;
using System;
using Myra.Utility;

namespace Multiplayer_Games_Programming_Framework
{
	internal class BallControllerComponent : Component
	{
		float m_Speed;
		Vector2 m_InitDirection;
		Rigidbody m_Rigidbody;
        int m_Index;
        public BallControllerComponent(GameObject gameObject, int index) : base(gameObject)
		{
            m_Index = index;
        }

		protected override void Start(float deltaTime)
		{
			m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
            NetworkManager.m_Instance.m_BallPosition.Add(0, UpdateVectorPosition);
            NetworkManager.m_Instance.m_BallLinearVelocity.Add(0, UpdateDirection);
        }
		public void Init(float speed, Vector2 direction)
		{
			m_Speed = speed;
			m_InitDirection = direction;
		}

		public void UpdateDirection(Vector2 LinearVelocity)
		{
			m_Rigidbody.m_Body.LinearVelocity = LinearVelocity;
		}

		public void UpdateVectorPosition(Vector2 position)
		{
			m_Rigidbody.UpdatePosition(position);
		}

		public void StartBall()
		{
			m_Rigidbody.m_Body.LinearVelocity = (m_InitDirection * m_Speed);
		}

		protected override void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
		{
			Vector2 normal = contact.Manifold.LocalNormal;
			Vector2 velocity = m_Rigidbody.m_Body.LinearVelocity;
			Vector2 reflection =  Vector2.Reflect(velocity, normal);
			m_Rigidbody.m_Body.LinearVelocity = reflection * 1.0f;
			//if (m_Index == 0)
			//{
				BallPacket ball = new BallPacket(NetworkManager.m_Instance.m_Index, m_Rigidbody.m_Transform.Position.X, m_Rigidbody.m_Transform.Position.Y, reflection.X * 1.0f, reflection.Y * 1.0f);
				NetworkManager.m_Instance.TCPSendMessage(ball);
			//}

		}
	}
}
