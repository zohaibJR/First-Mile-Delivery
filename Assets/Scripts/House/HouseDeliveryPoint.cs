using UnityEngine;

public class HouseDeliveryPoint : MonoBehaviour
{
    public OrderUIController UIcontroller;
    [Header("This House Address")]
    public HouseAddress houseAddress;   // Enum dropdown

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package"))
        {
            PackageData package = other.GetComponent<PackageData>();

            if (package == null) return;

            Debug.Log("Package detected at " + houseAddress);

            // Compare addresses
            if (package.address == houseAddress)
            {
                package.status = PackageStatus.Delivered;

                Debug.Log($"✔ ORDER COMPLETED: Package {package.packageID} delivered to {houseAddress}");
                UIcontroller.ActivateOrderCompleteText();

            }
            else
            {
                Debug.Log($"❌ WRONG HOUSE! Package belongs to {package.address}, not {houseAddress}");
            }
        }
    }
}
