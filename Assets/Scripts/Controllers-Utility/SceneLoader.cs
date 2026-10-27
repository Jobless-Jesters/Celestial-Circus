using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        GameController.Instance.LoadSceneByName(sceneName); // Takes the scene name
    }
}
