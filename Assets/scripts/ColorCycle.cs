using UnityEngine;

public class ColorCycle : MonoBehaviour
{
    [SerializeField] private float vitesse = 0.1f;
    [SerializeField] private float saturation = 0.8f;
    [SerializeField] private float luminosite = 0.9f;

    private Renderer rend;
    private MaterialPropertyBlock block;
    private float hue;
    private bool actif = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        hue = Random.value;
    }

    public void SetActif(bool value)
    {
        actif = value;
        if (!actif)
        {
            rend.SetPropertyBlock(null);
        }
    }

    private void Update()
    {
        if (!actif) return;

        hue += vitesse * Time.deltaTime;
        if (hue > 1f) hue -= 1f;

        Color couleur = Color.HSVToRGB(hue, saturation, luminosite);

        rend.GetPropertyBlock(block);
        block.SetColor("_BaseColor", couleur);
        rend.SetPropertyBlock(block);
    }
}
