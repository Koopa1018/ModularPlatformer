using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using Clouds.Movement2D;

namespace Clouds.Platformer.Forces {
	[System.Obsolete("Directional Forces has been folded into AcceptsForces and should no longer be referenced directly.")]
	public class DirectionalForces : MonoBehaviour {
		float2 addedForce, addedForceWithDT = 0;

		public float2 ForceRaw {get => addedForce;}
		public float2 ForceWithDeltaTime {get => addedForceWithDT;}

		public void AcceptForce (float2 force, float timestep) {
			Debug.LogError("Calling obsolete AcceptForce method; please call on AcceptsForces instead.", this);
			addedForce += force;
			addedForceWithDT += force * timestep;
		}
		public void AcceptForce (float forceX, float forceY, float timestep) {
			Debug.LogError("Calling obsolete AcceptForce method; please call on AcceptsForces instead.", this);
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
