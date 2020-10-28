using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCListCTRL : MonoBehaviour
{
    [Header("NPC")]
    public NpcController npc;
    [Header("Resource")]
    public List<Sprite> HP5 = new List<Sprite>();//1 2 3 4 5
    public List<Sprite> ItemIcon = new List<Sprite>();//Meditcal Kit    Repair_parts    Keys
    public List<Sprite> NPCIcon = new List<Sprite>();//Chef Guard1 Guard2 Guard3 Preist Theology Worker1 Worker2 HighPresit
    [Header("NPCList")]
    public Text BPMPos;//
    public Text NamePos;//
    public Image NPCIconPos;//
    public Image HPPos;
    public Image ItemPos;//

    float UpdateTime;

    public void Setup(NpcController NC)
    {
        npc = NC;
    }

    private void Update()
    {
        if(npc != null)
        {
            UpdateTime += Time.deltaTime;
            switch (npc.status.npcName)
            {
                case "":
                    NPCIconPos.sprite = null;
                    ItemPos.color = new Color(1, 1, 1, 0);
                    break;
                case "RestInPeaceGuy":
                    NPCIconPos.sprite = NPCIcon[0];
                    ItemPos.color = Color.white;
                    break;
                default:
                    break;
            }

            NamePos.text = npc.status.npcName;
            switch (npc.status.CarryItem)
            {
                case Item_SO.ItemType.None:
                    ItemPos.sprite = null;
                    ItemPos.color = new Color(1, 1, 1, 0);
                    break;
                case Item_SO.ItemType.MedicalKit:
                    ItemPos.sprite = ItemIcon[0];
                    ItemPos.color = Color.white;
                    break;
                case Item_SO.ItemType.RepairedPart:
                    ItemPos.sprite = ItemIcon[1];
                    ItemPos.color = Color.white;
                    break;
                case Item_SO.ItemType.Key:
                    ItemPos.sprite = ItemIcon[2];
                    ItemPos.color = Color.white;
                    break;
                default:
                    break;
            }

            if(UpdateTime >= 1)
            {
                BPMPos.text = (70 + (100 - npc.status.currentStamina) + Random.Range(-10, 8)).ToString() + "<size=9>BPM</size>";
                UpdateTime = 0;
            }

            if (npc.status.currentHealth == 100)
            {
                HPPos.sprite = HP5[4];
            }
            else if (npc.status.currentHealth > 75)
            {
                HPPos.sprite = HP5[3];
            }
            else if (npc.status.currentHealth > 50)
            {
                HPPos.sprite = HP5[2];
            }
            else if (npc.status.currentHealth > 25)
            {
                HPPos.sprite = HP5[1];
            }
            else if (npc.status.currentHealth > 0)
            {
                HPPos.sprite = HP5[0];
            }
        }
    }
}
