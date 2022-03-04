using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour 
{
    static public Enemy S; // Singleton

    [Header("Set in Inspector: Enemy")]
    public float speed = 10f; // The speed in m/s
    public float fireRate = 0.5f; // Seconds/shot (Unused)
    public float health = 10;
    public int score = 100; // Points earned for destroying this
    public float showDamageDuration = 0.1f; // # seconds to show damage
    public float powerUpDropChance = 1f; // Chance to drop a power-up
 

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials;// All the Materials of this & its children
    public bool showingDamage = false;
    public float damageDoneTime; // Time to stop showing damage
    public bool notifiedOfDestruction = false; // Will be used later
    public GameObject projectilePrefab;
    [SerializeField]
    public float _shieldLevel = 1;

    protected BoundsCheck bndCheck;
    private float time = 0.0f; 

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        // Get materials and colors for this GameObject and its children
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i=0; i<materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
        InvokeRepeating("MakeProjectile", 2.0f, 4.0f);
        InvokeRepeating("MakeProjectile", 2.3f, 4.0f);
        InvokeRepeating("MakeProjectile", 2.6f, 4.0f);
    }

    // This is a property: A method that acts like a field
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }
        set
        {
            this.transform.position = value;
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        Move();

        if(showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if (bndCheck != null && bndCheck.offDown)
        {
            // We're off the bottom, so destroy this GameObject
            Destroy(gameObject);
        }
        if (time >= fireRate)
        {
            //MakeProjectile();
            time = 0.0f;
        }
    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        switch (otherGO.tag)
        {
            case "ProjectileHero":
                Projectile p = otherGO.GetComponent<Projectile>();
                // If this Enemy is off screen, don't damage it.
                if (!bndCheck.isOnScreen)
                {
                    Destroy(otherGO);
                    break;
                }

                if (shieldLevel > 0)
                //if (otherGO.tag == "ProjectileHero")
                {
                    shieldLevel--;
                    Destroy(otherGO);

                }

                // Hurt this Enemy
                ShowDamage();
                // Get the damage amount from the Main WEAP_DICT
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                if (health <= 0)
                {
                    // Tell the Main singleton that this ship was destroyed
                    if (!notifiedOfDestruction)
                    {
                        Main.S.ShipDestroyed(this);
                    }
                    notifiedOfDestruction = true;
                    // Destroy this enemy
                    Destroy(this.gameObject);
                }
                Destroy(otherGO);
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGO.name);
                break;
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials)
        {
            m.color = Color.red;
        }
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for (int i=0; i<materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }
    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(projectilePrefab);
        
        go.tag = "ProjectileEnemy";
        go.layer = LayerMask.NameToLayer("ProjectileEnemy");

        go.transform.position = new Vector3(pos.x, pos.y - 5, pos.z);
        Projectile p = go.GetComponent<Projectile>();
        p.rigid.velocity = new Vector3(0,-20,0);
        return p;
    }

    public float shieldLevel
    {
        get
        {
            return (_shieldLevel);
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 2);
            // If the shield is going to be set to less than zero
            if (value < 0)
            {
                GameObject shield = this.gameObject.transform.Find("Enemy_Shield").gameObject;
                Destroy(shield);
            }
        }
    }
}
