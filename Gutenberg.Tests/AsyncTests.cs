namespace Gutenberg.Tests;

using System;
using System.Threading;

using Doc = Document<object>;

public class AsyncTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task TestRendererGoingAsync(int goAsyncAt)
    {
        var doc = Doc.Concat(Enumerable.Repeat(Doc.FromString("a"), 4));
        var renderer = new AsyncFakeDocumentRenderer(goAsyncAt);

        await doc.Render(renderer, TestContext.Current.CancellationToken);

        Assert.Equal(new string('a', 4), renderer.ToString());
    }

    private sealed class AsyncFakeDocumentRenderer(int goAsyncAt) : FakeDocumentRenderer<object>
    {
        private int _callCount = 0;

        public override async ValueTask Text(
            ReadOnlyMemory<char> mem,
            CancellationToken cancellationToken = default
        )
        {
            _callCount++;

            await base.Text(mem, cancellationToken);

            if (_callCount >= goAsyncAt)
            {
                await Task.Yield();
            }
        }
    }
}
