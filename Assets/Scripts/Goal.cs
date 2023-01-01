using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [SerializeField]
    private string NextScene;

    public void CompleteLevel()
    {
        // We can do whatever we want here before completing the level

        // Go to the next level
        SceneManager.LoadScene(NextScene);
    }
}
