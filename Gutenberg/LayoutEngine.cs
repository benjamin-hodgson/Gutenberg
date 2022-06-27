namespace Gutenberg;

internal class LayoutEngine<T>
{
    private readonly LayoutOptions _options;
    private readonly IDocumentRenderer<T> _renderer;

    private bool _flatten = false;
    private int _nestingLevel = 0;
    private int _wroteIndentation = 0;

    private readonly List<LayoutInstruction<T>> _lineBuffer = new();
    private int _lineTextLength = 0;

    private readonly List<IStackItem<T>> _stack = new();
    private bool _canBacktrack = false;  // are there any ChoicePoints on the stack?

    public LayoutEngine(LayoutOptions options, IDocumentRenderer<T> renderer)
    {
        _options = options;
        _renderer = renderer;
    }

    public async ValueTask Layout(Document<T> document, CancellationToken cancellationToken)
    {
        Push(document);

        while (Pop(out var item))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await ValueTask.FromCanceled(cancellationToken).ConfigureAwait(false);
                return;
            }

            switch (item)
            {
                case EmptyDocument<T>:
                    break;

                case LineDocument<T>:
                    if (_flatten)
                    {
                        // can't write a line break in flatten mode
                        Backtrack();
                    }
                    else
                    {
                        _lineBuffer.Add(NewLineInstruction<T>.Instance);
                        await Flush(cancellationToken).ConfigureAwait(false);
                    }
                    break;

                case WhiteSpaceDocument<T>(var amount):
                    _lineBuffer.Add(new WhiteSpaceInstruction<T>(amount));
                    _lineTextLength += amount;
                    if (!_canBacktrack)
                    {
                        await Flush(cancellationToken).ConfigureAwait(false);
                    }
                    else if (!Fits())
                    {
                        Backtrack();
                    }
                    break;

                case TextDocument<T>(var text):
                    _lineBuffer.Add(new TextInstruction<T>(text));
                    _lineTextLength += text.Length;
                    if (!_canBacktrack)
                    {
                        await Flush(cancellationToken).ConfigureAwait(false);
                    }
                    else if (!Fits())
                    {
                        Backtrack();
                    }
                    break;

                case BoxDocument<T>(var box):
                    if (box.Height > 1 && _flatten)
                    {
                        // can't write box in flatten mode
                        Backtrack();
                        break;
                    }
                    if (_canBacktrack && !WillFit(box.Width))
                    {
                        Backtrack();
                        break;
                    }

                    if (box.Width == 0 || box.Height == 0)
                    {
                        break;
                    }

                    // make sure box gets aligned
                    var delta1 = _wroteIndentation + _lineTextLength - _nestingLevel;
                    _nestingLevel += delta1;
                    Push(new Unnest<T>(delta1));

                    // need to flush the buffer, since box bypasses it
                    await Flush(cancellationToken).ConfigureAwait(false);

                    for (var i = 0; i < box.Height; i++)
                    {
                        await box.WriteLine(_renderer, i, cancellationToken).ConfigureAwait(false);
                        await WriteNewLine(cancellationToken).ConfigureAwait(false);
                    }

                    break;

                case AppendDocument<T>(var left, var right):
                    Push(right);
                    Push(left);
                    break;

                case AlternativeDocument<T>(var @default, var ifFlattened):
                    Push(_flatten ? ifFlattened : @default);
                    break;

                case ChoiceDocument<T>(FlattenedDocument<T> first, var second):  // Grouped()
                    Push(WillFit(first.FlattenedWidth) ? first : second);
                    break;

                case ChoiceDocument<T>(var first, var second):
                    Push(new ChoicePoint<T>(
                        second,
                        _nestingLevel,
                        _lineBuffer.Count,
                        _lineTextLength,
                        _flatten,
                        _canBacktrack,
                        GetResumeAt(_stack.Count - 1)
                    ));
                    _canBacktrack = true;
                    Push(first);
                    break;

                case NestedDocument<T>(var n, var nestedDoc):
                    var m = n ?? _options.DefaultNesting;
                    _nestingLevel += m;
                    Push(new Unnest<T>(m));
                    Push(nestedDoc);
                    break;

                case AnnotatedDocument<T>(var value, var child):
                    _lineBuffer.Add(new AnnotationInstruction<T>(value));
                    Push(PopAnnotation<T>.Instance);
                    Push(child);
                    break;

                case FlattenedDocument<T>(var flattenedDoc):
                    if (!_flatten)
                    {
                        _flatten = true;
                        Push(EndFlatten<T>.Instance);
                    }
                    Push(flattenedDoc);
                    break;

                case AlignedDocument<T>(var doc):
                    var delta = _wroteIndentation + _lineTextLength - _nestingLevel;
                    Push(new NestedDocument<T>(delta, doc));
                    break;

                case ChoicePoint<T>(_, _, _, _, _, _, var resumeAt) cp:
                    if (resumeAt < 0)
                    {
                        // we wrote the whole document
                        await Flush(cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    Push(cp with { ResumeAt = GetResumeAt(resumeAt - 1) });
                    Push(_stack[resumeAt]);
                    break;

                case Unnest<T>(var n):
                    _nestingLevel -= n;
                    break;

                case PopAnnotation<T>:
                    _lineBuffer.Add(PopAnnotationInstruction<T>.Instance);
                    break;

                case EndFlatten<T>:
                    _flatten = false;
                    break;
            }
        }

        await Flush(cancellationToken).ConfigureAwait(false);
    }

    private void Push(IStackItem<T> item) => _stack.Add(item);

    private bool Pop(out IStackItem<T>? item)
    {
        if (_stack!.Count == 0)
        {
            item = null;
            return false;
        }
        item = _stack[^1];
        _stack.RemoveAt(_stack.Count - 1);
        return true;
    }

    private bool Fits()
        => WillFit(0);

    private bool WillFit(int? additionalWidth)
        => _options.PageWidth == null || (
            additionalWidth != null
            && _wroteIndentation + _lineTextLength + additionalWidth <= _options.PageWidth.PageWidth
            && _lineTextLength + additionalWidth <= _options.PageWidth.PageWidth * _options.PageWidth.PrintableRatio
        );

    private int GetResumeAt(int candidate)
    {
        while (candidate >= 0 && _stack[candidate] is ChoicePoint<T> cp)
        {
            candidate = cp.ResumeAt;
        }
        return candidate;
    }

    private void Backtrack()
    {
        if (!_canBacktrack)
        {
            throw new InvalidOperationException("Couldn't backtrack! Please report this as a bug in Gutenberg");
        }
        while (Pop(out var doc))
        {
            // ignore resumeAt during failure - want to resume where the choice was
            if (doc is ChoicePoint<T>(var fb, var nest, var count, var textLength, var f, var cb, _))
            {
                Push(fb);
                _nestingLevel = nest;
                _lineBuffer.RemoveRange(count, _lineBuffer.Count - count);
                _lineTextLength = textLength;
                _flatten = f;
                _canBacktrack = cb;
                return;
            }
        }
    }

    private async ValueTask Flush(CancellationToken cancellationToken)
    {
        // commit to all choices since start of line
        for (var i = 0; i < _stack.Count; i++)
        {
            if (_stack[i] is ChoicePoint<T> cp)
            {
                // everything that's between a choice point and its ResumeAt
                // got written to the buffer
                for (var j = cp.ResumeAt + 1; j <= i; j++)
                {
                    _stack[j] = Document<T>.Empty;
                }
            }
        }
        _canBacktrack = false;

        foreach (var instr in _lineBuffer)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await ValueTask.FromCanceled(cancellationToken).ConfigureAwait(false);
                return;
            }
            switch (instr)
            {
                case TextInstruction<T>(var text):
                    await _renderer.Text(text, cancellationToken).ConfigureAwait(false);
                    break;
                case WhiteSpaceInstruction<T>(var amt):
                    await _renderer.WhiteSpace(amt, cancellationToken).ConfigureAwait(false);
                    break;
                case NewLineInstruction<T>:
                    await WriteNewLine(cancellationToken).ConfigureAwait(false);
                    break;
                case AnnotationInstruction<T>(var val):
                    await _renderer.PushAnnotation(val, cancellationToken).ConfigureAwait(false);
                    break;
                case PopAnnotationInstruction<T>:
                    await _renderer.PopAnnotation(cancellationToken).ConfigureAwait(false);
                    break;
            }
        }
        _lineBuffer.Clear();
    }

    private async ValueTask WriteNewLine(CancellationToken cancellationToken)
    {
        await _renderer.NewLine(cancellationToken).ConfigureAwait(false);
        await _renderer.WhiteSpace(_nestingLevel, cancellationToken).ConfigureAwait(false);
        _lineTextLength = 0;
        _wroteIndentation = _nestingLevel;
    }
}
