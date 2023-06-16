using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mapbox.Utils;
using Mapbox.Directions;
/*using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;*/
using Mapbox.Unity.MeshGeneration.Factories;

using ManeuverDirections = Mapbox.Unity.MeshGeneration.Factories.RoutesController.ManeuverDirections;


public class DirectionController : MonoBehaviour
{
    [Header("UI Components")]

    /// <summary>
    /// UI element containing all waypoints in a step route.
    /// </summary>
    public GameObject navRouteView;

    private RoutesController routeController;

    public void Awake()
    {
        routeController = navRouteView.GetComponent<RoutesController>();
    }

    public void OnEnable()
    {
        Debug.Log("********* In OnEnable DirectionController ********");
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform child = this.transform.GetChild(i);
            if (child.name.Contains("orange"))
                child.gameObject.SetActive(false);
        }
        routeController.EnteredNewStep += HandleHeadingArrow;
    }

    public void OnDisable()
    {
        routeController.EnteredNewStep -= HandleHeadingArrow;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleHeadingArrow(Step currStep, Vector2d lastPoint)
    {
        Debug.Log($"In HandleHeadingArrow first line");
        var direction = routeController.getStepDirection();
        Debug.Log($"In HandleHeadingArrow second line");
        Transform headingPanelT = this.transform;
        Debug.Log($"ManeuverDirections : {direction}");
        for (int i= 0; i < headingPanelT.childCount; i++)
        {
            Transform child = headingPanelT.GetChild(i);
            if (child.name.Contains("orange"))
                child.gameObject.SetActive(false);
        }
        Debug.Log($"In HandleHeadingArrow before switch");
        switch (direction)
        {
            case ManeuverDirections.None:
                headingPanelT.Find("orange_straight").gameObject.SetActive(true);
                break;
            case ManeuverDirections.Straight:
                headingPanelT.Find("orange_straight").gameObject.SetActive(true);
                break;
            case ManeuverDirections.Right:
                headingPanelT.Find("orange_right").gameObject.SetActive(true);
                break;
            case ManeuverDirections.Left:
                headingPanelT.Find("orange_left").gameObject.SetActive(true);
                break;
            case ManeuverDirections.UTurn:
                headingPanelT.Find("orange_uturn_left").gameObject.SetActive(true);
                break;
            case ManeuverDirections.RoundAbout:
                headingPanelT.Find("orange_square").gameObject.SetActive(true);
                break;
            default:
                Debug.Log("ManeuverDirections.default");
                break;
        }
        Debug.Log($"In HandleHeadingArrow Placed Arrow");
    }
}
