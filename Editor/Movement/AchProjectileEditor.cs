using UnityEditor;
using UnityEngine;

namespace AchEngine.Editor
{
    [CustomEditor(typeof(AchProjectile))]
    public class AchProjectileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var typeProp   = serializedObject.FindProperty("Type");
            var speedProp  = serializedObject.FindProperty("MoveSpeed");
            var dirProp    = serializedObject.FindProperty("Direction");
            var targetProp = serializedObject.FindProperty("Target");
            var turnProp   = serializedObject.FindProperty("TurnSpeed");

            // ── 공통 ──────────────────────────────────────────────────────
            EditorGUILayout.LabelField("공통", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(typeProp,  new GUIContent("Type"));
            EditorGUILayout.PropertyField(speedProp, new GUIContent("Move Speed"));

            EditorGUILayout.Space(6);

            // ── 타입별 필드 ───────────────────────────────────────────────
            var type = (ProjectileType)typeProp.enumValueIndex;
            switch (type)
            {
                case ProjectileType.Straight:
                    EditorGUILayout.LabelField("Straight", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(dirProp, new GUIContent("Direction"));
                    break;

                case ProjectileType.Homing:
                    EditorGUILayout.LabelField("Homing", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(targetProp, new GUIContent("Target"));
                    EditorGUILayout.PropertyField(turnProp,   new GUIContent("Turn Speed"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
