using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{

    private Animator playerAnimator;
    private Player player;
    private const string IsWalking = "IsWalking";

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        playerAnimator.SetBool(IsWalking, player.IsWalking());
    }


}
