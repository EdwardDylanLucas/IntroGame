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

        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp.activeInHierarchy)
            {
                float distanceToPickUp = Vector3.Distance(transform.position, pickUp.transform.position);

                if (distanceToPickUp < closestDistance)
                {
                    closestDistance = distanceToPickUp;
                    closestPickUp = pickUp;
                }

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

        if (closestPickUp != null)
        {
            closestPickUp.GetComponent<Renderer>().material.color = Color.blue;
            ClosestPickUp.text = "Closest PickUp Distance: " + closestDistance.ToString();
           
            lineRenderer.SetPosition(0, player.transform.position);
            
            lineRenderer.SetPosition(1, closestPickUp.transform.position);
            
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
        Vector3 currentPosition = player.transform.position;
        Vector3 velocity = (currentPosition - oldPosition) / Time.deltaTime;
        float speed = velocity.magnitude;

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + velocity);

        GameObject targetedPickup = null;
        float closestAngle = Mathf.Infinity;

        foreach (GameObject pickUp in pickUps)
        {
            if (pickUp.activeInHierarchy)
            {
                Vector3 directionToPickup = (pickUp.transform.position - transform.position).normalized;

                float angle = Vector3.Angle(velocity, directionToPickup);

                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    targetedPickup = pickUp;
                }

                pickUp.GetComponent<Renderer>().material.color = Color.white;
                pickUp.transform.Rotate(Vector3.up * Time.deltaTime * 45); 
            }
        }

        if (targetedPickup != null)
        {
            targetedPickup.GetComponent<Renderer>().material.color = Color.green;
            targetedPickup.transform.LookAt(transform.position);

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
