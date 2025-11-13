using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 0.5f;
    [SerializeField] private int damage = 10;
    

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return; 

        IDamageable damageable = other.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            damageable.OnDamage(damage); 
            Debug.Log($"{other.name} recived {damage} damage");
            
            Destroy(gameObject);
            
        }
        else if (!other.isTrigger) 
        {
            Destroy(gameObject);
        }
    }
}