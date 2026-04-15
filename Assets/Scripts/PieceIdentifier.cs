public static class PieceIdentifier
{
    public const int SPOON = 0;
    public const int SPONGE = 1;
    public const int SPICE_MILL = 2;
    public const int KNIFE = 3;
    public const int LITTLE_CHEF = 4;

    public static string GetPieceName(int glyphId)
    {
        switch (glyphId)
        {
            case SPOON: return "spoon";
            case SPONGE: return "sponge";
            case SPICE_MILL: return "spice_mill";
            case KNIFE: return "knife";
            case LITTLE_CHEF: return "little_chef";
            default: return "unknown";
        }
    }

    public static bool IsUtensil(int glyphId)
    {
        return glyphId >= SPOON && glyphId <= KNIFE;
    }

    public static bool IsSelector(int glyphId)
    {
        return glyphId == LITTLE_CHEF;
    }
}
