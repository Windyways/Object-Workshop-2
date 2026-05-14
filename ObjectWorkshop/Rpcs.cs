namespace ObjectWorkshop;

public enum Rpcs : uint
{
    // Roles
    RpcActivate,
    RpcBarricade,
    RpcSpin,
    RpcInnerSpider,
    RpcMark,
    RpcResetWinChance,
    RpcSharpen,
    RpcDuel,
    RpcDestination,
    RpcAbduct,
    RpcRadiate,
    RpcPrepare,
    RpcIgnite,
    RpcFire,
    RpcAim,
    RpcMoveCrosshair,
    RpcDestroyCrosshair,
    RpcSpawnBook,
    RpcNotifyBC,
    RpcDeclare,
    RpcBloom,

    // Oasis
    RpcSanctify,
    RpcStartSandstorm,
    RpcStopSandstorm,
    RpcSanctifyNotif,
    RpcSandstormMissNotif,

    // Settler
    RpcSpawnNest,

    // Ambiguator
    RpcDetermine,
    RpcEncapsulate,
    RpcPlayJumpAnim,

    // Reaper
    RpcReaperAttack,
    RpcCatastrophe,

    // Undead Reaper
    RpcFlipReaperVisual,

    // Gravekeeper
    RpcTombstone,
    RpcDigUp,

    // Totemist
    RpcInstall,

    // Culverin
    RpcLoadAndShoot,
    RpcMoveCannonball,

    // Mechanics
    RpcAddVote,

    // Other
    RpcAddDeathReason,
    RpcNotifyAll,
    RpcPerformInteraction,
    RpcSpawnVent,

    // Chat
    RpcSendCustomChat,








    // Misc
    RequestDeathStateValidation,
    SyncDeathState,
    GhostRoleMurder,
    CatchGhost,
    RemoveSpawns,
    SetMap,
    ChangeRole,
    UpdateDeathHandler,
    Disperse,
    ButtonBarry,
    PlayerExile,
    SetPos
}