using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Mathematics;

namespace Clouds.Platformer.Character {
	[AddComponentMenu("Hidden/Platformer/Collision/4-Directional Collision State")]
	[DefaultExecutionOrder(2)]
	public class PlatformerCollisions : MonoBehaviour {
		bool4 _state;
		public bool Left {
			get {
				return _state.x;
			} set {
				_state.x = value;
			}
		}
		public bool Right {
			get {
				return _state.y;
			} set {
				_state.y = value;
			}
		}
		public bool Above {
			get {
				return _state.z;
			} set {
				_state.z = value;
			}
		}
		public bool Below {
			get {
				return _state.w;
			} set {
				_state.w = value;
			}
		}

		bool4 _hitByOther;
		/// <summary>
		/// Did an object collide with me on the left?
		/// </summary>
		public bool HitLeft {
			get {
				return _hitByOther.x;
			} set {
				_hitByOther.x = value;
			}
		}
		/// <summary>
		/// Did an object collide with me on the right?
		/// </summary>
		public bool HitRight {
			get {
				return _hitByOther.y;
			} set {
				_hitByOther.y = value;
			}
		}
		/// <summary>
		/// Did an object collide with me from above?
		/// </summary>
		public bool HitAbove {
			get {
				return _hitByOther.z;
			} set {
				_hitByOther.z = value;
			}
		}
		/// <summary>
		/// Did an object collide with me from below?
		/// </summary>
		public bool HitBelow {
			get {
				return _hitByOther.w;
			} set {
				_hitByOther.w = value;
			}
		}

		public void Clear () {
			_hitByOther = _state = false;
		}

	}
}