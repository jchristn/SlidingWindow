# SlidingWindow

SlidingWindow provides an interface to retrieve chunks from a byte array using a sliding window.

## Use Cases

SlidingWindow is helpful for use cases that require examination of a sliding window of bytes within a byte array.  For instance, SlidingWindow is useful for cryptographic and compression use cases.

## How It Works

Initialize SlidingWindow with a byte array over which you would like to iterate using a SlidingWindow.

Specify the desired window size in ```chunkSize```.

Call ```GetNextChunk()``` to retrieve the next window of data.  As you retrieve chunks, the window will advance by the number of bytes defined in ```shiftSize```.

On the last chunk, if fewer than ```chunkSize``` bytes remain, the remaining bytes will be returned, and ```out finalChunk``` will be set to true, indicating the end of the byte array.

## Example

Suppose you have the sentence ```The quick brown fox jumped over the lazy dog...```, which is 47 bytes long.
```
         1         2         3         4         5
12345678901234567890123456789012345678901234567890
The quick brown fox jumped over the lazy dog...
```

Initializing the ```Bytes``` class with this byte array, a ```chunkSize``` of ```24``` and a sliding window of ```8``` would result in the following:

```
using SlidingWindow;

string strData = "The quick brown fox jumped over the lazy dog..."
byte[] byteData = Encoding.UTF8.GetBytes(strData);
Bytes slidingWindow = new Bytes(byteData, 24, 8);

int chunks = slidingWindow.ChunkCount();  // 4
byte[] chunk = null;
int position = 0;

bool finalChunk = false;
chunk = slidingWindow.GetNextChunk(out position, out finalChunk);  // 'The quick brown fox jump'
chunk = slidingWindow.GetNextChunk(out position, out finalChunk);  // 'k brown fox jumped over '
chunk = slidingWindow.GetNextChunk(out position, out finalChunk);  // 'fox jumped over the lazy'
chunk = slidingWindow.GetNextChunk(out position, out finalChunk);  // 'ed over the lazy dog...'
// finalChunk would be true here
```

```chunkSize``` must be greater than zero and less than the length of the supplied data, and ```shiftSize``` must be greater than zero and less than ```chunkSize```.

## New in v1.0.x

- Initial release with support for byte arrays.
- Added support for net452.
- Added support for streams where the content length is known.
- Added out param for position at which the chunk starts from the source byte array or stream.
 
## Version History

Changes made from previous versions will be listed here.

