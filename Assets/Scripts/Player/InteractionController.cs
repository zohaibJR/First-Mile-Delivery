using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Camera playerCamera;
    public float interactDistance = 2f;
    public LayerMask interactLayer;

    [Header("Interactable Tags")]
    public List<string> detectableTags = new List<string>() { "Package", "Door" };

    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float pickupSmoothness = 15f;

    private GameObject heldObject = null;
    private Rigidbody heldRb = null;

    [Header("UI")]
    public GameObject PickupText;
    public GameObject dropText;
    private PackageDetailUI packageUI;

    public PlayerPickupManager playerPickupManager;

    private bool isLookingAtPackage = false;


    void Start()
    {
        packageUI = FindObjectOfType<PackageDetailUI>();
    }


    void Update()
    {
        if (heldObject == null)
        {
            DetectObject();
        }
        else
        {
            HoldObject();
            DropCheck();
        }
    }


    // --------------------------------------------------------------
    // RAYCAST + DETECTION
    // --------------------------------------------------------------
    void DetectObject()
    {
        PickupText.SetActive(false);   // hide by default

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        int layerMask = (interactLayer.value == 0) ? ~0 : interactLayer;

        if (Physics.Raycast(ray, out hit, interactDistance, layerMask))
        {
            string hitTag = hit.collider.tag;

            // ------------------------------
            // SHOW DETAILS FOR PACKAGE
            // ------------------------------
            if (hitTag == "Package")
            {
                ShowPackageDetails(hit);
                HandlePickup(hit);
                return;  // skip "looking nowhere"
            }

            // ------------------------------
            // ANY OTHER DETECTABLE OBJECT
            // ------------------------------
            if (detectableTags.Contains(hitTag))
            {
                PickupText.SetActive(true);
                Debug.Log("Raycast on: " + hitTag);
                isLookingAtPackage = false;

                packageUI.DectivatePanel(); // hide UI on non-package objects
                return;
            }
        }

        // ------------------------------------------------------
        // LOOKING NOWHERE
        // ------------------------------------------------------
        if (isLookingAtPackage)
        {
            Debug.Log("Looking nowhere");
            packageUI.DectivatePanel();
            isLookingAtPackage = false;
        }
    }



    // --------------------------------------------------------------
    // SHOW PACKAGE DETAILS
    // --------------------------------------------------------------
    void ShowPackageDetails(RaycastHit hit)
    {
        PickupText.SetActive(true);

        PackageData data = hit.collider.GetComponent<PackageData>();
        if (data != null)
        {
            Debug.Log($"Looking at Package {data.packageID}");
            packageUI.ActivatePanel(data);
        }

        isLookingAtPackage = true;
    }



    // --------------------------------------------------------------
    // PICKUP WITH E
    // --------------------------------------------------------------
    void HandlePickup(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PickupObject(hit.collider.gameObject);
            PickupText.SetActive(false);
            packageUI.DectivatePanel();
        }
    }



    // --------------------------------------------------------------
    // PICKUP LOGIC
    // --------------------------------------------------------------
    void PickupObject(GameObject obj)
    {
        dropText.SetActive(true);
        heldObject = obj;
        heldRb = obj.GetComponent<Rigidbody>();

        // disable collider
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // update status
        PackageData data = obj.GetComponent<PackageData>();
        if (data != null)
        {
            data.status = PackageStatus.Picked;
            Debug.Log($"Picked package {data.packageID}");
        }

        if (heldRb != null)
        {
            heldRb.useGravity = false;
            heldRb.isKinematic = true;
        }

        obj.transform.SetParent(holdPosition);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        playerPickupManager.ObjectPickedUp();
    }



    // --------------------------------------------------------------
    // HOLD OBJECT
    // --------------------------------------------------------------
    void HoldObject()
    {
        heldObject.transform.localPosition = Vector3.Lerp(
            heldObject.transform.localPosition,
            Vector3.zero,
            Time.deltaTime * pickupSmoothness
        );
    }



    // --------------------------------------------------------------
    // DROP SYSTEM
    // --------------------------------------------------------------
    void DropCheck()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropObject();
        }
    }

    void DropObject()
    {
        dropText.SetActive(false);

        if (heldObject == null) return;

        Collider col = heldObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        PackageData data = heldObject.GetComponent<PackageData>();
        if (data != null)
            data.status = PackageStatus.Waiting;

        heldObject.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        heldObject = null;
        heldRb = null;

        playerPickupManager.ObjectDropped();
    }
}
