using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to quickly set up the AudioManager in the scene
/// </summary>
public class AudioManagerSetup : EditorWindow
{
    [MenuItem("Pacman/Setup Audio Manager")]
    public static void SetupAudioManager()
    {
        // Check if AudioManager already exists
        AudioManager existingManager = FindFirstObjectByType<AudioManager>();
        
        if (existingManager != null)
        {
            Debug.LogWarning("AudioManager already exists in the scene!");
            Selection.activeGameObject = existingManager.gameObject;
            return;
        }

        // Create new GameObject
        GameObject audioManagerObj = new GameObject("AudioManager");
        
        // Add AudioManager component
        AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
        
        // Add two AudioSource components (one for music, one for SFX)
        AudioSource musicSource = audioManagerObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;
        
        AudioSource sfxSource = audioManagerObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.7f;
        
        // Use SerializedObject to assign private fields
        SerializedObject serializedManager = new SerializedObject(audioManager);
        serializedManager.FindProperty("musicSource").objectReferenceValue = musicSource;
        serializedManager.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        serializedManager.ApplyModifiedProperties();
        
        // Select the newly created object
        Selection.activeGameObject = audioManagerObj;
        
        Debug.Log("AudioManager created successfully! Now assign your audio clips in the Inspector.");
        EditorGUIUtility.PingObject(audioManagerObj);
    }
}
