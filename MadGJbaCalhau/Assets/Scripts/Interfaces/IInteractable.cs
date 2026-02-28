/// <summary>
/// Implement this interface on any object the player can interact with.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses the interact key while in range.
    /// </summary>
    void Interact();

    /// <summary>
    /// Optional: called when the player enters the interaction zone.
    /// Use this to show a prompt (e.g. "Press E to interact").
    /// </summary>
    void OnPlayerEnter();

    /// <summary>
    /// Optional: called when the player leaves the interaction zone.
    /// Use this to hide the prompt.
    /// </summary>
    void OnPlayerExit();
}

