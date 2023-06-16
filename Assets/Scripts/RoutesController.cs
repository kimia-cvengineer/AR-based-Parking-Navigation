namespace Mapbox.Unity.MeshGeneration.Factories
{
	using UnityEngine;
	using UnityEngine.Networking;
	using UnityEngine.UI;
	using Mapbox.Directions;
	using System.Collections.Generic;
	using System;
	using System.Linq;
	using Modifiers;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using System.Collections;
	using UnityEngine.XR.ARFoundation;
	using UnityEngine.XR.ARSubsystems;

	public class RoutesController : MonoBehaviour
	{

		//[SerializeField]
		//private Vector2d org = new Vector2d(34.4181673, -119.8567870); // Starting point coordinates

		[SerializeField]
		private Vector2d dest; // Destination coordinates
		// private Vector2d dest = new Vector2d(34.4311653, -119.7558367); // Destination coordinates

		[SerializeField]
		[Range(1, 10)]
		private float UpdateFrequency = 2;

		[Header("UI Elements")]
		public Transform HeadingPanel;
		public GameObject InfoPanel;
		public Text DistanceText;


		private Directions _directions;
		private int _counter;
		//public bool isNewStep = false;

		GameObject _directionsGO;
		private bool _recalculateNext;

		/*		public Text headingText;
				public Text locationText;
				public Text bearingText;
				public Text distanceText;*/
		float lat;
		float lon;
		float currStepLastLat;
		float currStepLastLon;
		int count = 0;
		List<Step> steps;
		public Step currStep = null;

		float brng;
		float compassBrng;



		bool open = true;

		//private ArrowsController headingArrows;
		private Vector3 arrowHeading;
		private bool _waitingForLocationService = false;
		private float _maxInitWaitTime = 4f;
		private float _elapsedTime = 0f;
		private float _approximityThresh = 0.0005f;
		private bool _isRouting = false;
		private RequestController requestController;

		//public delegate void NewStep(Step step, Vector2d lastPoint);
		//public event NewStep EnteredNewStep;
		public event Action<Step, Vector2d> EnteredNewStep;
		public event Action<Vector2d> Arrived;

		public enum ManeuverDirections
		{
			UTurn,
			Straight,
			Right,
			Left,
			RoundAbout,
			None
		}



		/// <summary>
		/// Callback handling "FindRoute" button click event.
		/// </summary>
		public void OnFindRouteClicked()
		{
			Debug.Log("Querying OnFindRouteClicked");
			requestController.requestRoute();
			Query();
		}

		private void OnParkingSlotDataReceived(RequestController.ParkingSlot slot)
        {
			dest = new Vector2d(slot.latitude, slot.longitude);
			Debug.Log("*** OnParkingSlotDataReceived ***");
			Query();
        }

		/// <summary>
		/// Unity's OnEnable() method.
		/// </summary>
		public void OnEnable()
		{
			Debug.Log("**** In OnEnable RoutesController****");
			//HeadingPanel.gameObject.SetActive(false);
			requestController = GetComponent<RequestController>();
			requestController.RequestDataReceived += OnParkingSlotDataReceived;

		}

		/// <summary>
		/// Unity's OnDisable() method.
		/// </summary>
		public void OnDisable()
		{
			Debug.Log("**** In OnDisable RoutesController ****");
			requestController.RequestDataReceived -= OnParkingSlotDataReceived;
		}

		bool isCollide()
		{
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;
			if (lat - currStepLastLat <= _approximityThresh && lat - currStepLastLat >= -_approximityThresh)
			{
				if (lon - currStepLastLon <= _approximityThresh && lon - currStepLastLon >= -_approximityThresh)
				{
					return true;
				}
			}

			return false;
		}

		float distance_metres(float lat1, float lon1, float lat2, float lon2)
		{
			float R = 6378.137f; // Radius of Earth in KM
			float dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
			float dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
			float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) + Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) * Mathf.Sin(dLon / 2)
				* Mathf.Sin(dLon / 2);
			float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
			float d = R * c;
			return d * 1000f;
		}

		// Update is called once per frame
		void Update()
		{
			//check if user has location service enabled
			if (!Input.location.isEnabledByUser)
			{
				//re-route
				Debug.LogError("Your location is diabled. Please give a permission.");
				return;
			}

			// Wait until service initializes
			if (Input.location.status == LocationServiceStatus.Initializing && _maxInitWaitTime > _elapsedTime)
				_elapsedTime += Time.deltaTime;
			else
				_elapsedTime = 0f;

			//Service didn't initialize in 4 seconds
			if (_elapsedTime >= _maxInitWaitTime)
			{
				Debug.Log("Timed out");
				return;
			}

			// Connection has failed
			if (Input.location.status == LocationServiceStatus.Failed)
			{
				Debug.Log("Unable to determine device location");
				return;
			}
			else
			{
				//arrowHeading = new Vector3(36.953f, 0, -(brng - compassBrng));
				//this.transform.eulerAngles = arrowHeading;
				// re-route
				if (_isRouting)
					updateRoute();
			}
		}

		

		protected virtual void Awake()
		{
			_directions = MapboxAccess.Instance.Directions;
			Debug.Log("**** In Awake RoutesController****");
		}

		public void Start()
		{
			Debug.Log("**** In Start RoutesController****");
			//Query();
		}

		void Query()
		{

			Debug.Log("*** Querying ***");
			// Get route
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;
			Vector2d org = new Vector2d(lat, lon);
			var wp = new[] {
				org, dest
			};
			var _directionResource = new DirectionResource(wp, RoutingProfile.Driving);
			_directionResource.Steps = true; // Include turn-by-turn steps in the response
			_directionResource.Alternatives = true; // Request multiple route alternatives
			_directions.Query(_directionResource, HandleDirectionsResponse);

		}


		void HandleDirectionsResponse(DirectionsResponse response)
		{
			Debug.Log("In HandleDirectionsResponse");
			if (response == null || null == response.Routes || response.Routes.Count < 1)
			{
				_isRouting = false;
				steps = null;
				return;
			}

			// Get the best route from the response
			var route = response.Routes[0];

			// Assume that we always have one leg (no intermediate waypoints)
			var leg = route.Legs[0];

			// Setup steps
			steps = leg.Steps;
			// remove the last step'slast coordinate so that
			// we can distinguish among prefab of the destination

			Debug.Log($"Last step before removing: {steps.Last().Geometry.Count}");
			var lastStepCoords = steps.Last().Geometry;
			lastStepCoords.RemoveAt(lastStepCoords.Count - 1);
			Debug.Log($"Last step after removing: {steps.Last().Geometry.Count}");

			Debug.Log($"In HandleDirectionsResponse: curr step {steps.Count}");
			currStep = steps[0];

			Debug.Log($"In HandleDirectionsResponse: curr step {currStep}");
			//HeadingPanel.gameObject.SetActive(true);
			//InfoPanel.gameObject.SetActive(true);
			Debug.Log($"In HandleDirectionsResponse: activating panels");

			updateNewStep();
			for (int j = 0; j < steps.Count; j++)
			{
				var step = steps[j];
				Debug.Log($"Step {j + 1}\n" +
							$"Instruction: {step.Maneuver.Instruction}\n" +
							$"Type: {step.Maneuver.Type}\n" +
							$"Modifier: {step.Maneuver.Modifier}\n" +
							$"Distance: {step.Distance} meters\n" +
							$"Duration: {step.Duration} seconds");
				// Access the geographical coordinates of the step
				Debug.Log($"Step Coordinates: {string.Join(", ", step.Geometry)}");

			}

			_isRouting = true;

			Debug.Log($"Last step's coord by list: {lastStepCoords.Last().x}," +
				$" {lastStepCoords.Last().y}");
			Debug.Log($"Last step's coord by dest: {dest.x}, {dest.y}");
			Debug.Log("Invoking New Step Event!");
			Debug.Log($"Steps counts : {steps.Count}");
			Debug.Log($"Steps finished");
            if (steps.Count > 1)
                EnteredNewStep?.Invoke(currStep, steps[1].Geometry[0]);
            else
                EnteredNewStep?.Invoke(currStep, dest);
			Debug.Log("Invoked New Step Event!");

		}


		void updateRoute()
		{
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;
			Debug.Log("+++++++++++ Preparing for step info +++++++++++");
			Debug.Log($"Preparing for step info step count : {count}");
			if (count > steps.Count - 1)
				return;
			currStep = steps[count];

			// constantly update distance shown
			int distance = Mathf.RoundToInt(distance_metres(lat, lon, currStepLastLat, currStepLastLon));
			DistanceText.gameObject.SetActive(true);
			DistanceText.text = Math.Round((distance * 0.0006), 2).ToString() + " mi";
			Debug.Log($"Preparing for step distance : {DistanceText.text}");
			Debug.Log($"Preparing for step distance active : {DistanceText.IsActive()}");

			if (isCollide())
			{
				Debug.Log("+++++++++++ Colliding +++++++++++");
				// firstPointOfnNextStep used for heading of the last arrow in the step
				var firstPointOfnNextStep = new Vector2d();
				count++;
				if (count < steps.Count - 1)
					firstPointOfnNextStep = steps[count + 1].Geometry[0];
				else
					// destination is the last coordinate of the next step
					firstPointOfnNextStep = dest;

				updateNewStep();
				EnteredNewStep?.Invoke(steps[count], firstPointOfnNextStep);
			}

		}

		private void updateNewStep()
		{
			currStep = steps[count];
			if (currStep != null)
			{
				// Get current step coordinates
				List<Vector2d> stepCoordinates = currStep.Geometry;

				currStepLastLat = (float)stepCoordinates.Last().x;
				currStepLastLon = (float)stepCoordinates.Last().y;

				Debug.Log("currStepLastLat: " + currStepLastLat + ", currStepLastLon: " + currStepLastLon);


				// Display step information on the panel
				Debug.Log("+++++++++++ Displaying step information on the panel +++++++++++");
				var texts = InfoPanel.GetComponentsInChildren<Text>();
				Debug.Log($"Text count: {texts.Length}");
				Debug.Log($"Text Active: {InfoPanel.gameObject.activeInHierarchy}");
				texts[2].text = $"Maneuver Type: {steps[count].Maneuver.Type}";
				string modifier = steps[count].Maneuver.Modifier;
				// texts[4].text = $"Maneuver Modifier: {(string.IsNullOrEmpty(modifier) ? "None" : modifier)}";
				texts[3].text = "Dest Coords: " +
						(float)dest.x + ", " + (float)dest.y;

				/*texts[3].text = "Dest Coords: " +
						stepCoordinates.Last().x + ", " + stepCoordinates.Last().y;*/
				if (count < steps.Count)
				{
					texts[1].text = steps[count].Maneuver.Instruction; // description
					texts[0].text = "Step " + (count + 1) + " / " + steps.Count;

				}

				if (count == steps.Count-1)
				{
					Debug.Log("You have arrived!");
					texts[0].text = "No further steps"; // step
					texts[1].text = "You have arrived!"; // description
					_isRouting = false;
					Arrived?.Invoke(dest);

				}

			}
			else
				Debug.Log("In updateNewStep: Curr step Is NULL");
		}

		public ManeuverDirections getStepDirection()
        {
			string mType = currStep.Maneuver.Type;
			string mModifier = currStep.Maneuver.Modifier;
			switch (mType)
			{
				case "depart":
				case "arrive":
					return ManeuverDirections.None;
				case "turn":
				case string s when mType.Contains("ramp"):
					if (mModifier.Contains("right"))
						return ManeuverDirections.Right;
					else
						return ManeuverDirections.Left;
				case "roundabout":
					return ManeuverDirections.RoundAbout;
				default:
					return ManeuverDirections.None;
			}

		}

		public void getDestinationLocation()
        {
			// fetch closest parking spot location
			/*var parkingLoc = queryParkingLocation();
			return parkingLoc;*/


		}
	}
}