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

int position = 0;         // zero-based index of current chunk
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

```chunkSize``` must be greater than zero and less than the length of the supplied data, and ```shiftSize``` must be greater than zero and less than ```chunkSize```.

## New in v1.0.x

- Initial release with support for byte arrays.
- Added support for net452.
- Added support for streams where the content length is known.
- Added out param for position at which the chunk starts from the source byte array or stream.
- Added out param for byte array containing the new data from the previous chunk included in the returned chunk.
 
## Version History

Changes made from previous versions will be listed here.

