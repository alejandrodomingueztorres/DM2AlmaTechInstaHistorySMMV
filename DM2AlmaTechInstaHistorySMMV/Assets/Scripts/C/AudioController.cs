using UnityEngine;
using UnityEngine.Timeline;

public class AudioController : MonoBehaviour
{
    [Header("Tracks configurados en escena")]
    public AudioTrack[] tracks;

    //Reproducir todos sincronizados
    public void PlayAll()
    {
        double startTime = UnityEngine.AudioSettings.dspTime;

        foreach (var track in tracks)
        {
            if (track.source == null) continue;

            track.source.Stop();
            track.source.PlayScheduled(startTime + track.delay);
        }
    }

    //Detener todos
    public void StopAll()
    {
        foreach (var track in tracks)
        {
            if (track.source == null) continue;

            track.source.Stop();
        }
    }

    //Reproducir uno solo
    public void PlayTrack(int index)
    {
        if (index < 0 || index >= tracks.Length) return;

        var track = tracks[index];

        track.source.Stop();
        track.source.PlayDelayed(track.delay);
    }
}

[System.Serializable]
public class AudioTrack
{
    public AudioSource source;
    public float delay;
}
