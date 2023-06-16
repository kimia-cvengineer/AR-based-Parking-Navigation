using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Mapbox.Unity.MeshGeneration.Factories;


public class DestinationInfoManager : MonoBehaviour {

    public static DestinationInfoManager Instance { get; private set; }
    public string query;

/*    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the script exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

    // Use this for initialization
    public void Start () {
        //DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	public void Update () {
		
	}
    public void Go()
    {
        Debug.Log("On Go btn clicked");
        InputField destField = GameObject.FindGameObjectWithTag("DestField").GetComponent<InputField>();

        if (destField != null)
        {
            query = destField.text;

            var arr = query.Split(' ');
            query = string.Join("%20", arr);
            Debug.Log($"Dest query: {query}");
            PlayerPrefs.SetString("destQuery", query);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Destination field not found!");
        }

        if (!string.IsNullOrEmpty(query))
        {
            SceneManager.LoadScene("NavScene");
        }
    }


}
