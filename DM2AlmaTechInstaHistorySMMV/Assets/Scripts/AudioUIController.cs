using UnityEngine;

public class AudioUIController : MonoBehaviour
{
    public AudioController audioController;

    public GameObject btnPlay;
    public GameObject btnPause;

    private AudioSource[] sources;
    private bool isPaused = false;

    void Start()
    {
        sources = GetComponentsInChildren<AudioSource>(true);
    }

    public void Play()
    {
        if (isPaused)
        {
            foreach (var s in sources)
                s.UnPause();
        }
        else
        {
            audioController.PlayAll();
        }

        isPaused = false;

        btnPlay.SetActive(false);
        btnPause.SetActive(true);
    }

    public void Pause()
    {
        foreach (var s in sources)
            s.Pause();

        isPaused = true;

        btnPlay.SetActive(true);
        btnPause.SetActive(false);
    }

    public void Restart()
    {
        audioController.PlayAll();

        isPaused = false;

        btnPlay.SetActive(false);
        btnPause.SetActive(true);
    }

    // OMITIR narración
    public void SkipNarration()
    {
        audioController.StopAll();

        isPaused = false;

        btnPlay.SetActive(true);
        btnPause.SetActive(false);
    }
}
