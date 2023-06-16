
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnHomeButtonClicked()
    {
        Debug.Log("*** In OnHomeButtonClicked *****");
        SceneManager.LoadScene("MainScene");
    }
}
