using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newGun", menuName = "Gun")]
public class Guns : ScriptableObject
{
    public string Name;
    public float FiringSpeed;
    public int Damage;
    public int Burst; // 0 single | 1 auto | 2+ burst
    public float Recoil;
    public float KickBack;
    public float AimSpeed;
    public float Bloom;
    public int Ammo;
    public int ClipSize;
    public float ReloadTime;
    public GameObject prefabs;

    private int Clip; //current clip
    private int Stash; //current ammo

    public void Initialize()
    {
        Stash = Ammo;
        Clip = ClipSize;
    }

    public bool FireBullet()
    {
        if (Clip > 0)
        {
            Clip -= 1;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        Stash += Clip;
        Clip = Mathf.Min(ClipSize, Stash);
        Stash -= Clip;
    }

    public int GetStash() { return Stash; }
    public int GetClip() { return Clip; }
}
