using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RequestController : MonoBehaviour
{
    public event Action<ParkingSlot> RequestDataReceived;
    private string _URL = "http://52.90.238.67/slots?status=true";

    // Use this for initialization
    public void Start()
    {
        Debug.Log($"In start RequestController");
        string query = PlayerPrefs.GetString("destQuery");
        Debug.Log($"Start request controller: query {query}");
        if (!string.IsNullOrEmpty(query))
        {
            Debug.Log("Invoking in start of RequestController");
            var coordinate = ParseLocation(query);
            Debug.Log("Passring Loc RequestController");
            RequestDataReceived?.Invoke(new ParkingSlot
            {
                slot_id = "441515",
                latitude = (float)coordinate[0],
                longitude = (float)coordinate[1],
                is_empty = true
            });

        }
    }

    public void requestRoute()
    {
        Debug.Log("requestRoute");
        StartCoroutine(Query());
    }

    private IEnumerator Query()
    {
        Debug.Log("In Query");
        // Request free parking slots using UnityWebRequest
        using (UnityWebRequest www = UnityWebRequest.Get(_URL))
        {
            Debug.Log("Requesting");

            // Send the request and wait for a response
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error getting requested data: " + www.error);
            }
            else
            {
                // Request successful, you can now access the result
                string result = www.downloadHandler.text;
                Debug.Log("*** FETCHED Result: " + result + "***");
                ParkingData data = JsonUtility.FromJson<ParkingData>(result);
                if (data != null)
                {
                    string date_time = data.date_time;
                    string parkingName = data.parking;
                    List<ParkingSlot> slots = data.slots;

                    Debug.Log("Date-Time: " + date_time);
                    Debug.Log("Parking: " + parkingName);

                    if (slots != null)
                    {
                        Debug.Log("Slots count: " + slots.Count);

                        foreach (ParkingSlot slot in slots)
                        {
                            Debug.Log("Slot ID: " + slot.slot_id);
                            Debug.Log("Slot Latitude: " + slot.latitude);
                            Debug.Log("Slot Longitude: " + slot.longitude);
                            Debug.Log("Slot Is Empty: " + slot.is_empty);
                        }

                        // TODO
                        var closestSlot = getClosestSlot(slots);
                        // TODO : temporarily
                        /*closestSlot.longitude = (float)-119.871168;
                        closestSlot.latitude = (float)34.428269;*/
                        closestSlot.longitude = (float)-119.8570294;
                        closestSlot.latitude = (float)34.4179562;
                        RequestDataReceived?.Invoke(closestSlot);
                    }
                    else
                    {
                        Debug.Log("No slots found in the data.");
                    }
                }
                else
                {
                    Debug.Log("Failed to parse the JSON data.");
                }

            }
        }
    }

    private ParkingSlot getClosestSlot(List<ParkingSlot> slots)
    {
        // TODO
        return slots[0];
    }


    private float[] ParseLocation(string location)
    {
        // Remove all spaces from the input string
        string cleanedLocation = location.Replace("%20", "");
        Debug.Log($"In ParseLocation, cleanedLocation: {cleanedLocation}");
        string[] coordinates = cleanedLocation.Split(',');
        Debug.Log($"In ParseLocation coordinates: {coordinates[0]}, {coordinates[1]}");
        // Removing leading and trailing spaces from latitude and longitude
        string latitudeString = coordinates[0].Trim();
        string longitudeString = coordinates[1].Trim();
        Debug.Log($"In ParseLocation Trimmed coordinates: {latitudeString}, {longitudeString}");

        // Parsing latitude and longitude as float values
        float latitude, longitude;
        if (float.TryParse(latitudeString, out latitude) && float.TryParse(longitudeString, out longitude))
        {
            // Successfully parsed latitude and longitude
            Debug.Log("Latitude: " + latitude);
            Debug.Log("Longitude: " + longitude);
            return new float[] { latitude, longitude };
        }
        else
        {
            // Failed to parse latitude and longitude
            Debug.LogError("Invalid latitude or longitude format.");
            return new float[] { };
        }

        
    }

    [System.Serializable]
    public class ParkingSlot
    {
        public string slot_id;
        public float latitude;
        public float longitude;
        public bool is_empty;
    }

    [System.Serializable]
    private class ParkingData
    {
        public string date_time;
        public string parking;
        public List<ParkingSlot> slots;
    }



}
