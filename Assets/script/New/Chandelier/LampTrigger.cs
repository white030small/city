using UnityEngine;

public class LampTrigger : MonoBehaviour
{
    public SimpleChandelier mainScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (mainScript != null)
            {
                mainScript.StartFalling();
            }
        }
    }
}