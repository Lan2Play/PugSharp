namespace PugSharp.G5Api;

public enum GameState
{
    None,                          // no setup has taken place
    PreVeto,                       // warmup, waiting for the veto
    Veto,                          // warmup, doing the veto
    Warmup,                        // setup done, waiting for players to ready up
    KnifeRound,                    // in the knife round
    WaitingForKnifeRoundDecision,  // waiting for a .stay/.swap command after the knife
    GoingLive,                     // counting down to live
    Live,                          // the match is live
    PendingRestore,                // pending restore to a live match
    PostGame,                      // postgame screen + waiting for GOTV to finish broadcast
};
