using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Structures;

public class Shell : MonoBehaviour
{
    int damage;
    DamageTypes damageType;
    Element element;
    float forceSpeed;
    float flyTime;
    Rigidbody2D rigidBody;

    bool damageDone;
    bool isFlying = true;
    Vector2 curPoint;
    Vector2 prevPoint;
    Vector2 curDir;

    AudioClip impactClip;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        
        float angle = transform.rotation.eulerAngles.z;
        angle *= Mathf.Deg2Rad;
        float xComponent = Mathf.Cos(angle) * forceSpeed;
        float yComponent = Mathf.Sin(angle) * forceSpeed;
        rigidBody.AddForce(new Vector2(xComponent, yComponent), ForceMode2D.Impulse);
    }

    private void Update()
    {
        //transform.Rotate(new Vector3(0, 0, 360.0f - Vector3.Angle(transform.right, rigidBody.velocity.normalized)));

        if (flyTime <= Time.time && isFlying)
        {
            rigidBody.gravityScale = .5f;
        }
    }

    private void FixedUpdate()
    {
        if (isFlying)
        {
            curPoint = transform.position;

            //get the direction (from previous pos to current pos)
            curDir = prevPoint - curPoint;

            //normalize the direction
            curDir.Normalize();

            //get angle whose tan = y/x, and convert from rads to degrees
            float rotationZ = Mathf.Atan2(curDir.y, curDir.x) * Mathf.Rad2Deg;
            Vector3 Vzero = Vector3.zero;

            //rotate z based on angle above + an offset (currently 180)
            transform.rotation = Quaternion.Euler(0, 0, rotationZ + 180f);

            //store the current point as the old point for the next frame
            prevPoint = curPoint;
        }
        else if(rigidBody.velocity.magnitude > 0f)
        {
            rigidBody.velocity *= .9f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 12 || collider.gameObject.layer == 9)      // 12 layer - Enemy, 9 - Platforms
        {
            isFlying = false;
            rigidBody.velocity = Vector2.zero;
            rigidBody.gravityScale = 0f;

            collider.TryGetComponent(out Enemy _enemy);

            if (_enemy != null && !damageDone)
            {
                rigidBody.bodyType = RigidbodyType2D.Kinematic;
                transform.parent = _enemy.transform;
                int _damageDone = _enemy.TakeDamage(damage, damageType, element);

                if (_damageDone > 0)
                    AudioManager.PlayClipAtPosition(impactClip, transform.position, .25f, 10f);

                damageDone = true;
            }

            Invoke("DestroyShell", 2f);
        }
    }

    public void SetParameters(int damage, DamageTypes damageType, Element element, float forceSpeed, float flyTime, AudioClip impactClip)
    {
        this.damage = damage;
        this.damageType = damageType;
        this.element = element;
        this.forceSpeed = forceSpeed;
        this.flyTime = flyTime + Time.time;
        this.impactClip = impactClip;
    }

    void DestroyShell()
    {
        Destroy(gameObject);

    }
}
