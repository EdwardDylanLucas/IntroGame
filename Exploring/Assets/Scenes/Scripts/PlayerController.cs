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
    private enum DebugMode { Normal, Distance, Vision }
    private DebugMode currentMode = DebugMode.Distance;

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
        HandleDebugModeSwitch();

        switch (currentMode)
        {
            case DebugMode.Normal:
                PlayerPosition.text = "";
                PlayerVelocity.text = "";
                PlayerSpeed.text = "";
                ClosestPickUp.text = "";
                lineRenderer.enabled = false;
                ResetPickupsToWhite();
                break;
            case DebugMode.Distance:
                UpdateDistanceMode();
                break;
            case DebugMode.Vision:
                UpdateVisionMode();
                break;
        }
        //Vector3 currentPosition = player.transform.position;
        //Vector3 velocity = (currentPosition - oldPosition) / Time.deltaTime;
        //float speed = velocity.magnitude;

        //oldPosition = currentPosition;

        //PlayerPosition.text = "Player position: " + currentPosition.ToString();
        //PlayerVelocity.text = "Player velocity: " + velocity.ToString();
        //PlayerSpeed.text = "Player speed: " + speed.ToString();
        //UpdateClosestPickUp();
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

    void HandleDebugModeSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentMode = (DebugMode)(((int)currentMode + 1) % 3);
        }
    }

    void UpdateDistanceMode()
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

        Vector3 currentPosition = player.transform.position;
        Vector3 velocity = (currentPosition - oldPosition) / Time.deltaTime;
        float speed = velocity.magnitude;

        oldPosition = currentPosition;

        PlayerPosition.text = "Player position: " + currentPosition.ToString();
        PlayerVelocity.text = "Player velocity: " + velocity.ToString();
        PlayerSpeed.text = "Player speed: " + speed.ToString();
        lineRenderer.enabled = true;

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

    void UpdateVisionMode()
    {
        // Calculate player velocity
        Vector3 currentPosition = player.transform.position;
        Vector3 velocity = (currentPosition - oldPosition) / Time.deltaTime;
        float speed = velocity.magnitude;

        // Set line renderer for the velocity vector
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + velocity);

        // Find the pickup the player is moving towards
        GameObject targetedPickup = null;
        float closestAngle = Mathf.Infinity;

        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp.activeInHierarchy)
            {
                // Calculate the direction to the pickup
                Vector3 directionToPickup = (pickUp.transform.position - transform.position).normalized;

                // Calculate the angle between the player's velocity direction and the direction to the pickup
                float angle = Vector3.Angle(velocity, directionToPickup);

                // The smaller the angle, the more directly the player is moving towards this pickup
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    targetedPickup = pickUp;
                }

                // Reset all pickups to white and continue rotating
                pickUp.GetComponent<Renderer>().material.color = Color.white;
                pickUp.transform.Rotate(Vector3.up * Time.deltaTime * 45); // Continue rotating
            }
        }

        // Highlight the targeted pickup and make it face the player
        if (targetedPickup != null)
        {
            targetedPickup.GetComponent<Renderer>().material.color = Color.green;
            targetedPickup.transform.LookAt(transform.position); // Make the green pickup face the player

            PlayerPosition.text = "Player position: " + transform.position.ToString();
            PlayerVelocity.text = "Player velocity: " + velocity.ToString();
            PlayerSpeed.text = "Player speed: " + speed.ToString();
            ClosestPickUp.text = "Targeted PickUp: " + closestAngle.ToString();
        }
    }

    void ResetPickupsToWhite()
    {
        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp.activeInHierarchy)
            {
                pickUp.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

}
