using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines; // Array to determine which lines should auto-progress after being fully displayed
    public float autoProgressDelay = 1f; // Time to wait before auto-progressing to the next line
    public float typingSpeed = 0.05f; // Time between each character being typed
    public AudioClip voiceSound;
    public float voicePitch = 1f;

 
}
