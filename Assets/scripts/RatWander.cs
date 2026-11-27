using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RatWander : MonoBehaviour
{
    [SerializeField] private Vector3 tailleZone = new Vector3(3f, 0f, 3f);
    [SerializeField] private float vitesse = 1f;
    [SerializeField] private float vitesseRotation = 5f;
    [SerializeField] private float tempsPauseMin = 1f;
    [SerializeField] private float tempsPauseMax = 3f;
    [SerializeField] private float tempsChangementDirectionMax = 6f;

    private Rigidbody rb;
    private Vector3 centreZone;
    private Vector3 direction;
    private float tempsPauseRestant = 0f;
    private float tempsAvantChangement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        centreZone = transform.position;
        ChoisirNouvelleDirection();
    }

    private void FixedUpdate()
    {
        float vitesseVerticale = rb.linearVelocity.y;

        if (tempsPauseRestant > 0f)
        {
            tempsPauseRestant -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(0f, vitesseVerticale, 0f);
            return;
        }

        Vector3 decalage = transform.position - centreZone;
        bool horsZoneX = Mathf.Abs(decalage.x) > tailleZone.x / 2f;
        bool horsZoneZ = Mathf.Abs(decalage.z) > tailleZone.z / 2f;

        if (horsZoneX || horsZoneZ)
        {
            direction = (centreZone - transform.position).normalized;
            direction.y = 0f;
            tempsAvantChangement = Random.Range(2f, tempsChangementDirectionMax);
        }
        else
        {
            tempsAvantChangement -= Time.fixedDeltaTime;
            if (tempsAvantChangement <= 0f)
            {
                tempsPauseRestant = Random.Range(tempsPauseMin, tempsPauseMax);
                ChoisirNouvelleDirection();
                return;
            }
        }

        rb.linearVelocity = new Vector3(direction.x * vitesse, vitesseVerticale, direction.z * vitesse);

        Quaternion rotationCible = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationCible, vitesseRotation * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            Vector3 normale = collision.contacts[0].normal;
            direction = Vector3.Reflect(direction, normale).normalized;
            direction.y = 0f;
            tempsAvantChangement = Random.Range(2f, tempsChangementDirectionMax);
        }
    }

    private void ChoisirNouvelleDirection()
    {
        float angle = Random.Range(0f, 360f);
        direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        tempsAvantChangement = Random.Range(2f, tempsChangementDirectionMax);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 centre = Application.isPlaying ? centreZone : transform.position;
        Gizmos.DrawWireCube(centre, new Vector3(tailleZone.x, 0.1f, tailleZone.z));
    }
}