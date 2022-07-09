namespace Gutenberg;

internal class LayoutEngine<T>
{
    private readonly LayoutOptions _options;
    private readonly IDocumentRenderer<T> _renderer;
    private Stack<ChoicePoint<T>>? _choicePointPool;

    private bool _flatten = false;
    private int _nestingLevel = 0;
    private int _wroteIndentation = 0;

    private readonly List<LayoutInstruction<T>> _lineBuffer = new();
    private int _lineTextLength = 0;

    private readonly List<IStackItem<T>> _stack = new();
    private bool _canBacktrack = false;  // are there any ChoicePoints on the stack?

    private int _bufferUntilDeIndent = -1;

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
                        _lineBuffer.Add(LayoutInstruction<T>.NewLine);
                        if (_bufferUntilDeIndent < 0)
                        {
                            await Flush(cancellationToken).ConfigureAwait(false);
                        }
                        _lineTextLength = 0;

                        if (_nestingLevel > 0)
                        {
                            _lineBuffer.Add(LayoutInstruction<T>.WhiteSpace(_nestingLevel));
                        }
                        _wroteIndentation = _nestingLevel;
                    }
                    break;

                case WhiteSpaceDocument<T>(var amount):
                    _lineBuffer.Add(LayoutInstruction<T>.WhiteSpace(amount));
                    _lineTextLength += amount;
                    if (_canBacktrack && !Fits())
                    {
                        Backtrack();
                    }
                    break;

                case TextDocument<T>(var text):
                    _lineBuffer.Add(LayoutInstruction<T>.Text(text));
                    _lineTextLength += text.Length;
                    if (_canBacktrack && !Fits())
                    {
                        Backtrack();
                    }
                    break;

                // fixme: kind of ugly. Shares some logic with Aligned,
                // and also bypasses the buffer
                case BoxDocument<T>(var box):
                    var backtrack =
                        (box.Height > 1 && _flatten)  // can't write box in flatten mode
                        || (_canBacktrack && !WillFit(box.Width));

                    if (backtrack)
                    {
                        Backtrack();
                        break;
                    }

                    if (box.Width == 0 || box.Height == 0)
                    {
                        // empty
                        break;
                    }

                    // make sure box gets aligned
                    Push(SetNestingLevel<T>.Create(_nestingLevel));
                    _nestingLevel = _wroteIndentation + _lineTextLength;

                    // need to flush the buffer, since box bypasses it
                    await Flush(cancellationToken, false).ConfigureAwait(false);

                    for (var i = 0; i < box.Height; i++)
                    {
                        await box.WriteLine(_renderer, i, cancellationToken).ConfigureAwait(false);
                        await _renderer.NewLine(cancellationToken).ConfigureAwait(false);
                        if (_nestingLevel > 0)
                        {
                            await _renderer.WhiteSpace(_nestingLevel, cancellationToken).ConfigureAwait(false);
                        }
                        _lineTextLength = 0;
                        _wroteIndentation = _nestingLevel;
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
                    Push(CreateChoicePoint(
                        second,
                        _nestingLevel,
                        _lineBuffer.Count,
                        _lineTextLength,
                        _flatten,
                        _canBacktrack,
                        _bufferUntilDeIndent,
                        GetResumeAt(_stack.Count - 1)
                    ));
                    _canBacktrack = true;
                    Push(first);
                    break;

                case NestedDocument<T>(var n, var nestedDoc):
                    Push(SetNestingLevel<T>.Create(_nestingLevel));
                    _nestingLevel += n ?? _options.DefaultNesting;
                    Push(nestedDoc);
                    break;

                case AnnotatedDocument<T>(var value, var child):
                    _lineBuffer.Add(LayoutInstruction<T>.PushAnnotation(value));
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

                // Is it sufficient to _only_ set _bufferUntilDeIndent when
                // handling an AlignedDocument? Or are there other cases
                case AlignedDocument<T>(var doc):
                    var currentColumn = _wroteIndentation + _lineTextLength;
                    if (_options.LayoutMode == LayoutMode.Smart && _canBacktrack && currentColumn > 0 && _bufferUntilDeIndent < 0)
                    {
                        _bufferUntilDeIndent = currentColumn;
                    }
                    // read from bottom to top, because most recent push is popped first:
                    // 1. set the nesting level to the current location
                    // 2. write the doc
                    // 3. return the nesting level to what it was
                    Push(SetNestingLevel<T>.Create(_nestingLevel));
                    Push(doc);
                    Push(SetNestingLevel<T>.Create(currentColumn));
                    break;

                case ChoicePoint<T> cp:
                    if (cp.ResumeAt < 0)
                    {
                        goto wroteWholeDoc;
                    }
                    var resume = _stack[cp.ResumeAt];
                    cp.ResumeAt -= 1;
                    Push(cp);
                    Push(resume);
                    break;

                case SetNestingLevel<T>(var nestingLevel):
                    if (nestingLevel < _bufferUntilDeIndent)
                    {
                        _bufferUntilDeIndent = -1;
                    }
                    _nestingLevel = nestingLevel;
                    break;

                case PopAnnotation<T>:
                    _lineBuffer.Add(LayoutInstruction<T>.PopAnnotation);
                    break;

                case EndFlatten<T>:
                    _flatten = false;
                    break;
            }
        }

        wroteWholeDoc: await Flush(cancellationToken, returnChoicePoints: false).ConfigureAwait(false);
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
            if (doc is ChoicePoint<T> cp)
            {
                Push(cp.Fallback!);
                _nestingLevel = cp.NestingLevel;
                _lineBuffer.RemoveRange(cp.BufferedInstructionCount, _lineBuffer.Count - cp.BufferedInstructionCount);
                _lineTextLength = cp.LineTextLength;
                _flatten = cp.Flatten;
                _canBacktrack = cp.CanBacktrack;
                _bufferUntilDeIndent = cp.BufferUntilDeIndent;
                return;
            }
        }
        throw new InvalidOperationException("Didn't find a choice point! Please report this as a bug in Gutenberg");
    }

    private async ValueTask Flush(
        CancellationToken cancellationToken,
        bool stripTrailingWhitespace = true,
        bool returnChoicePoints = true
    )
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
                if (returnChoicePoints)
                {
                    ReturnChoicePoint(cp);
                }
            }
        }
        _canBacktrack = false;

        for (var i = 0; i < _lineBuffer.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await ValueTask.FromCanceled(cancellationToken).ConfigureAwait(false);
                return;
            }
            switch (_lineBuffer[i].GetInstructionType())
            {
                case LayoutInstructionType.Text:
                    await _renderer.Text(_lineBuffer[i].GetText(), cancellationToken).ConfigureAwait(false);
                    break;
                case LayoutInstructionType.WhiteSpace:
                    if (!stripTrailingWhitespace || LineContainsTextAfter(i))  // look ahead to determine whether we should strip the whitespace
                    {
                        await _renderer.WhiteSpace(_lineBuffer[i].GetWhitespaceAmount(), cancellationToken).ConfigureAwait(false);
                    }
                    break;
                case LayoutInstructionType.NewLine:
                    await _renderer.NewLine(cancellationToken).ConfigureAwait(false);
                    break;
                case LayoutInstructionType.PushAnnotation:
                    await _renderer.PushAnnotation(_lineBuffer[i].GetAnnotation(), cancellationToken).ConfigureAwait(false);
                    break;
                case LayoutInstructionType.PopAnnotation:
                    await _renderer.PopAnnotation(cancellationToken).ConfigureAwait(false);
                    break;
            }
        }
        _lineBuffer.Clear();
    }

    private bool LineContainsTextAfter(int index)
    {
        for (var i = index + 1; i < _lineBuffer.Count; i++)
        {
            if (_lineBuffer[i].IsText)
            {
                return true;
            }
            if (_lineBuffer[i].IsNewLine)
            {
                return false;
            }
        }
        return false;
    }

    private void ReturnChoicePoint(ChoicePoint<T> cp)
    {
        cp.Fallback = null;
        if (_choicePointPool == null)
        {
            _choicePointPool = new();
        }
        _choicePointPool.Push(cp);
    }

    private ChoicePoint<T> CreateChoicePoint(
        Document<T> fallback,
        int nestingLevel,
        int bufferedInstructionCount,
        int lineTextLength,
        bool flatten,
        bool canBacktrack,
        int bufferUntilDeIndent,
        int resumeAt
    )
    {
        var cp = _choicePointPool?.Count > 0
            ? _choicePointPool.Pop()
            : new ChoicePoint<T>();

        cp.Fallback = fallback;
        cp.NestingLevel = nestingLevel;
        cp.BufferedInstructionCount = bufferedInstructionCount;
        cp.LineTextLength = lineTextLength;
        cp.Flatten = flatten;
        cp.CanBacktrack = canBacktrack;
        cp.BufferUntilDeIndent = bufferUntilDeIndent;
        cp.ResumeAt = resumeAt;

        return cp;
    }
}
