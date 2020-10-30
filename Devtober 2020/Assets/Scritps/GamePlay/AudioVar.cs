using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionType
{
    Int,
    Boolean,
    GameObject,
}
[CreateAssetMenu]
[System.Serializable]
public class AudioVar : ScriptableObject
{
    public ConditionType conditionType;
    [Header("Load sounds here")]
    public AudioSource audioSource;
    public List<AudioClip> audios = new List<AudioClip>();
    [Space]
    public int inputInt;
    public bool inputBool;
    public GameObject GoInput;
    [Header("Check")]
    public bool isPlayed;
    [Header("ClipNumber")]
    public int clipNumber;
    void Awake()
    {
        Debug.Log("Awake");
        Object[] sounds = Resources.LoadAll("Audio", typeof(AudioClip));
        foreach (AudioClip audio in sounds)
        {
            audios.Add(audio);
        }
    }

    void AudioTypeRegister()
    {

        switch (conditionType)
        {
            case ConditionType.Int:
                IntInput(inputInt);
                return;
            case ConditionType.Boolean:
                BoolInput(inputBool);
                return;
            case ConditionType.GameObject:
                GameobjectInput(GoInput);
                return;
        }
    }

    //----------------------------------

    void IntInput(int input)
    {
        if (!isPlayed)
        {
            audioSource.PlayOneShot(audios[clipNumber]);
            isPlayed = true;
        }
    }

    void BoolInput(bool input)
    {
        if (input)
        {
            if (!isPlayed)
            {
                audioSource.PlayOneShot(audios[clipNumber]);
                isPlayed = true;
            }
        }
    }

    void GameobjectInput(GameObject input)
    {
        if (!isPlayed)
        {
            audioSource.PlayOneShot(audios[clipNumber]);
            isPlayed = true;
        }
    }
}