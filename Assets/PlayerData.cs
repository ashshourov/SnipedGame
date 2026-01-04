using System.Collections.Generic;

public class PlayerData
{
    // Unique ID assigned by Firebase Auth (needed for sniping)
    public string userId;

    // Player's display name
    public string username;

    // Score tracking
    public int totalSnipes = 0;

    // Status tracking (for the Respawn Mechanic)
    public string status = "ACTIVE";

    // The Unix timestamp when the player can snipe again
    public long respawnEndTime = 0;

    // The ID of the team the player belongs to
    public string teamId;
}