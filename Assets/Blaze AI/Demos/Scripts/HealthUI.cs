using UnityEngine;
using TMPro;


public class HealthUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public Health blazeHealth;
    

    // Update is called once per frame
    void Update()
    {
        healthText.text = "Health: " + blazeHealth.currentHealth;
    }
}
