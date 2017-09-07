# ExFat

An exFAT accessor library.

Current build status: [![Build status](https://ci.appveyor.com/api/projects/status/k0jf58a0e5g2ue2h?svg=true
)](https://ci.appveyor.com/project/picrap/exfat).

## Summary

**ExFat** allows to manipulate an exFAT formatted partition (provided as a `System.IO.Stream`).

Current features:
- [X] Reading directories
- [X] Reading files
- [X] Writing streams
- [X] Writing files
- [X] Creating directories
- [X] Creating files
- [X] Deleting files/directories
- [X] Changing file length (`Stream.SetLength`)
- [ ] Using paths instead of objets (less efficient but more convenient)
- [ ] Handle second allocation bitmap
- [ ] Check for thread safety
- [ ] Partition formatting
- [ ] Working with [DiscUtils](https://github.com/DiscUtils/DiscUtils)
- [ ] Standalone Nuget package
- [ ] [DiscUtils](https://www.nuget.org/packages/DiscUtils) related package

## Samples

```csharp
// TODO, you lazy boy
```
