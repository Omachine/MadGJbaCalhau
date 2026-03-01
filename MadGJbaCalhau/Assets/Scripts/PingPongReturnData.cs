/// <summary>
/// Static data carrier — survives scene loads.
/// PingPongTable writes here before loading the ping pong scene.
/// BouncingBall2D reads here when the match ends.
/// </summary>
public static class PingPongReturnData
{
    public static string returnScene     = "GoncaloScene";
    public static int    playedDifficulty = 1;
}

