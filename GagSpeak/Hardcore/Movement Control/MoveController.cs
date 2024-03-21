using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using MButtonHoldState = FFXIVClientStructs.FFXIV.Client.Game.Control.InputManager.MouseButtonHoldState;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace GagSpeak.Hardcore.Movement;
// by far the most brainfuckery i've ever dealt with in my life, if you dont understand any of this i dont blame you one bit.
public class MoveController : IDisposable
{
    // possibility of null reference is very common here, so we can ignore most cases where it is flagged.
    #pragma warning disable CS8602

    private readonly HardcoreManager _hcManager;
    private readonly IObjectTable _objectTable;
    public bool DisablingALLMovement { get; private set; } = false;
    public bool DisablingMouseMovement { get; private set; } = false;

    // controls the complete blockage of movement from the player (Blocks /follow movement)
    #pragma warning disable CS0649
    [Signature("F3 0F 10 05 ?? ?? ?? ?? 0F 2E C6 0F 8A", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
    private nint forceDisableMovementPtr;
    private unsafe ref int ForceDisableMovement => ref *(int*)(forceDisableMovementPtr + 4);
    #pragma warning restore CS0649

    // prevents LMB+RMB moving by processing it prior to the games update movement check.
    public unsafe delegate byte MoveOnMousePreventorDelegate(MoveControllerSubMemberForMine* thisx);
    [Signature("40 55 53 48 8d 6c 24 c8 48 81 ec 38 01 00 00", DetourName = nameof(MovementUpdate), Fallibility = Fallibility.Auto)]
    private static Hook<MoveOnMousePreventorDelegate>? MovementUpdateHook { get; set; } = null!;
    [return: MarshalAs(UnmanagedType.U1)]
    public unsafe byte MovementUpdate(MoveControllerSubMemberForMine* thisx) { // was static before.
        // get the current mouse button hole state, note that because we are doing this during the move update,
        // we are getting and updating the mouse state PRIOR to the game doing so, allowing us to change it
        MButtonHoldState* hold = InputManager.GetMouseButtonHoldState();
        MButtonHoldState original = *hold;
        // modify the hold state
        if (*hold == (MButtonHoldState.Left | MButtonHoldState.Right)) {
            *hold = 0;
        }
        // GSLogger.LogType.Debug($"{((IntPtr)hold).ToString("X")}");
        // update the original
        byte ret = MovementUpdateHook.Original(thisx);
        // restore the original
        *hold = original;
        // return 
        return ret;
    }

    public unsafe delegate void TestDelegate(UnkTargetFollowStruct* unk1, IntPtr unk2);
    [Signature("48 89 5c 24 08 48 89 74 24 10 57 48 83 ec 20 48 8b d9 48 8b fa 0f b6 89 59 05 00 00 be 00 00 00 e0", DetourName = nameof(TestUpdate), Fallibility = Fallibility.Auto)]
    private static Hook<TestDelegate>? UnfollowHook { get; set; }
    
    [return: MarshalAs(UnmanagedType.U1)]
    public unsafe void TestUpdate(UnkTargetFollowStruct* unk1, IntPtr unk2)
    {
        UnkTargetFollowStruct* temp = unk1;
        /*
        targetFollowVar = unk1;
        GSLogger.LogType.Debug($"PRE:       UnkTargetFollowStruct: {((IntPtr)unk1).ToString("X")}");
        GSLogger.LogType.Debug($"---------------------------------");
        GSLogger.LogType.Debug($"PRE: Unk_0x450.Unk_GameObjectID0: {unk1->Unk_0x450.Unk_GameObjectID0.ToString("X")};");
        try {
            GSLogger.LogType.Debug($"PRE      Struct target4 Unk_0x10: {unk1->Unk_0x450.Unk_0x10};");
            GSLogger.LogType.Debug($"PRE      Struct target4 Unk_0x54: {unk1->Unk_0x450.Unk_0x54};");
        } catch (Exception ex) {
            GSLogger.LogType.Error($"Error converting Unk_0x10 to string: {ex}");
        }
        GSLogger.LogType.Debug($"PRE:             FollowingTarget: {unk1->FollowingTarget.ToString("X")}");
        GSLogger.LogType.Debug($"PRE:                 Follow Type: {unk1->FollowType.ToString("X")}");
        */
        foreach (Dalamud.Game.ClientState.Objects.Types.GameObject obj in _objectTable)
        {
            if (obj.ObjectId == unk1->GameObjectIDToFollow)
            {
                GSLogger.LogType.Debug($"Game Object To Follow: {unk1->GameObjectIDToFollow.ToString("X")}: {obj.Name.TextValue}");
                break;
            }
        }
        // if this condition it true, it means that the function is attempting to call a cancelation 
        if(DisablingMouseMovement && unk1->Unk_0x450.Unk_0x54 == 256) {
            GSLogger.LogType.Verbose($"Early escaping to prevent canceling follow!");
            return; // do an early return to prevent processing
        } else {
            // output the original
            UnfollowHook.Original(unk1, unk2);
        }
        /*
        try {
            GSLogger.LogType.Debug($"POST       UnkTargetFollowStruct: {((IntPtr)unk1).ToString("X")}");
            GSLogger.LogType.Debug($"---------------------------------");
            GSLogger.LogType.Debug($"POST Unk_0x450.Unk_GameObjectID0: {unk1->Unk_0x450.Unk_GameObjectID0.ToString("X")};");
            GSLogger.LogType.Debug($"POST     Struct target4 Unk_0x54: {unk1->Unk_0x450.Unk_0x54};");
            GSLogger.LogType.Debug($"POST             FollowingTarget: {unk1->FollowingTarget.ToString("X")}");
            GSLogger.LogType.Debug($"POST                 Follow Type: {unk1->FollowType.ToString("X")}");
        } catch (Exception ex) {
            GSLogger.LogType.Error($"Error {ex}");
        }
        */
        foreach (Dalamud.Game.ClientState.Objects.Types.GameObject obj in _objectTable)
        {
            if (obj.ObjectId == unk1->GameObjectIDToFollow)
            {
                GSLogger.LogType.Debug($"POST ObjectIDtoFollow: {unk1->GameObjectIDToFollow.ToString("X")}: {obj.Name.TextValue}");
                break;
            }
        }
    }

    public unsafe MoveController(HardcoreManager hardcoreManager, IGameInteropProvider interopProvider, IObjectTable objectTable) {
        _hcManager = hardcoreManager;
        _objectTable = objectTable;
        // initialize from attributes
        interopProvider.InitializeFromAttributes(this);        
    }

    #pragma warning restore CS8602

    // Hook enablers
    public void EnableMovementHooks() {
        MovementUpdateHook?.Enable(); // for enabling the prevention of LMB+RMB movement
        UnfollowHook?.Enable(); // makes it so you cant unfollow the target
    }

    // Hook disablers
    public void DisableMovementHooks() {
        MovementUpdateHook?.Disable(); // for disabling the prevention of LMB+RMB movement
        UnfollowHook?.Disable(); // makes it so you can unfollow the target
    }

    // the disposer
    public void Dispose() {
        GSLogger.LogType.Debug($"Disposing of MoveController: {DisablingALLMovement}, {DisablingMouseMovement}");
        DisableMovementHooks(); // disable the hooks

        // dispose of the hooks
        MovementUpdateHook?.Dispose();
        MovementUpdateHook = null; // make sure they are disposed of
        UnfollowHook?.Dispose();
        UnfollowHook = null; // make sure they are disposed of
    }

    // controls the movement toggle of the player (Still allows for /follow)
    public unsafe void CompletelyEnableMovement() {
        // if we have RestrictedMovement actively, and want to enable movement, we should set it to false
        if (DisablingALLMovement) {
            GSLogger.LogType.Debug($"Enabling moving, {ForceDisableMovement}");
            // if our pointer is above 0, it means that we are frozen, so let us move again
            if (ForceDisableMovement > 0) {
                ForceDisableMovement--;
            }
            // let our code know we are no longer preventing movement
            DisablingALLMovement = false;
        }
        // if we are toggling the mouse
        if(DisablingMouseMovement) {
            GSLogger.LogType.Debug($"Enabling mouse");
            // let our code know we are no longer preventing mouse movement
            DisableMovementHooks();
            DisablingMouseMovement = false;
        }
    }

    public unsafe void CompletelyDisableMovement(bool togglePointer, bool toggleMouse) {
        // if we are currenelty not preventing mouse movement, and we want to disable movement, we should set it to true
        if (!DisablingALLMovement && togglePointer) {
            GSLogger.LogType.Debug($"Disabling moving, {ForceDisableMovement}");
            // if our pointer is 0, it means that we are not frozen, so let us freeze
            if (ForceDisableMovement == 0) {
                ForceDisableMovement++;
            }
            DisablingALLMovement = true;
        }
        // if we are currenelty not preventing mouse movement, and we want to disable mouse movement, we should set it to true
        if (!DisablingMouseMovement && toggleMouse) {
            EnableMovementHooks();
            GSLogger.LogType.Debug($"Disabling mouse");
            DisablingMouseMovement = true;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UnkGameObjectStruct {
        [FieldOffset(0xD0)] public int Unk_0xD0;
        [FieldOffset(0x101)] public byte Unk_0x101;
        [FieldOffset(0x1C0)] public Vector3 DesiredPosition;
        [FieldOffset(0x1D0)] public float NewRotation;
        [FieldOffset(0x1FC)] public byte Unk_0x1FC;
        [FieldOffset(0x1FF)] public byte Unk_0x1FF;
        [FieldOffset(0x200)] public byte Unk_0x200;
        [FieldOffset(0x2C6)] public byte Unk_0x2C6;
        [FieldOffset(0x3D0)] public GameObject* Actor; // Points to local player
        [FieldOffset(0x3E0)] public byte Unk_0x3E0;
        [FieldOffset(0x3EC)] public float Unk_0x3EC; // This, 0x3F0, 0x418, and 0x419 seem to determine the direction (and where) you turn when turning around or facing left/right
        [FieldOffset(0x3F0)] public float Unk_0x3F0;
        [FieldOffset(0x418)] public byte Unk_0x418;
        [FieldOffset(0x419)] public byte Unk_0x419;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MoveControllerSubMemberForMine {
        [FieldOffset(0x10)] public Vector3 Direction; // direction?
        [FieldOffset(0x20)] public UnkGameObjectStruct* ActorStruct;
        [FieldOffset(0x28)] public uint Unk_0x28;
        [FieldOffset(0x3C)] public byte Moved;
        [FieldOffset(0x3D)] public byte Rotated; // 1 when the character has rotated
        [FieldOffset(0x3E)] public byte MovementLock; // Pretty much forced auto run when nonzero. Maybe used for scene transitions?
        [FieldOffset(0x3F)] public byte Unk_0x3F;
        [FieldOffset(0x40)] public byte Unk_0x40;
        [FieldOffset(0x80)] public Vector3 ZoningPosition; // this gets set to your positon when you are in a scene/zone transition
        [FieldOffset(0xF4)] public byte Unk_0xF4;
        [FieldOffset(0x80)] public Vector3 Unk_0x80;
        [FieldOffset(0x90)] public float MoveDir; // Relative direction (in radians) that  you are trying to move. Backwards is -PI, Left is HPI, Right is -HPI
        [FieldOffset(0x94)] public byte Unk_0x94;
        [FieldOffset(0xA0)] public Vector3 MoveForward; // direction output by MovementUpdate
        [FieldOffset(0xB0)] public float Unk_0xB0;
        [FieldOffset(0x104)] public byte Unk_0x104; // If you were moving last frame, this value is 0, you moved th is frame, and you moved on only one axis, this can get set to 3
        [FieldOffset(0x110)] public Int32 WishdirChanged; // 1 when your movement direction has changed (0 when autorunning, for example). This is set to 2 if dont_rotate_with_camera is 0, and this is not 1
        [FieldOffset(0x114)] public float Wishdir_Horizontal; // Relative direction on the horizontal axis
        [FieldOffset(0x118)] public float Wishdir_Vertical; // Relative direction on the vertical (forward) axis
        [FieldOffset(0x120)] public byte Unk_0x120;
        [FieldOffset(0x121)] public byte Rotated1; // 1 when the character has rotated, with the exception of standard-mode turn rotation
        [FieldOffset(0x122)] public byte Unk_0x122;
        [FieldOffset(0x123)] public byte Unk_0x123;
    }



    // this and it's siblings have member functions in vtable
    [StructLayout(LayoutKind.Explicit, Size = 0x56)]
    public unsafe struct UnkTargetFollowStruct_Unk0x450
    {
        [FieldOffset(0x00)] public IntPtr vtable;
        [FieldOffset(0x10)] public float Unk_0x10;
        [FieldOffset(0x14)] public float Unk_0x14;
        [FieldOffset(0x18)] public float Unk_0x18;
        [FieldOffset(0x20)] public float Unk_0x20;
        [FieldOffset(0x28)] public float Unk_0x28;
        [FieldOffset(0x30)] public Vector3 PlayerPosition;
        [FieldOffset(0x40)] public uint Unk_GameObjectID0; // seemingly always E0000000 when relating to targets
        [FieldOffset(0x48)] public uint Unk_GameObjectID1; // seemingly always E0000000 when relating to targets
        [FieldOffset(0x50)] public int Unk_0x50;
        [FieldOffset(0x54)] public short Unk_0x54;
    }

    // possibly FFXIVClientStructs.FFXIV.Client.Game.Control.InputManager, ctor is ran after CameraManager and TargetSystem
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UnkTargetFollowStruct
    {
        [FieldOffset(0x10)] public IntPtr Unk_0x10;
        [FieldOffset(0x30)] public ulong Unk_0x30;
        [FieldOffset(0x4C)] public byte Unk_0x4C;
        [FieldOffset(0x4D)] public byte Unk_0x4D;
        [FieldOffset(0x50)] public byte Unk_0x50;
        [FieldOffset(0x150)] public ulong Unk_0x150;
        [FieldOffset(0x180)] public ulong Unk_0x180;
        [FieldOffset(0x188)] public ulong Unk_0x188;
        [FieldOffset(0x1B6)] public byte Unk_0x1B6;
        [FieldOffset(0x1C0)] public byte Unk_0x1C0; // used like a bitfield
        [FieldOffset(0x1EC)] public uint Unk_0x1EC;

        // think some of these floats are arrays of floats
        [FieldOffset(0x2A0)] public float Unk_0x2A0;
        [FieldOffset(0x2B0)] public float Unk_0x2B0;
        [FieldOffset(0x2C0)] public float Unk_0x2C0;
        [FieldOffset(0x2D0)] public float Unk_0x2D0;
        [FieldOffset(0x2E0)] public float Unk_0x2E0;
        [FieldOffset(0x2E4)] public float Unk_0x2E4;
        [FieldOffset(0x2F4)] public float Unk_0x2F4;
        [FieldOffset(0x304)] public float Unk_0x304;
        [FieldOffset(0x314)] public float Unk_0x314;
        [FieldOffset(0x324)] public float Unk_0x324;
        [FieldOffset(0x328)] public float Unk_0x328;
        [FieldOffset(0x338)] public float Unk_0x338;
        [FieldOffset(0x348)] public float Unk_0x348;
        [FieldOffset(0x358)] public float Unk_0x358;
        [FieldOffset(0x368)] public float Unk_0x368;

        [FieldOffset(0x3A0)] public IntPtr Unk_0x3A0;
        [FieldOffset(0x3F0)] public ulong Unk_0x3F0;
        [FieldOffset(0x410)] public uint Unk_0x410;
        [FieldOffset(0x414)] public uint Unk_0x414;
        [FieldOffset(0x418)] public uint Unk_0x418;
        [FieldOffset(0x420)] public uint Unk_0x420;
        [FieldOffset(0x424)] public uint Unk_0x424;
        [FieldOffset(0x428)] public uint Unk_0x428;
        [FieldOffset(0x430)] public uint GameObjectIDToFollow;
        [FieldOffset(0x438)] public uint Unk_0x438;

        // possible union below ...

        // start of some substruct (used for FollowType == 3?)
        [FieldOffset(0x440)] public byte Unk_0x440;
        [FieldOffset(0x448)] public byte Unk_0x448;
        [FieldOffset(0x449)] public byte Unk_0x449;
        // end of substruct

        // start of UnkTargetFollowStruct_Unk0x450 (used for FollowType == 4?)
        [FieldOffset(0x450)] public UnkTargetFollowStruct_Unk0x450 Unk_0x450;
        [FieldOffset(0x4A0)] public int Unk_0x4A0; // intersects UnkTargetFollowStruct_Unk0x450->0x50
        [FieldOffset(0x4A4)] public byte Unk_0x4A4; // intersects UnkTargetFollowStruct_Unk0x450->0x54
        [FieldOffset(0x4A5)] public byte FollowingTarget; // nonzero when following target (intersects UnkTargetFollowStruct_Unk0x450->0x54)
        // end of substruct

        [FieldOffset(0x4B0)] public ulong Unk_0x4B0; // start of some substruct (dunno where this one ends) (used for FollowType == 2?)
        [FieldOffset(0x4B8)] public uint Unk_GameObjectID1;
        [FieldOffset(0x4C0)] public byte Unk_0x4C0; // start of some substruct (dunno where this one ends) (used for FollowType == 1?)
        [FieldOffset(0x4C8)] public byte Unk_0x4C8;

        // possible union probably ends around here

        [FieldOffset(0x4D0)] public IntPtr Unk_0x4D0; // some sort of array (indexed by Unk_0x558?) unsure how large

        [FieldOffset(0x548)] public ulong Unk_0x548; // param_1->Unk_0x548 = (lpPerformanceCount->QuadPart * 1000) / lpFrequency->QuadPart;
        [FieldOffset(0x550)] public float Unk_0x550;
        [FieldOffset(0x554)] public int Unk_0x554; // seems to be some sort of counter or timer
        [FieldOffset(0x558)] public byte Unk_0x558; // used as an index (?)
        [FieldOffset(0x559)] public byte FollowType; // 2 faces the player away, 3 runs away, 4 runs towards, 0 is none
                                                    // unknown but known possible values: 1, 5
        [FieldOffset(0x55B)] public byte Unk_0x55B;
        [FieldOffset(0x55C)] public byte Unk_0x55C;
        [FieldOffset(0x55D)] public byte Unk_0x55D;
        [FieldOffset(0x55E)] public byte Unk_0x55E;
        [FieldOffset(0x55F)] public byte Unk_0x55F;
        [FieldOffset(0x560)] public byte Unk_0x560;
    }
}