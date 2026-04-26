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

    // Mechanics
    RpcAddVote,

    // Other
    RpcAddDeathReason,
    RpcNotifyAll,
    RpcPerformInteraction,

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