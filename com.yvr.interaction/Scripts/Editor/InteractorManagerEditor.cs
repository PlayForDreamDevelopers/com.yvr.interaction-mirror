using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YVR.Interaction.Runtime
{
    [CustomEditor(typeof(InteractorManager))]
    public class InteractorManagerEditor : Editor
    {
        private SerializedProperty m_HMDProperty;
        private SerializedProperty m_EyeGazeProperty;

        private void OnEnable()
        {
            m_HMDProperty = serializedObject.FindProperty("m_HMD");
            m_EyeGazeProperty = serializedObject.FindProperty("m_EyeGaze");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject,m_HMDProperty.propertyPath,m_EyeGazeProperty.propertyPath);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_HMDProperty);
            if (m_HMDProperty.objectReferenceValue is GameObject hmd)
            {
                if (HasCameraInChildren(hmd.transform))
                {
                    EditorGUILayout.HelpBox("HMD has a camera, set hmd gaze game object", MessageType.Warning);
                }
            }

            EditorGUILayout.PropertyField(m_EyeGazeProperty);
            if (m_EyeGazeProperty.objectReferenceValue is GameObject eyegaze)
            {
                if (HasCameraInChildren(eyegaze.transform))
                {
                    EditorGUILayout.HelpBox("EyeGaze has a cameram, set eye gaze game object", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool HasCameraInChildren(Transform parent)
        {
            if (parent.GetComponent<Camera>() != null)
            {
                return true;
            }
            foreach (Transform child in parent)
            {
                if (HasCameraInChildren(child))
                {
                    return true;
                }
            }
            return false;
        }
    }
}