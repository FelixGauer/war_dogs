using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Guns : MonoBehaviour
{
    public GunsScriptableObjects gunStats;
    
    //Gun stats
    public int damage;
    int bulletsLeft, bulletsShot;
    public Transform gunTransform;
    private Vector3 direction;
    private float x;
    private float y;
    public GameObject bulletTrailGo;
    public Slider gunReloadBar;
    public float colorIntensity;
    
    //bools 
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    //Graphics
    public GameObject bulletHoleGraphic;
    public float camShakeMagnitude, camShakeDuration;

    private void Awake()
    {
        bulletsLeft = gunStats.magazineSize;
        readyToShoot = true;
    }
    private void Update()
    {
        MyInput();
        
        direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        Quaternion rotation = Quaternion.LookRotation(direction);
        gunTransform.rotation = rotation;

        gunReloadBar.maxValue = gunStats.magazineSize;
        gunReloadBar.value = bulletsLeft;
        
    }
    private void MyInput()
    {
        if (gunStats.allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < gunStats.magazineSize && !reloading)
        {
            StartCoroutine(Reload());
        }

        
        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0){
            bulletsShot = gunStats.bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        //Spread
        x = Random.Range(-gunStats.spread, gunStats.spread);
        y = Random.Range(gunStats.spread, gunStats.spread);

        //Calculate Direction with Spread

        //RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, gunStats.range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"));
            //TODO: Enemy AI controller and damage
        }

        //TODO: ShakeCamera


        bulletTrailGo = PoolManager.instance.GetPooledBullets();
        
        // Using Gradiant
         // ParticleSystem.MainModule bulletTrailParticle = bulletTrailGo.GetComponent<ParticleSystem>().main;
         // bulletTrailParticle.startColor = new ParticleSystem.MinMaxGradient(gunStats.minColor, gunStats.maxColor);

         
         //TODO move this to when switching weapons
        Material material = gunStats.material;
        material.SetColor("_EmissionColor", gunStats.emissionColor * colorIntensity);
        
        
        if (bulletTrailGo != null)
        {
            bulletTrailGo.transform.position = attackPoint.position;
            bulletTrailGo.transform.rotation = attackPoint.rotation;
            bulletTrailGo.SetActive(true);
            
        }
        
        //Graphics
        Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
        // Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", gunStats.fireRate);
        
        if(bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", gunStats.fireRate);
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
}