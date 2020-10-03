using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/Stats", order = 1)]
public class NPC_SO: ScriptableObject
{
    public string npcName;
    public string description;

    public int maxHealth = 0;
    public int currentHealth = 0;

    public int maxStamina = 0;
    public float currentStamina = 0;
    public int staminaConsumeRate = 0;
    public int staminaRecoverRate = 0;

    public int maxCredit = 0;
    public int currentCredit = 0;

    public List<string> toDoList;

    public void ApplyHealth(int healthAmount)
    {
        currentHealth = currentHealth + healthAmount > maxHealth ? maxHealth : currentHealth += healthAmount;
    }

    public void ApplyStamina(int staminaAmount)
    {
        currentStamina = currentStamina + staminaAmount > maxStamina ? maxStamina : currentStamina += staminaAmount;
    }

    public void ApplyCredit(int creditAmount)
    {
        currentCredit = currentCredit + creditAmount > maxCredit ? maxCredit : currentCredit += creditAmount;
    }


    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if(currentHealth <= 0)
        {
            //Death();
        }
    }

    public void ConsumeStamina()
    {
        currentStamina -= staminaConsumeRate * Time.deltaTime;
    }

    public void RecoverStamina()
    {
        currentStamina += staminaRecoverRate * Time.deltaTime;
    }

    public void ReduceCredit(int creditAmount)
    {
        currentCredit = currentCredit - creditAmount <= 0 ? 0 : currentCredit -= creditAmount;
    }
}
