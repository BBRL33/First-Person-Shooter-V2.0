using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHandling : MonoBehaviour
{
    public int shotDamage;
    public bool isAuto;
    public bool isDouble;
    public GameObject muzzleFlash;
    public float timeBetweenShots = 0.1f, heatPerShots = 1f;
    public float scope;
    public AudioSource hit;
}
