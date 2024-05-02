using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Ascend : MonoBehaviour
{
    [Header("Ascend Control Values")]
    public float maxAscendDist = 10f;
    //public float timeToAscendToCeiling = 1f;
    public float ascendMinTime = 0.5f;
    public float ascendMaxTime = 5f;
    public float distToMaxTime = 50f;
    public bool ascendMode = false;
    public bool isAscending = false;
    public bool waitingAtTop = false;
    public bool startExit = false;
    public bool startDescend = false;
    public AnimationCurve easingCurve;
    public GameObject ascendBackground;
    public GameObject descendBackground;
    public LayerMask playerLayer;

    [Header("Grid UI References")]
    public DecalProjector gridProjector;
    public Material greenGrid;
    public Material redGrid;

    private PlayerController playerController;
    private PlayerAnimController playerAnimController;
    private Vector3 ascendPoint;
    private Vector3 ceilingPoint;
    private Vector3 nextPotAscendPoint;
    private float playerHalfHeight;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAnimController = GetComponent<PlayerAnimController>();
        ascendBackground.SetActive(false);
        descendBackground.SetActive(false);
        gridProjector.enabled = false;
    }

    private void Start()
    {
        playerHalfHeight = playerController.PlayerHeight / 2;
        playerLayer = ~playerLayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (ascendMode && !isAscending)
        {
            ShowAscendUI();
        }
    }

    //Projects a grid UI element on the ceiling
    private void ShowAscendUI()
    {
        RaycastHit hit;
        Vector3 playerPos = transform.position + new Vector3(0, playerHalfHeight, 0);

        if (Physics.Raycast(playerPos, Vector3.up, out hit, Mathf.Infinity, playerLayer))
        {
            //Set canvas position to draw grid on ceiling
            gridProjector.transform.position = hit.point - new Vector3(0, 2f, 0);
            gridProjector.enabled = true;

            if (hit.distance <= maxAscendDist)
                gridProjector.material = greenGrid; //Can Ascend here
            else
                gridProjector.material = redGrid; //Cannot Ascend here
        }
        else
            gridProjector.enabled = false; //No surface to draw UI on
    }

    public void AttemptToAscend()
    {
        //if in ascend mode, not currently ascending, and able to ascend, start ascending
        if(ascendMode && !isAscending && CanAscend())
        {
            Vector3 ascendPoint = GetAscendPoint();

            if (ascendPoint != Vector3.zero)
                AscendToPoint(ascendPoint);
            else
                Debug.Log("Failed to Ascend");
        }
        //if waiting at the top - exit the ground
        else if(waitingAtTop)
        {
            startExit = true;
        }
    }

    private void AscendToPoint(Vector3 ascendPoint)
    {
        CameraController.Instance.ActivateMainCam();
        gridProjector.enabled = false;
        AbilitiesMenu.Instance.ascendControlsPanel.SetActive(false);

        float ascendToCeilingTime = GetAscendToCeilingTime(transform.position, ceilingPoint);
        float ascendThroughCeilingTime = GetAscendThroughCeilingTime(ceilingPoint, ascendPoint);
        Vector3 pointB = ceilingPoint - new Vector3(0, playerHalfHeight, 0);
        Vector3 pointC = ascendPoint - new Vector3(0, playerHalfHeight, 0);

        StartCoroutine(AscendLerp(transform.position, pointB, pointC, ascendToCeilingTime, ascendThroughCeilingTime));
    }

    /// <summary>
    /// Gets and returns the amount of time to ascend through the ceiling based on the distance
    /// </summary>
    /// <param name="pointA">The ceiling above the player</param>
    /// <param name="pointB">The end position the player will ascend to</param>
    /// <returns></returns>
    private float GetAscendToCeilingTime(Vector3 pointA, Vector3 pointB)
    {
        //Calculate distance to ascend to the ceiling - clamp to maximum
        float distToTravel = Mathf.Clamp(Vector3.Distance(pointA, pointB), 0, maxAscendDist);

        //Calculate the time to ascend
        float time = Mathf.Lerp(.25f, 1.25f, distToTravel / maxAscendDist);

        //Return time to ascend
        return time;
    }

    /// <summary>
    /// Gets and returns the amount of time to ascend through the ceiling based on the distance
    /// </summary>
    /// <param name="pointB">The ceiling above the player</param>
    /// <param name="pointC">The end position the player will ascend to</param>
    /// <returns></returns>
    private float GetAscendThroughCeilingTime(Vector3 pointB, Vector3 pointC)
    {
        //Calculate distance to ascend through the ceiling - clamp to maximum
        float distToTravel = Mathf.Clamp(Vector3.Distance(pointB, pointC), 0, distToMaxTime);

        //Calculate the time to ascend
        float time = Mathf.Lerp(ascendMinTime, ascendMaxTime, distToTravel / distToMaxTime);

        //Return time to ascend
        return time;
    }

    //Gets the top position of the platform to ascend though and end up at
    private Vector3 GetAscendPoint()
    {
        ascendPoint = Vector3.zero;
        Vector3 playerPos = transform.position + new Vector3(0, 1, 0);

        RaycastHit hit;
        Vector3 raycastOrigin = playerPos + new Vector3(0, 10000f, 0);
     
        //Raycast down from the sky to see the top-most piece of geometry above the player
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, Mathf.Infinity, playerLayer))
        {
            ascendPoint = hit.point;
            Debug.DrawLine(raycastOrigin, hit.point, Color.yellow);
        }

        nextPotAscendPoint = ascendPoint;

        //Keep checking if a lower platform exists above the player
        while (nextPotAscendPoint.y > playerPos.y)
        {
            //Raycast down from the last discovered ascendPoint
            if (Physics.Raycast(ascendPoint - new Vector3(0, 0.5f, 0), Vector3.down, out hit, Mathf.Infinity, playerLayer))
            {
                nextPotAscendPoint = hit.point;           

                //Check if the new hit point is greater than the player's y pos
                if (nextPotAscendPoint.y > playerPos.y)
                {
                    Debug.DrawLine(ascendPoint, nextPotAscendPoint, Color.blue);
                    ascendPoint = nextPotAscendPoint;
                }
                else
                    Debug.DrawLine(ascendPoint, nextPotAscendPoint, Color.magenta);
            }
            else
            {
                nextPotAscendPoint.y = ascendPoint.y - 10000f;
                Debug.DrawLine(ascendPoint, Vector3.down * 10000, Color.red);
            }
        }

        return ascendPoint;
    }

    //Raycasts up and returns true if able to ascend at this point
    private bool CanAscend()
    {
        bool ascendable = false;

        RaycastHit hit;
        Vector3 playerPos = transform.position + new Vector3(0, 1, 0);

        if (Physics.Raycast(playerPos, Vector3.up, out hit, Mathf.Infinity, playerLayer))
        {
            if(hit.distance <= maxAscendDist)
            {
                ascendable = true;
                ceilingPoint = hit.point;
                Debug.DrawLine(playerPos, hit.point, Color.green);
            }
            //Debug.DrawLine(playerPos, 10000 * Vector3.up, Color.red);
        }
        //else
            //Debug.DrawLine(playerPos, 10000 * Vector3.up, Color.red);

        return ascendable;
    }

    /// <summary>
    /// Lerps the player upwards to Ascend
    /// </summary>
    /// <param name="pointA">Player's starting position on the ground</param>
    /// <param name="pointB">The ceiling above the player</param>
    /// <param name="pointC">The end position the player will ascend to</param>
    /// <param name="timeAB">The time to lerp from pointA to pointB</param>
    /// <param name="timeBC">The time to lerp from pointB to pointC</param>
    /// <returns></returns>
    IEnumerator AscendLerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, float timeAB, float timeBC)
    {
        float timeElapsed = 0;
        playerController.canMove = false;
        playerController.canUseGravity = false;
        isAscending = true;
        playerAnimController.SetAscendingState(true);

        //Pause before moving to ceiling
        yield return new WaitForSeconds(0.75f);

        //Lerp player from ground to ceiling
        while (timeElapsed < timeAB)
        {
            float u = timeElapsed / timeAB;
            transform.position = Vector3.Lerp(pointA, pointB, u); //easingCurve.Evaluate(u)

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = pointB;

        //Pause before moving through ceiling
        yield return new WaitForSeconds(1f);
        timeElapsed = 0;

        //Activate the ascend background & particles
        ascendBackground.SetActive(true);
        CameraController.Instance.AscendRendering();

        //Lerp player from ceiling to ascend point
        while (timeElapsed < timeBC)
        {
            float u = timeElapsed / timeBC;
            transform.position = Vector3.Lerp(pointB, pointC, u);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = pointC;

        //Deactivate the ascend background & particles
        ascendBackground.SetActive(false);
        CameraController.Instance.RenderEverything();

        //Display Exit Controls
        AbilitiesMenu.Instance.ascendExitPanel.SetActive(true);

        playerAnimController.SetAscendingState(false);
        playerAnimController.SetMoveSpeed(0);

        //Pause before exiting ceiling
        waitingAtTop = true;
        while(waitingAtTop)
        {
            if(startExit)
            {
                waitingAtTop = false;
                StartCoroutine(ExitGround(pointC, 0.5f));
                startExit = false;
            }
            else if(startDescend)
            {
                waitingAtTop = false;
                StartCoroutine(DescendLerp(pointC, pointA, timeBC));
                startDescend = false;
            }
            yield return null;
        }

        //Hide Exit Controls
        AbilitiesMenu.Instance.ascendExitPanel.SetActive(false);
    }

    //Lerp player to stand on ground of ascend point
    private IEnumerator ExitGround(Vector3 pointC, float exitTime)
    {
        playerAnimController.SetAscendingState(true);

        float timeElapsed = 0;
        Vector3 finalPoint = pointC + new Vector3(0, playerHalfHeight, 0);

        while (timeElapsed < exitTime)
        {
            float u = timeElapsed / exitTime;
            transform.position = Vector3.Lerp(pointC, finalPoint, u);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = finalPoint;

        //Exit ascending state
        playerController.canMove = true;
        playerController.canUseGravity = true;
        isAscending = false;
        playerAnimController.SetAscendingState(false);
        ascendMode = false;
    }

    //Lerp player from ascend point back to original ground point
    IEnumerator DescendLerp(Vector3 pointA, Vector3 pointB, float timeAB)
    {
        float timeElapsed = 0;
        playerAnimController.SetAscendingState(true);

        //Activate the ascend background & particles
        descendBackground.SetActive(true);
        CameraController.Instance.AscendRendering();

        //Lerp player from ground to ceiling
        while (timeElapsed < timeAB)
        {
            float u = timeElapsed / timeAB;
            transform.position = Vector3.Lerp(pointA, pointB, u); //easingCurve.Evaluate(u)

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = pointB;

        //Deactivate the ascend background & particles
        descendBackground.SetActive(false);
        CameraController.Instance.RenderEverything();

        //Exit ascending state
        playerController.canMove = true;
        playerController.canUseGravity = true;
        isAscending = false;
        playerAnimController.SetAscendingState(false);
        ascendMode = false;
    }
}
