# PixivLib
PixivLib is a library for accessing [Pixiv](http://pixiv.net/) from C#.
PixivLib was developed by intercepting HTTPS communications from the Android
application and mimicking them.

Currently, the old CSV-based APIs are not supported, but a few JSON-based
APIs are supported, including the ability to search works and authenticate. The
`PHPSESSID` token needed to access old-style APIs is properly saved for when
such functionality is implemented.

## Usage
PixivLib requires a Pixiv account. An example usage follows:

```csharp
var client = new PixivClient();

await client.Authenticate("username", "password");

// List available emoji.
var emojis = await client.GetEmoji();
foreach (var emoji in emojis)
{
    Console.WriteLine("emoji {0}: {1}", emoji.Slug, emoji.ImageURLs.LargeURL);
}

// Query a given user ID.
var user = await client.GetUser(7964);
Console.WriteLine("User name: {0}", user.Name);

// Search the 'rakugaki' tag.
var works = await client.SearchWorks("らくがき");
foreach (Work w in works)
{
    Console.WriteLine("title: {0}", w.Title);
}
```
