using UnityEngine;

namespace Player
{
    public class PlayerSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource hit;
        
        [Header("Item")]
        [SerializeField] private AudioSource earn;

        public void PlayItemEarn() => earn.Play();
    }
}