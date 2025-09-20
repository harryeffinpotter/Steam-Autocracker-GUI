// Just open the fucking search in browser
private void SearchRinForGame(string gameName)
{
    var searchUrl = $"https://cs.rin.ru/forum/search.php?keywords={Uri.EscapeDataString(gameName)}&terms=all&author=&sc=1&sf=titleonly&sk=t&sd=d&sr=topics&st=0&ch=300&t=0&submit=Search";
    System.Diagnostics.Process.Start(searchUrl);
}

// That's it. User can look with their own eyes.