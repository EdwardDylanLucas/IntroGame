using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour {

    public Vector2 moveValue;
    public GameObject player;
    public float speed;
    private Vector3 oldPosition;
    private int count;
    private int numPickups = 8;
    private GameObject[] pickUps;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI PlayerPosition;
    public TextMeshProUGUI PlayerVelocity;
    public TextMeshProUGUI PlayerSpeed;
    public TextMeshProUGUI ClosestPickUp;
    private LineRenderer lineRenderer;

    void Start()
    {
        count = 0;
        winText.text = "";
        SetCountText();
        PlayerPosition.text = "";
        PlayerVelocity.text = "";
        PlayerSpeed.text = "";
        oldPosition = player.transform.position;
        pickUps = GameObject.FindGameObjectsWithTag("PickUp");
        lineRenderer = gameObject.AddComponent<LineRenderer>();
    }

    void OnMove(InputValue value)
    {
        moveValue = value.Get<Vector2 > ();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveValue.x, 0.0f, moveValue.y);
        GetComponent<Rigidbody>().AddForce(movement * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        Vector3 currentPosition = player.transform.position;
        Vector3 velocity = (currentPosition - oldPosition) / Time.deltaTime;
        float speed = velocity.magnitude;

        oldPosition = currentPosition;

        PlayerPosition.text = "Player position: " + currentPosition.ToString();
        PlayerVelocity.text = "Player velocity: " + velocity.ToString();
        PlayerSpeed.text = "Player speed: " + speed.ToString();
        UpdateClosestPickUp();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PickUp")
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }

    private void SetCountText()
    {
        scoreText.text = "Score: " + count.ToString();
        if(count >= numPickups)
        {
            winText.text = "You win!";
        }
    }

    void UpdateClosestPickUp()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPickUp = null;

        // Loop through each pickup to find the closest one
        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp.activeInHierarchy) // Check if the pickup is still active
            {
                float distanceToPickUp = Vector3.Distance(transform.position, pickUp.transform.position);

                // If this pickup is closer than the previously closest one, update the closest one
                if (distanceToPickUp < closestDistance)
                {
                    closestDistance = distanceToPickUp;
                    closestPickUp = pickUp;
                }

                // Reset the pickup's color to white
                pickUp.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        // If a closest pickup was found, highlight it and update the text
        if (closestPickUp != null)
        {
            closestPickUp.GetComponent<Renderer>().material.color = Color.blue;
            ClosestPickUp.text = "Closest PickUp Distance: " + closestDistance.ToString();
            // 0 for the start point , position vector ’ startPosition ’
            lineRenderer.SetPosition(0, player.transform.position);
            // 1 for the end point , position vector ’endPosition ’
            lineRenderer.SetPosition(1, closestPickUp.transform.position);
            // Width of 0.1 f both at origin and end of the line
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
        else
        {
            ClosestPickUp.text = "No active PickUps found.";
        }
    }

}
