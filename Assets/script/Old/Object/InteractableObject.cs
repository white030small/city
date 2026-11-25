using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool isPlayerNearby = false;

 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("Player is nearby");
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            Debug.Log("Player is no longer nearby");
        }
    }

    void Update()
    {

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            DoInteract();
        }
    }

    void DoInteract()
    {
        Debug.Log("Interacted with object");

    }
}