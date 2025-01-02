using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cute : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Animator animator;
    public GameObject Player;
    public Transform Playerpoint;
    public MeshRenderer MeshRenderer;
    public GameObject Wing1;
    public GameObject Wing2;
    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        transform.parent = Playerpoint.transform;
    }

    void Update()
    {
        Vector3 origin = Player.transform.position;
        Vector3 direction = transform.position;

        lineRenderer.SetPosition(0, direction); 
        lineRenderer.SetPosition(1, origin);

        if (Player.GetComponent<Player>().State == StarterAssets.Player.PlayerState.Climb ||
    Player.GetComponent<Player>().State == StarterAssets.Player.PlayerState.Throw)
        {
            MeshRenderer.enabled = true;
            Wing1.GetComponent <MeshRenderer>().enabled = true;
            Wing2.GetComponent<MeshRenderer>().enabled = true;
            lineRenderer.enabled = true;
        }
        else
        {
            MeshRenderer.enabled = false;
            Wing1.GetComponent<MeshRenderer>().enabled = false;
            Wing2.GetComponent<MeshRenderer>().enabled = false;
            lineRenderer.enabled = false;
        }
        if(Player.GetComponent<Player>().State == StarterAssets.Player.PlayerState.Throw)
        {
            animator.enabled = false;
            transform.parent = null;
            transform.position = Player.GetComponent<Player>().Hitwall;
        }
        else
        {
            transform.parent = Playerpoint.transform;
            transform.position = Playerpoint.position;
            animator.enabled = true ;
        }
    }
}
