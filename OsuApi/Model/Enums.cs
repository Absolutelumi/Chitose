namespace OsuApi.Model
{
    public enum Genre
    {
        Any = 0,
        Unspecified = 1,
        VideoGame = 2,
        Anime = 3,
        Rock = 4,
        Pop = 5,
        Other = 6,
        Novelty = 7,
        HipHop = 9,
        Electronic = 10
    }

    public enum Language
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

    public enum Mode
    {
        Osu = 0,
        Taiko = 1,
        CatchTheBeat = 2,
        Mania = 3
    }

    public enum Status
    {
        Graveyard = -2,
        WorkInProgress = -1,
        Pending = 0,
        Ranked = 1,
        Approved = 2,
        Qualified = 3,
        Loved = 4
    }

    public enum UserCredentialType
    {
        Auto,
        Username,
        UserId
    }
}