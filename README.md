# SlidingWindow

SlidingWindow provides an interface to retrieve chunks from a byte array using a sliding window.

## Use Cases

SlidingWindow is helpful for use cases that require examination of a sliding window of bytes within a byte array.  For instance, SlidingWindow is useful for cryptographic and compression use cases.

## How It Works

Initialize SlidingWindow with a byte array over which you would like to iterate using a SlidingWindow.

Specify the desired window size in ```chunkSize```.

Call ```GetNextChunk()``` to retrieve the next window of data.  As you retrieve chunks, the window will advance by the number of bytes defined in ```shiftSize```.

On the last chunk, if fewer than ```chunkSize``` bytes remain, the remaining bytes will be returned. ```out finalChunk``` is set to true on the chunk that reaches the end of the data. Any further calls to ```GetNextChunk()``` return ```null```, with ```position``` set to ```-1``` and ```finalChunk``` set to true.

```out newData``` contains the bytes in the returned chunk that were not part of any previously returned chunk. Concatenating ```newData``` from every chunk reproduces the original data.

Call ```AdvanceToNewChunk()``` to skip the overlap: the next chunk will start immediately after the end of the previous chunk, and all of its bytes will be returned in ```newData```.

## Example

Suppose you have the sentence ```The quick brown fox jumped over the lazy dog...```, which is 47 bytes long.
```
         1         2         3         4         5
12345678901234567890123456789012345678901234567890
The quick brown fox jumped over the lazy dog...
```

Initializing the ```Bytes``` class with this byte array, a ```chunkSize``` of ```24``` and a sliding window of ```8``` would result in the following:

```csharp
using SlidingWindow;

string strData = "The quick brown fox jumped over the lazy dog...";
byte[] byteData = Encoding.UTF8.GetBytes(strData);
Bytes slidingWindow = new Bytes(byteData, 24, 8);

int chunks = slidingWindow.ChunkCount();  // 4

long position = 0;        // zero-based index of current chunk
byte[] chunk = null;      // all of the data in the window
byte[] newData = null;    // new data retrieved from the previous chunk
bool finalChunk = false;  // whether or not it's the last chunk

chunk = slidingWindow.GetNextChunk(out position, out newData, out finalChunk);  
// position   : 0
// chunk      : 'The quick brown fox jump'
// newData    : 'The quick brown fox jump'
// finalChunk : false

chunk = slidingWindow.GetNextChunk(out position, out newData, out finalChunk);  
// position   : 8
// chunk      : 'k brown fox jumped over '
// newData    : 'ed over '
// finalChunk : false

chunk = slidingWindow.GetNextChunk(out position, out newData, out finalChunk);  
// position   : 16
// chunk      : 'fox jumped over the lazy'
// newData    : 'the lazy'
// finalChunk : false

chunk = slidingWindow.GetNextChunk(out position, out newData, out finalChunk);  
// position   : 24
// chunk      : 'ed over the lazy dog...'
// newData    : ' dog...'
// finalChunk : true
```

```chunkSize``` must be greater than zero and must not exceed the length of the supplied data, and ```shiftSize``` must be greater than zero and must not exceed ```chunkSize```.  Invalid values throw ```ArgumentException```.

## Streams

Use the ```Streams``` class to apply the same sliding window to a readable stream when the number of bytes to read (```contentLength```) is known.  Reading begins at the stream's current position, and only ```contentLength``` bytes are consumed.

```csharp
using (FileStream fs = new FileStream("data.bin", FileMode.Open, FileAccess.Read))
{
    Streams slidingWindow = new Streams(fs, fs.Length, 24, 8);

    while (true)
    {
        byte[] chunk = slidingWindow.GetNextChunk(out long position, out byte[] newData, out bool finalChunk);
        if (chunk == null) break;
        // process chunk
        if (finalChunk) break;
    }
}
```

If the stream ends before ```contentLength``` bytes have been read, ```GetNextChunk()``` throws ```EndOfStreamException```.

## Testing

Tests are written once in ```Test.Shared``` using [Touchstone](https://github.com/jchristn/touchstone) and can be run with any of three runners:

```bash
# Console runner (optionally export results to JSON)
dotnet run --project Test.Automated -f net10.0
dotnet run --project Test.Automated -f net10.0 -- --results results.json

# xUnit
dotnet test Test.Xunit

# NUnit
dotnet test Test.Nunit
```

```Test.Bytes``` and ```Test.Streams``` are interactive console applications for manually exploring the library.

## New in v1.1.1

- Test dependency updates: Touchstone 0.2.0, NUnit 5.0.0, NUnit3TestAdapter 6.3.0, NUnit.Analyzers 4.15.0, xunit.runner.visualstudio 4.0.0, Microsoft.NET.Test.Sdk 18.10.1, coverlet.collector 10.1.0
- No changes to library behavior or public API

## New in v1.1.0

- Correct ```finalChunk``` reporting when the last chunk ends exactly at the end of the data
- ```Bytes.GetNextChunk()``` returns ```null``` after the final chunk
- ```Streams``` no longer zero-pads a partial final chunk, handles partial stream reads, and throws ```EndOfStreamException``` on truncated streams
- ```AdvanceToNewChunk()``` fixes for ```newData```, ```BytesRemaining```, and dropped trailing data
- Test infrastructure using Touchstone (console, xUnit, and NUnit runners)
 
## Version History

Please refer to CHANGELOG.md.

