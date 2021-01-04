using UnityEngine;
using Clouds.MovementBase;
using Clouds.Platformer.Character;

namespace Clouds.Platformer.Forces {
	[AddComponentMenu("Platformer/Forces/Accepts Forces")]
	public class AcceptsForces : MonoBehaviour {
		[Header("Inputs")]
		[SerializeField] DirectionalForces forcesToApply;
		
		[Header("Outputs")]
		[SerializeField] ScalarVelocity xVelocity;
		[SerializeField] ScalarVelocity yVelocity;

		void OnEnable () {
			//Reset added force on enable.
			forcesToApply.ClearForce();
		}

		void FixedUpdate () {
			//Apply added forces to horizontal velocity
			xVelocity.TerminalVelocity += forcesToApply.ForceRaw.y;
			xVelocity.Value += forcesToApply.ForceWithDeltaTime.y;
			//And to vertical velocity.
			yVelocity.TerminalVelocity += forcesToApply.ForceRaw.y;
			yVelocity.Value += forcesToApply.ForceWithDeltaTime.y;
			
			//Don't forget to absorb the force after using it.
			forcesToApply.ClearForce();
		}

		float2 addedForce, addedForceWithDT = 0;

		public float2 ForceRaw {get => addedForce;}
		public float2 ForceWithDeltaTime {get => addedForceWithDT;}

		public void AcceptForce (float2 force, float timestep) {
			addedForce += force;
			addedForceWithDT += force * timestep;
		}
		public void AcceptForce (float forceX, float forceY, float timestep) {
			AcceptForce(new float2(forceX, forceY), timestep);
		}

		public void AcceptForceX (float forceX, float timestep) => AcceptForce(forceX, 0, timestep);
		public void AcceptForceY (float forceY, float timestep) => AcceptForce(0, forceY, timestep);

		public void AcceptForce (float2 force) => AcceptForce(force, Time.fixedDeltaTime);
		public void AcceptForce (float forceX, float forceY) => AcceptForce(forceX, forceY, Time.fixedDeltaTime);

		public void AcceptForceX (float forceX) => AcceptForceX(forceX, Time.fixedDeltaTime);
		public void AcceptForceY (float forceY) => AcceptForceY(forceY, Time.fixedDeltaTime);
		

		public void ClearForce () => addedForce = addedForceWithDT = 0;
		public void ClearForceX () => addedForce.x = addedForceWithDT.x = 0;
		public void ClearForceY () => addedForce.y = addedForceWithDT.y = 0;
	}
}