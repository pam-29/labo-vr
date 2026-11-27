using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

public class VitrineOuverture : MonoBehaviour
{
    [Header("Références des objets")]
    public Transform clocheOuvrante;        
    public XRBaseInteractable champignonInteractable;

    [Header("Réglages de l'animation")]
    public float delaiAvantOuverture = 3f;    
    public float hauteurOuverture = 0.8f;      
    public float vitesseOuverture = 1.5f;      

    private Vector3 positionFermee;
    private Vector3 positionOuverte;

    void Start()
    {
        if (champignonInteractable != null)
        {
            champignonInteractable.enabled = false;
        }

        if (clocheOuvrante != null)
        {
            positionFermee = clocheOuvrante.localPosition;
            positionOuverte = positionFermee + new Vector3(0f, hauteurOuverture, 0f);
            
            StartCoroutine(OuvrirApresDelai());
        }
    }

    private IEnumerator OuvrirApresDelai()
    {
        yield return new WaitForSeconds(delaiAvantOuverture);

        float progression = 0f;
        while (progression < 1f)
        {
            progression += Time.deltaTime * vitesseOuverture;
            clocheOuvrante.localPosition = Vector3.Lerp(positionFermee, positionOuverte, progression);
            yield return null;
        }

        if (champignonInteractable != null)
        {
            champignonInteractable.enabled = true;
        }
    }
}
