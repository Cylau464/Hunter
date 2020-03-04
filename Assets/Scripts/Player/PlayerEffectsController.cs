using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;
using Enums;

public class PlayerEffectsController : MonoBehaviour
{


    [SerializeField] private EffectsDictionary effects = new EffectsDictionary()
    {
        { Effects.Freeze, new Effect(5, 1f) },
        { Effects.Burning, new Effect(5, 2f) },
        { Effects.Bleeding, new Effect(5, 3f) },
    };
}