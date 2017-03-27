namespace Osu
{

    public enum Team : int
    {
        Red = 2,
        Blue = 1
    }

    public enum TeamType : int
    {
        HeadToHead = 0,
        TagCoop = 1,
        TeamVs = 2,
        TagTeamVs = 3
    }

    public enum WinningCondition
    {
        Score = 0,
        Accuracy = 1,
        Combo = 2
    }

    public enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        //NoVideo      = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,  // Autopilot?
        Perfect = 16384,
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        keyMod = Key4 | Key5 | Key6 | Key7 | Key8,
        FadeIn = 1048576,
        Random = 2097152,
        LastMod = 4194304,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
        Key9 = 16777216,
        Key10 = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456
    }

    public enum Mode : int
    {
        Osu = 0,
        Taiko = 1,
        CtB = 2,
        OsuMania = 3
    }

    public enum RankStatus : int
    {
        Graveyard = -2,
        WIP = -1,
        Pending = 0,
        Ranked = 1,
        Approved = 2,
        Qualified = 3
    }

    public enum Genre : int
    {
        Any = 0,
        unspecified = 1,
        videoGame = 2,
        anime = 3,
        rock = 4,
        pop = 5,
        other = 6,
        novelty = 7,
        hipHop = 9,
        electronic = 10
    }

    public enum Language : int
    {
        Any = 0,
        Other = 1,
        English = 2,
        Japanese = 3,
        Chinese = 4,
        Instrumental = 5,
        Korean = 6,
        French = 7,
        German = 8,
        Swedish = 9,
        Spanish = 10,
        Italian = 11
    }
    
}
