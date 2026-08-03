# VPKTools

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Build Status](https://img.shields.io/github/actions/workflow/status/Swiftly-Tracker/VPKTools/build.yml?branch=main)](https://github.com/Swiftly-Tracker/VPKTools/actions)
[![Release](https://img.shields.io/github/v/release/Swiftly-Tracker/VPKTools?include_prereleases)](https://github.com/Swiftly-Tracker/VPKTools/releases)
[![NuGet](https://img.shields.io/nuget/v/VPKTools.svg)](https://www.nuget.org/packages/VPKTools/)

Parse and inspect Valve's VPK archive format. Works as both a CLI tool and a .NET library.

Uses [ValvePak](https://www.nuget.org/packages/ValvePak) to read `.vpk` files. List, find, extract, and verify entries without needing any Valve tooling installed.

## Install

Grab an archive from the [latest release](https://github.com/Swiftly-Tracker/VPKTools/releases/latest):

| Archive                                                                                                                                   | Needs .NET installed? | Use when                  |
| ----------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ------------------------- |
| [`VPKTools-win-x64.zip`](https://github.com/Swiftly-Tracker/VPKTools/releases/latest/download/VPKTools-win-x64.zip)                       | No                    | Windows, just run it      |
| [`VPKTools-linux-x64.zip`](https://github.com/Swiftly-Tracker/VPKTools/releases/latest/download/VPKTools-linux-x64.zip)                   | No                    | Linux, just run it        |
| [`VPKTools-win-x64-portable.zip`](https://github.com/Swiftly-Tracker/VPKTools/releases/latest/download/VPKTools-win-x64-portable.zip)     | .NET 10 runtime       | Windows, smaller download |
| [`VPKTools-linux-x64-portable.zip`](https://github.com/Swiftly-Tracker/VPKTools/releases/latest/download/VPKTools-linux-x64-portable.zip) | .NET 10 runtime       | Linux, smaller download   |

Those links always resolve to the newest stable release. On Linux, `chmod +x VPKTools.App` after unzipping.

As a library:

```bash
dotnet add package VPKTools
```

## Layout

| Project          | Purpose                                                                             |
| ---------------- | ----------------------------------------------------------------------------------- |
| `VPKTools.Tier0` | Core framework: interface registry, ConVars, ConCommands, logging, terminal REPL.   |
| `VPKTools.Pak`   | VPK reading, backed by [ValvePak](https://www.nuget.org/packages/ValvePak). Public API in `src/Shared/`, implementation in `src/Core/`. |
| `VPKTools.App`   | CLI entry point. One-shot VPK action when you pass `-vpk` + a flag, or interactive terminal. |

## Interactive terminal

Run with no arguments to get a REPL:

```bash
./VPKTools.App
```

`help` lists commands, `help <name>` describes one. `cmds` dumps every command, `convars` dumps every ConVar. `quit` exits.

## Inspect VPK files

Read-only VPK inspection is built in — open a `.vpk`, list/find entries, extract, or verify hashes and signature, either as a one-shot CLI command or from the terminal.

One-shot:

```bash
./VPKTools.App -vpk pak01_dir.vpk -list
./VPKTools.App -vpk pak01_dir.vpk -list -filter .vmt -output listing.txt
./VPKTools.App -vpk pak01_dir.vpk -info
./VPKTools.App -vpk pak01_dir.vpk -verify
./VPKTools.App -vpk pak01_dir.vpk -find materials/foo/bar.vmt
./VPKTools.App -vpk pak01_dir.vpk -extract materials/foo/bar.vmt -dest bar.vmt
./VPKTools.App -vpk pak01_dir.vpk -extractall ./out -filter .vmt
```

`-list`/`-output`/`-extractall` accept `-filter <substring>` to narrow which entries are matched. `-vpk` with no action flag just opens the pak and drops you into the terminal instead of exiting, so `pak_*` commands below pick up where the CLI left off.

Entry listings (`-list`, `-output`, `pak_list`, `pak_output`, `pak_find`) print one line per entry:

```
path=materials/foo/bar.vmt crc=B5ECB9D0 size=51 B size_in_bytes=51
```

`crc` is the 8-digit uppercase-hex CRC32; `size` is human-readable (KiB/MiB/... binary units); `size_in_bytes` is the exact byte count.

From the terminal, the same functionality is available as commands:

```
pak_open pak01_dir.vpk
pak_info
pak_list [filter]
pak_find <path>
pak_extract <entryPath> <destPath>
pak_extractall <destDir> [filter]
pak_verify
pak_output <filePath> [filter]
pak_close
```

## Building from source

Requires the **.NET 10 SDK**.

```bash
git clone https://github.com/Swiftly-Tracker/VPKTools.git
cd VPKTools
dotnet build VPKTools.slnx -c Release
```

Output lands in `build/Release/<project>/`; the CLI is `build/Release/VPKTools.App/VPKTools.App`.

To produce a standalone binary like the release archives:

```bash
dotnet publish VPKTools.App/VPKTools.App.csproj -c Release \
  -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=false \
  -o out/linux-x64
```

> `PublishTrimmed` must stay `false`. The app resolves its modules by name through `Assembly.Load`, which the trimmer cannot see.

## Releases & branches

| Branch | Publishes      | Tag             |
| ------ | -------------- | --------------- |
| `main` | Stable release | `vX.Y.Z`        |
| `beta` | Prerelease     | `vX.Y.Z-beta.N` |

The flow:

1. Features and fixes land on **`beta`**. Every push builds and publishes a prerelease with all four archives attached.
2. When a batch is ready, open a PR from **`beta` → `main`**. Merging it publishes the stable release and pushes the NuGet package.
3. `beta` is then automatically force-reset onto `main`, so the next cycle starts clean. If you keep a local `beta`, run `git fetch && git reset --hard origin/beta` after each stable release.

## Architecture

```
VPKTools/
├── VPKTools.Tier0/        # Framework layer
│   └── src/
│       ├── Core/                    # ConVar system, logging, terminal, serialization
│       └── Shared/                  # Public interfaces (IInterfaceSystem, IConVar, ITerminal, ...)
├── VPKTools.Pak/          # VPK reading (ValvePak-backed)
│   └── src/
│       ├── Core/                    # CPakSystem, CPakCommands (pak_* terminal commands), CPakModule
│       └── Shared/                  # Public API (IPakSystem, PakInfo, PakEntryInfo, ...) + Formatting/
├── VPKTools.App/          # CLI entry point
│   └── src/Application.cs
├── VPKTools.csproj        # NuGet packaging front
├── Directory.Build.props            # Shared metadata + version
└── GitVersion.yml                   # Versioning rules
```

## Community

- **Issues**: [Report bugs and request features](https://github.com/Swiftly-Tracker/VPKTools/issues)
- **Security**: [Report privately](https://github.com/Swiftly-Tracker/VPKTools/security/advisories/new) — never in a public issue

## License

GPL-3.0. See [LICENSE](LICENSE). Third-party attributions in [THIRDPARTY.md](THIRDPARTY.md).

---

<div align="center">
  <strong>Made with ❤️ by the Swiftly Development team</strong>
</div>
