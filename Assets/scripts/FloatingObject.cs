using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float vitesse = 1f;
    [SerializeField] private float vitesseRotation = 20f;

    private Vector3 positionDepart;

    private void Awake()
    {
        positionDepart = transform.localPosition;
    }

    private void Update()
    {
        float decalageY = Mathf.Sin(Time.time * vitesse) * amplitude;
        transform.localPosition = positionDepart + new Vector3(0f, decalageY, 0f);

        transform.Rotate(Vector3.up, vitesseRotation * Time.deltaTime);
    }
}
