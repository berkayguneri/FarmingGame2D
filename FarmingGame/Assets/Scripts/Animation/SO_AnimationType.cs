using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_AnimationType",menuName ="Scriptable Objects/Animation/Animation Type")]
public class SO_AnimationType : ScriptableObject
{
    public AnimationClip animationClip; // A reference to the actual animation clip
    public AnimationName animationName; // An enum of the animation name like “AnimationName.IdleUp”
    public CharacterPartAnimator characterPart; // The gameobject name the Animator is on that controls these animation clips e.g. “arms
    public PartVariantColour partVariantColour;// To enable colour variations on a animation type to be specified e.g. “none”, “bronze”, “silver”, “gold”
    public PartVariantType partVariantType; // The variant type to specify what variant this animation type refers to e.g. “none”, “carry”, “hoe”, “pickaxe”, “axe” .. etc

}
