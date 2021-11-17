using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquidSoldier : MonoBehaviour
{
    [SerializeField] Slider hpbar;
    [SerializeField] GameObject explosion;
    [SerializeField] Rigidbody _base;
    int hp = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            GetDamage();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name.Contains("Glove") && collision.impulse.magnitude > 3)
            GetDamage();
    }

    public void GetDamage(int damage = 1)
    {
        if(hp > 0)
        {
            hp -= damage;
            hpbar.value = hp / 10.0f;
            if(hp == 0)
                Die();
        }
    }

    void Die()
    {
        _base.isKinematic = false;
        _base.AddExplosionForce(30000, _base.transform.position - _base.transform.forward - _base.transform.up, 100);
        explosion.SetActive(true);
    }
}
