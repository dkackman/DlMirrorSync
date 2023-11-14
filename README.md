# DlMirrorSync

[![.NET](https://github.com/dkackman/DlMirrorSync/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dkackman/DlMirrorSync/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/dkackman/DlMirrorSync/actions/workflows/codeql.yml/badge.svg)](https://github.com/dkackman/DlMirrorSync/actions/workflows/codeql.yml)

This is a utility service that will synchronize the list of chia data layer singletons from [datalayer.storage](https://api.datalayer.storage/mirrors/v1/list_all) to the local chia node. By running this tool, the local node will subscribe to and mirror all of the datalayer.storage singletons. This includes a transaction fee for each and devoting 0.0003 XCH per mirror, so be sure you are ready to do this.

Can either be run from code, from built binaries in [the latest release(https://github.com/dkackman/DlMirrorSync/releases/tag/v0.1.1), or as a windows service.

- The `singlefile` versions require [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).
- The `standalone` versions have .net embedded.
- The MSI installs the windows serivce that will synchronize the singletons once per day. (this installs as autostart so will run immediately)
