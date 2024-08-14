using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Guns : NetworkBehaviour
{
    [Header("Gun Stats scriptable object")]
    public GunsScriptableObjects gunStats;
    
    //Gun stats
    [Header("Guns General Stats")]
    private int bulletsLeft, bulletsShot;
    public Transform gunTransform;
    private Vector3 direction;
    private float x;
    private float y;
    public GameObject bulletTrailPrefab;
    public Slider gunReloadBar;
    public float colorIntensity;
    public float throwSpeed;
    public Rigidbody rb;
    
    [Header("Bools")]
    bool shooting, readyToShoot, reloading;

    
    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public LayerMask whatIsEnemy;

    [Header("Graphics")]
    public GameObject bulletHoleGraphic;
    
    private void Awake()
    {
        fpsCam = Camera.main;
        gunReloadBar = GameObject.Find("ReloadSlider").GetComponent<Slider>();
        bulletsLeft = gunStats.magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        MyInput();

        direction = fpsCam.transform.forward + new Vector3(x, y, 0);
        Quaternion rotation = Quaternion.LookRotation(direction);
        gunTransform.rotation = rotation;

        gunReloadBar.maxValue = gunStats.magazineSize;
        gunReloadBar.value = bulletsLeft;
    }

    private void MyInput()
    {
        shooting = gunStats.allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < gunStats.magazineSize && !reloading)
        {
            Debug.Log("Reload started by player " + NetworkManager.Singleton.LocalClientId);
            StartCoroutine(Reload());
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = gunStats.bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        RaycastHit rayHit;

        x = Random.Range(-gunStats.spread, gunStats.spread);
        y = Random.Range(-gunStats.spread, gunStats.spread);
        direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        if (Physics.Raycast(attackPoint.position, direction, out rayHit, gunStats.range, whatIsEnemy))
        {
            if (rayHit.collider.CompareTag("Enemy"))
            {
                rayHit.collider.GetComponent<EnemyAi>().TakeDamage(gunStats.damage);
            }
        }

        if (IsServer)
        {
            SpawnBulletOrTrailServerRpc(attackPoint.position, Quaternion.LookRotation(direction));
        }
        else
        {
            SpawnBulletOrTrailClientRpc(attackPoint.position, Quaternion.LookRotation(direction));
        }

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", gunStats.fireRate);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", gunStats.fireRate);
        }
    }

    [ServerRpc]
    private void SpawnBulletOrTrailServerRpc(Vector3 position, Quaternion rotation)
    {
        SpawnBulletOrTrail(position, rotation);
    }

    [ClientRpc]
    private void SpawnBulletOrTrailClientRpc(Vector3 position, Quaternion rotation)
    {
        SpawnBulletOrTrail(position, rotation);
    }

    private void SpawnBulletOrTrail(Vector3 position, Quaternion rotation)
    {
        if (gunStats.bulletObject)
        {
            bulletTrailPrefab = PoolManager.instance.GetPooledBulletObject();
        }
        else
        {
            bulletTrailPrefab = PoolManager.instance.GetPooledBulletsTrails();
        }

        if (bulletTrailPrefab != null)
        {
            bulletTrailPrefab.transform.position = position;
            bulletTrailPrefab.transform.rotation = rotation;
            bulletTrailPrefab.SetActive(true);

            if (gunStats.bulletObject)
            {
                rb = bulletTrailPrefab.GetComponent<Rigidbody>();
                rb.velocity = attackPoint.forward * throwSpeed;
            }
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private IEnumerator Reload()
    {
        reloading = true;
        float elapsedTime = 0f;

        while (elapsedTime < gunStats.reloadTime)
        {
            gunReloadBar.value = Mathf.Lerp(0, gunStats.magazineSize, elapsedTime / gunStats.reloadTime);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        ReloadFinished();
    }

    private void ReloadFinished()
    {
        bulletsLeft = gunStats.magazineSize;
        reloading = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.color = Color.red;
        Vector3 startPos = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;
        Gizmos.DrawLine(startPos, startPos + direction * gunStats.range);
    }
}
