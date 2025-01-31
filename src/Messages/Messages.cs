namespace Wallsh.Messages;

public record IntervalUpdatedMessage(TimeSpan Interval);

public record StopRequestedMessage;

public record WallpaperFolderUpdatedMessage(string WallpapersFolder);

public record WallpaperUpdatedMessage;
