using UnityEngine;
using Unity.Mathematics;
using Clouds.Movement2D;

namespace Clouds.Platformer.Character {
	public interface ILateralMoveSignal {
		float GetSignal (
			float velocity,
			float timestep,
			float rawSpeed,
			int facingDirection
		);
	}
}