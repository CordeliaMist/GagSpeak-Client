using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;

namespace GagSpeak.Hardcore.Movement;
public unsafe class MoveController : IDisposable
{
    private readonly HardcoreManager _hcManager;
    // If true, we should enable the forcedisablemovement pointer. If false, we should only disable the virutual keys
    public bool PreventingMouseMovement { get; private set; } = false;

    // partial restriction virtual keys are stored within hardcore manager.

    // controls the complete blockage of movement from the player (Still allows for /follow)
    [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C6 0F 8A", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
    private nint forceDisableMovementPtr;
    private ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);

    public MoveController(HardcoreManager hardcoreManager, IGameInteropProvider interopProvider) {
        _hcManager = hardcoreManager;

        // initialize from attributes
        interopProvider.InitializeFromAttributes(this);
    }

    public void Dispose() {
        // reactivate movement on plugin disposal
        EnableMouseMoving();
    }

    // controls the movement toggle of the player (Still allows for /follow)
    public unsafe void EnableMouseMoving() {
        // if we have RestrictedMovement actively, and want to enable movement, we should set it to false
        if (PreventingMouseMovement) {
            GagSpeak.Log.Debug($"Enabling moving, {ForceDisableMovement}");
            // if our pointer is above 0, it means that we are frozen, so let us move again
            if (ForceDisableMovement > 0) {
                ForceDisableMovement--;
            }
            // let our code know we are no longer preventing movement
            PreventingMouseMovement = false;
        }
    }

    public void DisableMouseMoving() {
        // if we are currenelty not preventing mouse movement, and we want to disable movement, we should set it to true
        if (!PreventingMouseMovement) {
            GagSpeak.Log.Debug($"Disabling moving, {ForceDisableMovement}");
            // if our pointer is 0, it means that we are not frozen, so let us freeze
            if (ForceDisableMovement == 0) {
                ForceDisableMovement++;
            }
            // let our code know we are preventing movement
            PreventingMouseMovement = true;
        }
    }
}

