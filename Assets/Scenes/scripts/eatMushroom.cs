using UnityEngine;
using UnityEngine.Events;

public class eatMushroom : MonoBehaviour
{
    public UnityEvent onEatMushroom;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Mouth")
        {
            onEatMushroom.Invoke();
            gameObject.SetActive(false);
        }
    }
}
