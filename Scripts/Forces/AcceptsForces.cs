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
	}
}