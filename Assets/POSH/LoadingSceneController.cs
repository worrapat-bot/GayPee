using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public Slider progressBar;
    public AudioSource audioSource;
    public AudioClip loadedSound;

    void Start()
    {
        string nextScene = PlayerPrefs.GetString("NextSceneToLoad", "GameScene");
        StartCoroutine(LoadAsync(nextScene));
    }

    private System.Collections.IEnumerator LoadAsync(string nextScene)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar) progressBar.value = progress;

            if (op.progress >= 0.9f)
            {
                if (audioSource && loadedSound && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(loadedSound);
                    yield return new WaitForSeconds(loadedSound.length);
                }

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}