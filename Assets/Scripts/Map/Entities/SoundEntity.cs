using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEntity : MonoBehaviour
{
    public RMeshData room;
    public int soundId;
    public float range;

    public void RefreshData()
    {
        AudioSource src = GetComponent<AudioSource>();
        src.dopplerLevel = 0f;
        src.loop = true;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 0.25f * range;
        src.maxDistance = range;
        src.clip = GameData.instance.roomAmbientAudio[soundId];
        src.Stop();
        src.Play();
    }
}