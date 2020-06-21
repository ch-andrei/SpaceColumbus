using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using Random = UnityEngine.Random;

namespace GameSounds.AudioEvents
{
    public abstract class AudioEvent : ScriptableObject
    {
        public abstract void Play(AudioSource source);
    }

    [CreateAssetMenu(menuName = "Fire and Ice/Audio/SFXEvent")]
    public class SFXEvent : AudioEvent
    {
        public AudioClip[] Sfx;
        public AudioMixerGroup AudioOutput;

        [Range(0f, 2f)]
        public float Volume;

        [Range(0f, 2f)]
        public float Pitch;

        public override void Play(AudioSource source)
        {
            if (Sfx.Length == 0)
                return;

            source.clip = Sfx[Random.Range(0, Sfx.Length)];
            source.volume = Random.Range(0, Volume);
            source.pitch = Random.Range(0, Pitch);
            source.outputAudioMixerGroup = AudioOutput;
            source.Play();
        }

        public void Play()
        {

        }
    }

    [CustomEditor(typeof(AudioEvent), editorForChildClasses:true)]
    public class AudioEventEditor : Editor
    {
        private AudioSource _previewer;

        public void OnEnable()
        {
            _previewer = EditorUtility
                .CreateGameObjectWithHideFlags("Audio Preview", HideFlags.HideAndDontSave, typeof(AudioSource))
                .GetComponent<AudioSource>();
        }

        public void OnDisable()
        {
            DestroyImmediate(_previewer.gameObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview"))
            {
                ((AudioEvent)target).Play((_previewer));
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
