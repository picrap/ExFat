# ExFat

An exFAT accessor library.

## Summary

**ExFat** allows to manipulate an exFAT formatted partition (provided as a `System.IO.Stream`).
It comes with two packages:
* The core package `ExFat.Core` available from [NuGet](https://www.nuget.org/packages/ExFat.Core), which allows simple exFAT management at three different levels (partition, entry and path).
* The DiscUtils package `ExFat.DiscUtils` available from [NuGet](https://www.nuget.org/packages/ExFat.DiscUtils), which depends on [`DiscUtils`](https://www.nuget.org/packages/DiscUtils) package.

Currently, `ExFat.Core` does what it says: files/directories manipulation at any level.
DiscUtils support is on its way and should be released in the very next few days.

`ExFat.Core` works at three levels:
1. Lowest level: partition access. This allows to manipulate clusters, allocation bitmap, directory entries and clusterr streams.
2. Middle level: entry access. Files/directories can be used to read/write content.
3. High level: path access. This works as you would expect using file paths.

`ExFat.DiscUtils` is also a high-level access (using paths) with implementation for [`DiscUtils`](https://github.com/DiscUtils/DiscUtils).

Because it is still under development, you can see pending features state [here](https://github.com/picrap/ExFat/labels/feature).

## Samples

All examples assume you have a `Stream` containing an exFAT partition.
```csharp
// Access at partition-level. Most efficient, most dangerous.
// Integrity is not guaranteed at this level, 
// user needs to make all neceassary operations in right order.
using(var partition = new ExFatPartition(partitionStream))
{
    // returns all entries (including bitmap, volume label, etc.) from root directory
    var entries = partition.GetEntries(partition.RootDirectoryDataDescriptor);

    // returns all files/directories meta entries
    var metaEntries = partition.GetMetaEntries(partition.RootDirectoryDataDescriptor);

    // assuming there is one, of course (but we're in a sample)
    var someDirectory = metaEntries.First(e => e.IsDirectory);
    var directoryMetaEntries = partition.GetMetaEntries(someDirectory.DataDescriptor);

    var someFile = metaEntries.First(e => !e.IsDirectory);
    using(var dataStream = partition.OpenDataStream(someFile.DataDescriptor, FileAccess.Read))
    { }
}
```
```csharp
// Access at entry level. Quite fast, since user has to track entries.
// Integrity is guaranteed. File attributes are not honored (maybe one day...)
using(var entryFilesystem = new ExFatEntryFilesystem(partitionStream))
{
    var someFileEntry = entryFilesystem.FindChild(filesystem.RootDirectory, "someFile");
    using(var fileStream = entryFilesystem.OpenFile(someFileEntry, FileAccess.Read)
    { }

    // finding one file in a directory requires two steps
    var someDirectoryEntry = entryFilesystem.FindChild(filesystem.RootDirectory, "someDirectory");
    var someChildFileEntry = entryFilesystem.FindChild(someDirectoryEntry, "someDirectory");
}
```
```csharp
// Access at path level. Uses a path cache to retrieve entries, 
// so speed is not as good (but not that bad either)
// since there is not drive, paths only specify the directory chain
// (so "a\b\c" for example)
using(var pathFilesystem = new ExFatPathFilesystem(partitionStream))
{
    var rootEntries = pathFilesystem.EnumerateEntries("\"); // "" works too for root
    var childEntries = pathFilesystem.EnumerateEntries(@"\somedir"); // "somedir" works too
    using(var s = pathFilesystem.Open(@"a\b\c", FileMode.Open, FileAccess.Read)
    { }
}
```

Current build status (for people who care... If you ever meet one): [![Build status](https://ci.appveyor.com/api/projects/status/k0jf58a0e5g2ue2h?svg=true
)](https://ci.appveyor.com/project/picrap/exfat)

