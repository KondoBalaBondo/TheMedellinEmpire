using UnityEngine;

namespace BigRookGames.Weapons
{
    public class GunfireController : MonoBehaviour
    {
        // --- Audio ---
        public AudioClip GunShotClip;
        public AudioSource source;
        public Vector2 audioPitch = new Vector2(.9f, 1.1f);

        // --- Muzzle ---
        public GameObject muzzlePrefab;
        public GameObject muzzlePosition;

        // --- Config ---
        [Tooltip("Minimum time between shots (seconds).")]
        public float shotDelay = .5f;

        public bool rotate = true;
        public float rotationSpeed = .25f;

        // --- Options ---
        public GameObject scope;
        public bool scopeActive = true;
        private bool lastScopeState;

        // --- Projectile ---
        [Tooltip("The projectile gameobject to instantiate each time the weapon is fired.")]
        public GameObject projectilePrefab;

        [Tooltip("Sometimes a mesh will want to be disabled on fire. For example: when a rocket is fired, we instantiate a new rocket, and disable" +
                 " the visible rocket attached to the rocket launcher")]
        public GameObject projectileToDisableOnFire;

        // --- Timing ---
        [SerializeField] private float timeLastFired;

        private void Start()
        {
            if (source != null) source.clip = GunShotClip;
            timeLastFired = 0;
            lastScopeState = scopeActive;
        }

        private void Update()
        {
            // --- If rotate is set to true, rotate the weapon in scene ---
            if (rotate)
            {
                transform.localEulerAngles = new Vector3(
                    transform.localEulerAngles.x,
                    transform.localEulerAngles.y + rotationSpeed,
                    transform.localEulerAngles.z
                );
            }

            // --- Semi-auto fire: hold left mouse button to keep firing with shotDelay ---
            if (Input.GetMouseButton(0) && ((timeLastFired + shotDelay) <= Time.time))
            {
                FireWeapon();
            }

            // --- Toggle scope based on public variable value ---
            if (scope && lastScopeState != scopeActive)
            {
                lastScopeState = scopeActive;
                scope.SetActive(scopeActive);
            }
        }

        /// <summary>
        /// Creates an instance of the muzzle flash.
        /// Also creates an instance of the audioSource so that multiple shots are not overlapped on the same audio source.
        /// Insert projectile code in this function.
        /// </summary>
        public void FireWeapon()
        {
            // --- Keep track of when the weapon is being fired ---
            timeLastFired = Time.time;

            // --- Spawn muzzle flash ---
            if (muzzlePrefab != null && muzzlePosition != null)
            {
                var flash = Instantiate(muzzlePrefab, muzzlePosition.transform);
            }

            // --- Shoot Projectile Object ---
            if (projectilePrefab != null && muzzlePosition != null)
            {
                GameObject newProjectile = Instantiate(
                    projectilePrefab,
                    muzzlePosition.transform.position,
                    muzzlePosition.transform.rotation,
                    transform
                );
            }

            // --- Disable any gameobjects, if needed ---
            if (projectileToDisableOnFire != null)
            {
                projectileToDisableOnFire.SetActive(false);
                Invoke(nameof(ReEnableDisabledProjectile), 3f);
            }

            // --- Handle Audio ---
            if (source != null)
            {
                // If source is a child, just play it
                if (source.transform.IsChildOf(transform))
                {
                    source.Play();
                }
                else
                {
                    // Instantiate audio source so shots don't overlap on one source
                    AudioSource newAS = Instantiate(source);

                    if (newAS != null)
                    {
                        // Pitch variation
                        float pitch = Random.Range(audioPitch.x, audioPitch.y);
                        newAS.pitch = pitch;

                        // If using an audio mixer with a Pitch parameter named "Pitch", try to set it too
                        if (newAS.outputAudioMixerGroup != null && newAS.outputAudioMixerGroup.audioMixer != null)
                        {
                            newAS.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", pitch);
                        }

                        // Play the gunshot sound
                        newAS.PlayOneShot(GunShotClip);

                        // Cleanup (recommend pool in real project)
                        Destroy(newAS.gameObject, 4f);
                    }
                }
            }

            // --- Insert custom code here to shoot projectile or hitscan from weapon ---
        }

        private void ReEnableDisabledProjectile()
        {
            projectileToDisableOnFire.SetActive(true);
        }
    }
}
