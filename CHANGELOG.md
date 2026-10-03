# Change Log

## Current Version

v1.1.1

- Updated test dependencies: Touchstone.Core, Touchstone.Cli, Touchstone.XunitAdapter, Touchstone.NunitAdapter (0.1.12 -> 0.2.0), NUnit (4.3.2 -> 5.0.0), NUnit3TestAdapter (5.0.0 -> 6.3.0), NUnit.Analyzers (4.7.0 -> 4.15.0), xunit.runner.visualstudio (3.1.4 -> 4.0.0), Microsoft.NET.Test.Sdk (17.14.1 -> 18.10.1), coverlet.collector (6.0.4 -> 10.1.0)
- No changes to library behavior or public API

## Previous Versions

v1.1.0

- `finalChunk` is now `true` whenever the returned chunk ends at the end of the data, including when `chunkSize` equals the data length or the last window ends exactly on the boundary
- `Bytes.GetNextChunk` now returns `null` (with `position` of `-1` and `finalChunk` of `true`) after the final chunk, matching `Streams`
- `Streams` no longer pads a partial final chunk with zero bytes
- `Streams` now handles streams that return fewer bytes than requested per `Read` call (network, compression, and similar streams)
- `Streams` now throws `EndOfStreamException` when the stream ends before `contentLength` bytes are read, instead of looping forever
- `AdvanceToNewChunk` no longer drops trailing data in `Streams`, and `newData` and `BytesRemaining` are correct after advancing in `Bytes`
- Validation message for `shiftSize` now reads "must not exceed chunk size", matching the existing behavior that allows `shiftSize == chunkSize`
- Added test infrastructure built on Touchstone: `Test.Shared`, `Test.Automated`, `Test.Xunit`, and `Test.Nunit`

## Previous Versions

v1.0.8

- Retargeted to netstandard2.0, netstandard2.1, net8.0, and net10.0
- Symbol package generation

v1.0.7.1

- .NET 6 support

v1.0.7

- .NET 5 support

v1.0.6

- XML documentation

v1.0.4

- Added `newData` out param containing the new data included in the returned chunk

v1.0.3

- Added support for streams where the content length is known

v1.0.2

- Added `position` out param indicating where the chunk starts in the source

v1.0.1

- Added support for net452

v1.0.0

- Initial release with support for byte arrays
