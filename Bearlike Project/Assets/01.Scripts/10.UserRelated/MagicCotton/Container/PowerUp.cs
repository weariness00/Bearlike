using Player;

namespace User.MagicCotton.Container
{
    public class PowerUp : MagicCottonBase
    {
        public override void Apply(PlayerController playerController)
        {
            playerController.status.damageMultiple += Level.Current * 0.1f;
        }
    }
}

