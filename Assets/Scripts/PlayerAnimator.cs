using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";

    [SerializeField] Player player;

    private Animator _animator;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        _animator.SetBool(IS_WALKING, player.IsWalking);
    }
}
