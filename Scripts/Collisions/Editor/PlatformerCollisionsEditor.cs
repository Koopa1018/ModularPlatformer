using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Clouds.Platformer.Character {
	[CustomEditor(typeof(PlatformerCollisions), true)]
	public class PlatformerCollisionsEditor : Editor {
		PlatformerCollisions readMe;
		SerializedProperty showDebugInfo;

		void OnEnable () {
			readMe = target as PlatformerCollisions;
		}

		public override void OnInspectorGUI () {
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.LabelField("Hitting things on these sides:");
			//TODO: This should be a nicer shape.
			EditorGUILayout.Toggle("Above", readMe.Above);
			EditorGUILayout.Toggle("Left", readMe.Left);
			EditorGUILayout.Toggle("Right", readMe.Right);
			EditorGUILayout.Toggle("Below", readMe.Below);

			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Things hit me on these sides:");
			EditorGUILayout.Toggle("Above", readMe.HitAbove);
			EditorGUILayout.Toggle("Left", readMe.HitLeft);
			EditorGUILayout.Toggle("Right", readMe.HitRight);
			EditorGUILayout.Toggle("Below", readMe.HitBelow);

			EditorGUI.EndDisabledGroup();
		}

		public override bool RequiresConstantRepaint() {
			return EditorApplication.isPlaying;
		}

	}
}