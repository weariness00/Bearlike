using UnityEngine;
using Weapon;

namespace Player
{
    public class PlayerSoundController : MonoBehaviour, IWeaponHitSound
    {
        [SerializeField] private AudioSource hit;
  
        [Header("Monster")] 
        [SerializeField] private AudioSource monsterHit;

        [Header("Item")]
        [SerializeField] private AudioSource earn;

        public void PlayItemEarn() => earn.Play();

        public void PlayWeaponHit()
        {
            monsterHit.playOnAwake = true;
            var sound = Instantiate(monsterHit.gameObject);
            monsterHit.playOnAwake = false;
        }
    }
}