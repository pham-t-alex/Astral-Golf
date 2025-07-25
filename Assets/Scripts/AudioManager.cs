using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject sourcePoolParent; 
    
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    [SerializeField] private List<AudioClip> sounds;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float spread;
    private int index = 0;

    private static AudioManager instance;
    public static AudioManager Instance => instance;

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Destroy(this);
            return;
        }
        if (instance == null) instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < sourcePoolParent.transform.childCount; i++)
        {
            audioSourcePool.Add(sourcePoolParent.transform.GetChild(i).GetComponent<AudioSource>());
        }
        foreach (AudioSource audioSource in audioSourcePool)
        {
            audioSource.maxDistance = maxDistance;
            audioSource.spread = spread;
        }
    }

    public void PlaySound(int idStart, int idEnd, Vector2 position)
    {
        AudioSource source = audioSourcePool[index];
        index = (index + 1) % audioSourcePool.Count;

        source.transform.position = position;
        source.clip = sounds[Random.Range(idStart, idEnd + 1)];

        source.Play();
    }
}
