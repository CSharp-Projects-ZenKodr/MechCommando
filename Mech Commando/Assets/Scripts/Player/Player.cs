﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingEntity
{
    int currentShield;
    [SerializeField]
    int maxShield;

    public int currentEnergy;
    [SerializeField]
    int maxEnergy;
    protected int healthPacksQt;
    public readonly int healthPacksQtMax = 3;
    bool alive;
    public bool inControl;
    public bool inPlayableArea;
    float deathTimer;
    [SerializeField]
    float deathTime;

    public delegate void PlayerDeath();
    public static event PlayerDeath onDeath;

    public delegate void UpdateHealthEvent(int h, int maxH);
    public static event UpdateHealthEvent onHealthUpdate;

    public delegate void UpdateEnergyEvent(int e, int maxE);
    public static event UpdateEnergyEvent onEnergyUpdate;

    public delegate void UpdateShieldEvent(int s, int maxS);
    public static event UpdateShieldEvent onShieldUpdate;

    public delegate void UpdateNanopakEvent(int n);
    public static event UpdateNanopakEvent onNanopakUpdate;

    public delegate void PrimaryFire();
    public static event PrimaryFire onPrimaryFire;

    public delegate void UpdatePlayableAreaHud(float timer, bool outsideMap, bool alive);
    public static event UpdatePlayableAreaHud onAreaUpdate;

    //Energy
    float EnergyRecoverTimer;
    [SerializeField]
    readonly float EnergyRecoverTime = 0.025f; //Time Between Energy recovering
    bool canRecoverEnergy;
    [SerializeField]
    float EnergyRecoverDelay = 0.3f; //Time before energy starts recovering again
    float EnergyRecoverDelayTimer;

    PlayerMovementManager movManager;

    

    protected override void Awake()
    {
        alive = true;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        inControl = true;
        EnergyRecoverTimer = 0;
        canRecoverEnergy = true;
        EnergyRecoverTimer = 0;
        currentShield = 0;
        inPlayableArea = true;
        deathTimer = deathTime;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        onHealthUpdate(currentHealth, maxHealth);
        onEnergyUpdate(currentEnergy, maxEnergy);
        onShieldUpdate(currentShield, maxShield);
        onAreaUpdate(deathTimer, inPlayableArea, alive);
        onNanopakUpdate(healthPacksQt);

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (alive && inControl)
        {
            recoverEnergy();

            if (healthPacksQt > 0 && Input.GetButtonDown("NanoPak"))
            {
                //Debug.Log("nanoPak");
                useHealthPack();
            }

            PlayableArea();
        }
    }

    private void PlayableArea()
    {
       if (!inPlayableArea)
        {
            
            if (deathTimer >= 0) deathTimer -= Time.deltaTime;
            else
            {
                deathTimer = 0;
                Die();                         
            }            
        } else
        {
            deathTimer = deathTime;
        }
        onAreaUpdate(deathTimer, inPlayableArea, alive);
    }

    public bool isAlive() => alive;
    public int CurrentShield() => currentShield;
    public int MaxShield() => maxShield;


    public void spendEnergy(int newEnergy)
    {
        currentEnergy = newEnergy;
        try
        {
            onEnergyUpdate(currentEnergy, maxEnergy);
        }
        catch (NullReferenceException)
        {
            
        }
        canRecoverEnergy = false;
        EnergyRecoverDelayTimer = 0;
    }


    private void recoverEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            if (canRecoverEnergy)
            {
                if (EnergyRecoverTimer < EnergyRecoverTime) EnergyRecoverTimer += Time.deltaTime;
                else
                {
                    currentEnergy++;
                    onEnergyUpdate(currentEnergy, maxEnergy);
                    EnergyRecoverTimer = 0;
                }
            } else
            {
                if (EnergyRecoverDelayTimer < EnergyRecoverDelay) EnergyRecoverDelayTimer += Time.deltaTime;
                else { canRecoverEnergy = true; }
            }
        }
    }


    void useHealthPack()
    {
        if (currentHealth < maxHealth)
        {
            if (currentHealth + 50 > maxHealth) { currentHealth = maxHealth; }
            else { currentHealth += 50; healthPacksQt--; Debug.Log($"NanoPak used, {healthPacksQt} remaining..."); }

        } else
        {
            Debug.Log("Health already maxed");
        }
        onNanopakUpdate(healthPacksQt);
    }

    public override void getDamage(int damage)
    {
       // base.getDamage(damage);

        if (currentShield < 0) { //If player has shield

            int dmgHealth = damage / 5; //damage receive is 1/5
            currentHealth -= dmgHealth;
            int dmgShield = (damage - damage / 5) / 2; //shield receives 80% / 2 damage
            currentShield -= dmgShield;
            if (currentShield > 0) currentShield = 0;

        }
        else
        {
            currentHealth -= damage;
        }

        checkHealth();

        onHealthUpdate(currentHealth, maxHealth);
        onShieldUpdate(currentShield, maxShield);

    }

    public override void Die()
    {
        currentHealth = 0;
        alive = false;
        inControl = false;
        Debug.Log($"Player Died");

    }



    public void increaseHpak() {
        healthPacksQt++;
        onNanopakUpdate(healthPacksQt);
    }

    public int getHealthPakQt() => healthPacksQt;

    public void increaseShield(int ammount)
    {
        if (currentShield + ammount < maxShield) currentShield += ammount;
        else currentShield = maxShield;

        onShieldUpdate(currentShield, maxShield);
    }

}
