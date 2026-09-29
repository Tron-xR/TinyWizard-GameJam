using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceSounds
    {
        public string surfaceTag = "Untagged";
        public AudioClip[] footstepClips;
        public float volume = 1f;
        public float pitchMin = 0.9f;
        public float pitchMax = 1.1f;
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource footstepSource;

    [Header("Surface Config")]
    [SerializeField] private SurfaceSounds[] surfaceSounds;
    [SerializeField] private SurfaceSounds defaultSounds;

    [Header("Settings")]
    [SerializeField] private float groundCheckDistance = 1.5f;
    [SerializeField] private LayerMask groundLayer = -1;

    private Transform cachedTransform;

    private void OnEnable()
    {
        cachedTransform = transform;
        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();
    }

    public void OnFootstep()
    {
        if (footstepSource == null) return;

        AudioClip clip = GetSurfaceClip();
        if (clip == null) return;

        if (defaultSounds != null)
            footstepSource.pitch = Random.Range(defaultSounds.pitchMin, defaultSounds.pitchMax);

        footstepSource.PlayOneShot(clip);
    }

    private AudioClip GetSurfaceClip()
    {
        RaycastHit hit;
        if (Physics.Raycast(cachedTransform.position + Vector3.up * 0.1f, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            string tag = hit.collider.tag;

            if (surfaceSounds != null)
            {
                foreach (SurfaceSounds surface in surfaceSounds)
                {
                    if (surface != null && surface.surfaceTag == tag && surface.footstepClips != null && surface.footstepClips.Length > 0)
                        return surface.footstepClips[Random.Range(0, surface.footstepClips.Length)];
                }
            }
        }

        if (defaultSounds != null && defaultSounds.footstepClips != null && defaultSounds.footstepClips.Length > 0)
            return defaultSounds.footstepClips[Random.Range(0, defaultSounds.footstepClips.Length)];

        return null;
    }
}
