using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SpaceTransition : MonoBehaviour
{
    public GameObject labEnvironment;
    public GameObject mainLight;
    public GameObject spaceLightTunnel;
    public float lightDuration = 5f;
    public string nextSceneName = "Scene_TubesMusicaux";

    public void EnterSpaceMode()
    {
        if (labEnvironment != null) labEnvironment.SetActive(false);

        if (mainLight != null) mainLight.SetActive(false);

        RenderSettings.skybox = null;

        RenderSettings.ambientLight = Color.black;

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }

        if (spaceLightTunnel != null) spaceLightTunnel.SetActive(true);
        StartCoroutine(WaitAndLoadNextScene());
    }

    private IEnumerator WaitAndLoadNextScene()
    {
        yield return new WaitForSeconds(lightDuration);

        SceneManager.LoadScene(nextSceneName);
    }
}