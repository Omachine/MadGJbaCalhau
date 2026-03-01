/// <summary>
/// Static data carrier — survives scene loads.
/// PingPongTable writes here before loading the ping pong scene.
/// BouncingBall2D reads here when the match ends.
/// </summary>
public static class PingPongReturnData
{
    public static string returnScene      = "GonScene";
    public static int    playedDifficulty = 1;
    public static bool   hasReturnPosition = false;
    public static float  returnPositionX   = 0f;
    public static float  returnPositionY   = 0f;
}

