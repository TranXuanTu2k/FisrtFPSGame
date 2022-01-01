using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{

    public Guns[] ListGuns;
    private int index;
    public Transform WeaponParent;
    private GameObject currentWeapon;

    public GameObject BulletHolePrefab;
    public LayerMask CanbeShoot;

    private float coolDown;

    private bool isReloading;

    public bool isAiming = false;

    private void Start()
    {
        foreach (Guns a in ListGuns) a.Initialize();
        Equip(0);
    }

    void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("Equip", RpcTarget.All, 0);
        }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("Equip", RpcTarget.All, 1);
        }

        if (currentWeapon != null)
        {
            if (photonView.IsMine)
            {
                Aim(Input.GetMouseButton(1));

                if(ListGuns[index].Burst != 1)
                {
                    if (Input.GetMouseButtonDown(0) && coolDown <= 0)
                    {
                        if (ListGuns[index].FireBullet())
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                        else StartCoroutine(Reload(ListGuns[index].ReloadTime));

                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && coolDown <= 0)
                    {
                        if (ListGuns[index].FireBullet())
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                        else StartCoroutine(Reload(ListGuns[index].ReloadTime));

                    }
                }
                

                if (Input.GetKeyDown(KeyCode.R))
                {
                    StartCoroutine(Reload(ListGuns[index].ReloadTime));
                }

                //cooldown
                if (coolDown > 0)
                {
                    coolDown -= Time.deltaTime;
                }
            }

            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
        }
        
    }

    [PunRPC]
    void Equip(int gunID)
    {
        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine("Reload");
            Destroy(currentWeapon);
        }

        index = gunID;

        GameObject newWeapon = Instantiate(ListGuns[gunID].prefabs, WeaponParent.position, WeaponParent.rotation, WeaponParent) as GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        newWeapon.GetComponent<SwayGun>().isMine = photonView.IsMine;

        currentWeapon = newWeapon;
    }

    void Aim(bool r_aim)
    {
        isAiming = r_aim;
        Transform anchor = currentWeapon.transform.GetChild(0);
        Transform Status_nor = currentWeapon.transform.GetChild(1).GetChild(0);
        Transform Status_aim = currentWeapon.transform.GetChild(1).GetChild(1);

        if (r_aim)
        {
            anchor.position = Vector3.Lerp(anchor.position, Status_aim.position, ListGuns[index].AimSpeed * Time.deltaTime);
        }
        else
        {
            anchor.position = Vector3.Lerp(anchor.position, Status_nor.position, ListGuns[index].AimSpeed * Time.deltaTime);
        }
    }

    [PunRPC]
    void Shoot()
    {
        Transform p_cam = transform.Find("PlayerCamera");

        //bloom
        Vector3 bloom = p_cam.position + p_cam.forward * 1000f;
        bloom += Random.Range(-ListGuns[index].Bloom, ListGuns[index].Bloom) * p_cam.up;
        bloom += Random.Range(-ListGuns[index].Bloom, ListGuns[index].Bloom) * p_cam.right;
        bloom -= p_cam.position;
        bloom.Normalize();

        //cooldown
        coolDown = ListGuns[index].FiringSpeed;

        //raycast
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(p_cam.position, bloom, out hit, 1000f, CanbeShoot))
        {
            GameObject newBulletHole = Instantiate(BulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity);
            newBulletHole.transform.LookAt(hit.point + hit.normal);

            Destroy(newBulletHole, 5f);

            if (photonView.IsMine)
            {
                //shooting other player on network
                if(hit.collider.gameObject.layer == 9)
                {
                    hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, ListGuns[index].Damage);
                }
            }
        }

        //giat len
        currentWeapon.transform.Rotate(-ListGuns[index].Recoil, 0, 0);

        //giat ve sau
        currentWeapon.transform.position -= currentWeapon.transform.forward * ListGuns[index].KickBack;
    }

    [PunRPC]
    private void TakeDamage(int dmg)
    {
        GetComponent<PlayerController>().TakeDamage(dmg);
    }

    IEnumerator Reload(float r_wait)
    {
        isReloading = true;
        currentWeapon.SetActive(false);

        yield return new WaitForSeconds(r_wait);

        ListGuns[index].Reload();
        currentWeapon.SetActive(true);

        isReloading = false;
    }

    public void RefreshAmmo(Text txt)
    {
        int clip = ListGuns[index].GetClip();
        int stash = ListGuns[index].GetStash();

        txt.text = clip.ToString("D2") + " / " + stash.ToString("D2");
    }
}
