using UnityEngine;

public class Key : MonoBehaviour
{
    public string colorID; // 在 Inspector 里填入 "Red", "Blue" 等

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory != null)
            {
                Debug.Log("player touched");
                inventory.AddKey(colorID);
                Destroy(gameObject); // 钥匙消失
            }
        }
    }
}