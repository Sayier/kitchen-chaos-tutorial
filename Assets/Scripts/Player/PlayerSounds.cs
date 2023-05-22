using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    private float footstepTimer;
    private float footstepTimerMax = .1f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player.IsWalking())
        {
            //Measure time between footsteps if the player is walking then update Sound system when time for next step
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = footstepTimerMax;

                SoundManager.Instance.PlayFootstepsSound(player.transform.position);
            }
        }
    }
}
