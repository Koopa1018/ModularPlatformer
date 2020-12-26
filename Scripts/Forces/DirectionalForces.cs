using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using Clouds.Movement2D;

namespace Clouds.Platformer.Forces {
	public class DirectionalForces : MonoBehaviour {
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

		public void ClearForce () => addedForce = addedForceWithDT = 0;
		public void ClearForceX () => addedForce.x = addedForceWithDT.x = 0;
		public void ClearForceY () => addedForce.y = addedForceWithDT.y = 0;
	}
}
