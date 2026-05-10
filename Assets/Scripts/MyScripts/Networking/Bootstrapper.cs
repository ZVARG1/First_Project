using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        // Add logic here if you need to wait for Steam to init
        // Otherwise, just jump to the Menu
        SceneManager.LoadScene("Scene_MainMenu");
    }
}