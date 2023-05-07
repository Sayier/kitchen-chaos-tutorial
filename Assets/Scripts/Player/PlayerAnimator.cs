using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
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
        playerAnimator.SetBool(IsWalking, player.IsWalking());
    }


}
