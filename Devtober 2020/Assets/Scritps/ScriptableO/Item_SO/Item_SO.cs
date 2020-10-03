using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_SO : ScriptableObject
{
    public int maxHealth = 0;
    public int currentHealth = 0;

    public void ApplyHealth(int healthAmount)
    {
        currentHealth = currentHealth + healthAmount > maxHealth ? maxHealth : currentHealth += healthAmount;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            //Death();
        }
    }
}
