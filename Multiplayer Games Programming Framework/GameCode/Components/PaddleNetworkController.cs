using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Multiplayer_Games_Programming_Framework.Core;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components
{
	internal class PaddleNetworkController : Component
	{
		int m_Index;
		float m_Speed;
		int direction;
		Rigidbody m_Rigidbody;
		Vector2 Location;
		Vector2 input = Vector2.Zero;
        bool m_PosIsDirty = false;

        public PaddleNetworkController(GameObject gameObject, int index) : base(gameObject)
		{
			m_Index = index;
			m_Speed = 10;
		}



		protected override void Start(float deltaTime)
		{
			m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
			NetworkManager.m_Instance.m_RemotePaddleActions.Add(m_Index, UpdateDirection);
			NetworkManager.m_Instance.m_RemotePaddlePositions.Add(m_Index, UpdateVectorPosition);
		}


        public override void Destroy()
        {
            NetworkManager.m_Instance.m_RemotePaddleActions.Remove(m_Index);
            NetworkManager.m_Instance.m_RemotePaddlePositions.Remove(m_Index);

            base.Destroy();
        }


        public void UpdateDirection(int direction)
		{

			this.direction = direction;
			if (direction == 0) { input.Y = 0; /*m_Rigidbody.UpdatePosition(Location);*/ }
			else { input.Y += direction; /*m_Rigidbody.UpdatePosition(Location);*/ }

		}

		public void UpdateVectorPosition(Vector2 position)
		{			
			Location = position;
			m_PosIsDirty = true;
		}

		protected override void Update(float deltaTime)
		{
			if (m_PosIsDirty)
			{
				m_Rigidbody.UpdatePosition(Location);
				m_PosIsDirty = false;
				m_Rigidbody.m_Body.LinearVelocity = (m_Transform.Up * input.Y * m_Speed);
			}
			else
			{
                m_Rigidbody.m_Body.LinearVelocity = (m_Transform.Up * input.Y * m_Speed);
            }
            
        }
	}
}
