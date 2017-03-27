# osu-api
osu!api - C# cross-platform library for using [osu!api](https://github.com/ppy/osu-api/wiki) with intagrated JsonTextReader class of  [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) project.

## Using
You can create an instance of Osu.Api class
```c# 
var osuApi = new Osu.Api("your api key");
```
or use static methods.

### GetBeatmapsAsync ([/api/get_beatmaps](https://github.com/ppy/osu-api/wiki#apiget_beatmaps))
```c#
var beatmaps = await osuApi.GetBeatmapsAsync();
```
or 
```c#
var beatmaps = await Osu.Api.GetBeatmapsAsync("your api key");
```

### GetUserAsync ([/api/get_user](https://github.com/ppy/osu-api/wiki#apiget_user))
Parameter "u" can be string (username)
```c#
var user = await osuApi.GetUserAsync("Taazar");
```
or long (user_id).
```c#
var user = await osuApi.GetUserAsync("6613346");
```

### GetScoresAsync ([/api/get_scores](https://github.com/ppy/osu-api/wiki#apiget_scores))
Parameter "b" - a beatmap_id (NOT beatmapset_id).
```c#
var scores = await osuApi.GetScoresAsync(736215);
```

### GetUserBestAsync ([/api/get_user_best](https://github.com/ppy/osu-api/wiki#apiget_user_best))
```c#
var userBest = await osuApi.GetUserBestAsync(124493);
```

### GetUserRecentAsync ([/api/get_user_recent](https://github.com/ppy/osu-api/wiki#apiget_user_recent))
```c#
var userRecent = await osuApi.GetUserRecentAsync(50265);
```

### GetMatchAsync ([/api/get_match](https://github.com/ppy/osu-api/wiki#apiget_match))
```c#
int mp;
var match = await osuApi.GetMatchAsync(mp);
```
where "mp" - id of the match.
