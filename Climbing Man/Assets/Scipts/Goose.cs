using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Goose : MonoBehaviour
{
    private GameObject currentBaby = null;

    [Header("player empty object")]
    public Transform babyAttachPoint;

    [Header("Home empty object slot")]
    public List<Transform> homeSlots;

    [Header("UI Elements")]
    public TextMeshProUGUI progressText;
    private int placeBabyCount = 0;
    private bool uiShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Baby") && currentBaby == null)
        {
            Baby babyScript = other.GetComponent<Baby>();
            if (babyScript != null && !babyScript.isPlacedAtHome)
            {
                AttachBabyToPlayer(other.gameObject);
                
            }
            
        }
        else if(other.CompareTag ("Home")&& currentBaby != null)
        {
            TransferBabyToHome();
        }
    }

    void AttachBabyToPlayer(GameObject baby)
    {
        currentBaby = baby;
        baby.transform.SetParent(babyAttachPoint);
        baby.transform.localPosition = Vector3.zero;
        baby.transform.localRotation = Quaternion.identity;
        //baby.transform.localScale = Vector3.one;

        if(!uiShown)
        {
            progressText.gameObject.SetActive(true);
            UpdateProgressText();
            uiShown = true;
        }
    }

    void TransferBabyToHome()
    {
        foreach(Transform slot in homeSlots)
        {
            if (slot.childCount == 0)
            {
                currentBaby.transform.SetParent(slot);
                currentBaby.transform.localPosition = Vector3.zero;
                currentBaby.transform.localRotation = Quaternion.identity;
                //currentBaby.transform.localScale = Vector3.one;

                Baby babyScript = currentBaby.GetComponent<Baby>();
                if(babyScript !=null)
                {
                    babyScript.isPlacedAtHome = true;
                }
                currentBaby = null;
               
                placeBabyCount++;
                UpdateProgressText();

                CheckGameOver();
                break;
            }
        }
    }

    

    void UpdateProgressText()
    {
        progressText.text = $"{placeBabyCount}/{homeSlots.Count}";
      
    } 

    void CheckGameOver()
    {
        foreach(Transform slot in homeSlots)
        {
            if (slot.childCount == 0) return;
        }
        GameOver();
    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }

}
