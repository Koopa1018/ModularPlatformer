using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace Clouds.Platformer.Character {
	[CustomEditor(typeof(PlatformerSlopeCollisions), true)]
	public class PlatformerSlopeCollisionsEditor : Editor {
		PlatformerSlopeCollisions readMe;
		SerializedProperty showDebugInfo;

		void OnEnable () {
			readMe = target as PlatformerSlopeCollisions;
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.FloatField("This Angle", readMe.angle);
			EditorGUILayout.FloatField("Last Angle", readMe.lastAngle);
			EditorGUILayout.Vector2Field("This Normal", readMe.normal);

			EditorGUILayout.Space();

			EditorGUILayout.EnumPopup("Direction On Slope", readMe.directionOnSlope);

			EditorGUI.EndDisabledGroup();
		}

		public override bool RequiresConstantRepaint() {
			return EditorApplication.isPlaying;
		}

	}
}