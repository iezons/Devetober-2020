using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Anim_SO", menuName = "Anim_SO", order = 1)]
public class Anim_SO : ScriptableObject
{
    public List<AnimationClip> AnimClips;
}
