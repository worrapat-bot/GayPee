using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    public Slider progressBar;
    public AudioSource audioSource;
    public AudioClip loadedSound;
    public float waitBeforeEnter = 3f; // 🕒 หน่วงเวลา (วินาที) ก่อนเข้าเกม

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

            // โหลดครบ 90% แปลว่าโหลดเสร็จ รอให้ถึงเวลาเข้าเกม
            if (op.progress >= 0.9f)
            {
                // เล่นเสียงเมื่อโหลดเสร็จ (ถ้ามี)
                if (audioSource && loadedSound && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(loadedSound);
                }

                // ✅ หน่วงเวลารอ ก่อนเข้าเกม
                yield return new WaitForSeconds(waitBeforeEnter);

                // ✅ ถ้ามีเสียง รอจนเสียงเล่นจบก่อนเข้าเกม
                if (audioSource && audioSource.isPlaying)
                    yield return new WaitWhile(() => audioSource.isPlaying);

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
