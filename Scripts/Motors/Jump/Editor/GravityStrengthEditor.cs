using UnityEditor;

namespace Clouds.Platformer.Character {
	[CustomEditor(typeof(GravityStrength), true)]
	public class GravityStrengthEditor : Editor {
		public override void OnInspectorGUI () {
			//Run main editor.
			base.OnInspectorGUI();

			//Separate main from debug info.
			//EditorGUILayout.Space();

			//Show debug info.
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.FloatField("Current Velocity", (target as GravityStrength).Value);
			EditorGUI.EndDisabledGroup();
		}

		public override bool RequiresConstantRepaint() {
			return EditorApplication.isPlaying;
		}

	}
}