# Overview

This mod's primary goal is to let _the other mods_ to have own filters in the game's editor. See more details in [Wiki](https://github.com/BobPalmer/CommunityCategoryKit/wiki).

# Building the mod

To build this mod locally, you need to map the root folder of `KSP` to the `Q:` drive (given you use _Windows_).

E.g. if the installation folder of the game was `D:\SteamLibrary\steamapps\common\Kerbal Space Program`, then the approprite mapping commands would be:

```
d:
cd "SteamLibrary\steamapps\common"
subst q: "Kerbal Space Program"
```

If drive `Q:` is already taken in your system, then map the folder to any other drive and modify `SOURCE\CCK\CCK\CCK.csproj` accordingly.

# Making a release

__NOTE__. Only the owner of the mod makes the releases. In a usual case the contributors only send PRs into the `DEVELOP` branch. Special permissions on `GitHub` are required to perform the releases activity.

In nutshell, the release process is the following:

1. Modify the mod's version in `SOURCE\CCK\CCK\Properties\AssemblyInfo.cs`.
2. Build a new binary.
3. Put the binary into the `FOR_RELEASE` folder at the appropriate location.
4. Update `FOR_RELEASE\GameData\CommunityCategoryKit\CCK.version` accordignly.
5. Make a ZIP archive from the `FOR_RELEASE` folder. And name it `CCK_<major.minor.patch>.zip` (e.g. `CCK_5.1.0.zip`). Make sure the `GameData` folder shows up at the _root_ in the archive.
6. Create a GitHub PR from the changes mention above. Have it sync'ed and merged into the `master`.
7. Create a release on GitHub, set the version tag (all four parts are required, e.g. `5.1.0.0`), and attach the release archive to it.

## Using `tools` to make a release

Steps 2-5 can be skipped if you use the build script from `Tools` folder. However, for the script to work, the following prerequisites need to be satisfied:

1. Python `2.7` must be installed into the system.
2. `7-Zip` must be installed into the system.
3. `SHELL_ZIP_BINARY` variable in the `Tools/make_release.py` script must be adjusted to the actual location of the `7-Zip` executable.
4. `Tools/make_binary.cmd` script must be adjusted to fit the current system settings (`MSBuild` location).

Once the above conditions are satisfied, simply run `Tools/make_release.py -p`, having the `Tools` folder current. This will result in compilation of the current state and packing the result `DLL` into an archive. If no packing is needed, omit the `-p` argument (`p` stands for `package`).

Note, that if there is a version of archive already made, then the `-p` option will _not_ update the existing archive. The existing archive must either be removed, or the `-po` arguments must be provided to the tool (`o` stands for `overwrite`).
