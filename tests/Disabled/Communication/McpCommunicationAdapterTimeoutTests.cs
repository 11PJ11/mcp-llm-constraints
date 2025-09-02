using System.Text;
using NUnit.Framework;
using ConstraintMcpServer.Infrastructure.Communication;

namespace ConstraintMcpServer.Tests.Infrastructure.Communication;

/// <summary>
/// Tests that reproduce the exact timeout bugs in McpCommunicationAdapter.
/// These tests target the specific ReadContentLengthHeader and ReadJsonContent methods that hang.
/// 
/// RED PHASE: These tests should TIMEOUT/HANG when the bug is present  
/// GREEN PHASE: These tests should PASS quickly when timeout fixes are implemented
/// </summary>
[TestFixture]
public class McpCommunicationAdapterTimeoutTests
{
    private McpCommunicationAdapter _adapter = null!;

    [SetUp]
    public void SetUp()
    {
        _adapter = new McpCommunicationAdapter();
    }

    [Test]
    [Timeout(3000)] // Should complete within 3 seconds, not hang indefinitely
    public async Task ReadRequestAsync_ShouldTimeout_WhenStreamContainsNoData()
    {
        // Arrange - Reproduce the exact scenario where ReadContentLengthHeader hangs
        // Empty stream causes ReadLineAsync to return null, which should be handled gracefully

        var emptyStream = new MemoryStream();
        var reader = new StreamReader(emptyStream);

        // Act - This should timeout quickly, not hang waiting for input
        var result = await _adapter.ReadRequestAsync(reader);

        // Assert - Should return null quickly when no data available
        Assert.That(result, Is.Null, "Should return null when no data available in stream");
    }

    [Test]
    [Timeout(3000)] // Should complete within 3 seconds
    public async Task ReadRequestAsync_ShouldTimeout_WhenStreamContainsMalformedHeader()
    {
        // Arrange - Reproduce scenario where malformed header causes parsing issues
        var malformedHeader = "This is not a Content-Length header\nSome other content\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedHeader));
        var reader = new StreamReader(stream);

        // Act - Should handle malformed header gracefully without hanging
        var result = await _adapter.ReadRequestAsync(reader);

        // Assert - Should return null for malformed header
        Assert.That(result, Is.Null, "Should return null when header is malformed");
    }

    [Test]
    [Timeout(3000)] // Should complete within 3 seconds
    public async Task ReadRequestAsync_ShouldTimeout_WhenContentLengthIsIncorrect()
    {
        // Arrange - Reproduce the exact hanging scenario in ReadJsonContent
        // Claims more content than available, causing ReadAsync to hang waiting for more data

        var incorrectLengthContent = "Content-Length: 1000\n\n{\"short\":\"content\"}"; // Claims 1000 bytes but has ~20
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(incorrectLengthContent));
        var reader = new StreamReader(stream);

        // Act - This reproduces the hanging bug in ReadJsonContent
        // ReadAsync hangs when trying to read more content than available
        var result = await _adapter.ReadRequestAsync(reader);

        // Assert - Should handle incorrect content length gracefully
        // Currently this will hang because ReadAsync waits for more data that never comes
        Assert.That(result, Is.Not.Null, "Should return partial content when Content-Length is incorrect");
        Assert.That(result, Does.Contain("short"), "Should return available content even if length is wrong");
    }

    [Test]
    [Timeout(3000)] // Should complete within 3 seconds  
    public async Task ReadRequestAsync_ShouldTimeout_WhenStreamClosesUnexpectedly()
    {
        // Arrange - Stream that closes after sending header but before content
        var headerOnly = "Content-Length: 50\n\n"; // Header claims content but stream ends
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(headerOnly));
        var reader = new StreamReader(stream);

        // Act - Should handle unexpected stream closure gracefully
        var result = await _adapter.ReadRequestAsync(reader);

        // Assert - Should return null or empty content, not hang
        // Currently hangs because ReadAsync waits for content that never arrives
        Assert.That(result, Is.Null.Or.Empty, "Should handle unexpected stream closure gracefully");
    }

    [Test]
    [Timeout(3000)] // Should complete within 3 seconds
    public async Task ReadRequestAsync_ShouldTimeout_WithValidButSlowStream()
    {
        // Arrange - Test with a stream that delivers data slowly (simulates network delay)
        var validContent = "Content-Length: 25\n\n{\"method\":\"test\",\"id\":1}";

        // Create a slow stream that delivers one byte at a time with delays
        var slowStream = new SlowDeliveryStream(Encoding.UTF8.GetBytes(validContent), delayPerByte: 10);
        var reader = new StreamReader(slowStream);

        // Act - Should still work but with reasonable timeout
        var result = await _adapter.ReadRequestAsync(reader);

        // Assert - Should receive complete content despite slow delivery
        Assert.That(result, Is.Not.Null, "Should handle slow streams gracefully");
        Assert.That(result, Does.Contain("test"), "Should receive complete content");
    }

    [Test]
    [Timeout(2000)] // Should complete within 2 seconds
    public void WriteResponseAsync_ShouldTimeout_WhenWritingToClosedStream()
    {
        // Arrange - Closed stream to simulate network disconnection
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Close(); // Close the writer to simulate connection loss

        var response = new { jsonrpc = "2.0", id = 1, result = "test" };

        // Act & Assert - Should handle closed stream gracefully without hanging
        var exception = Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _adapter.WriteResponseAsync(writer, response));

        Assert.That(exception, Is.Not.Null, "Should throw exception when writing to closed stream");
    }

    /// <summary>
    /// Helper class that simulates slow network delivery by introducing delays between bytes
    /// </summary>
    private class SlowDeliveryStream : Stream
    {
        private readonly byte[] _data;
        private int _position = 0;
        private readonly int _delayPerByte;

        public SlowDeliveryStream(byte[] data, int delayPerByte = 1)
        {
            _data = data;
            _delayPerByte = delayPerByte;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _data.Length;
        public override long Position { get => _position; set => _position = (int)value; }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_position >= _data.Length)
            {
                return 0;
            }

            // Simulate slow delivery with small delays
            if (_delayPerByte > 0)
            {
                await Task.Delay(_delayPerByte, cancellationToken);
            }

            // Return only one byte at a time to maximize the stress on the reading logic
            buffer[offset] = _data[_position];
            _position++;
            return 1;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
