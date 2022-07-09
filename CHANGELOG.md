v1.2.0
------

### Added

* `LayoutMode.Smart`, which can produce better layouts in certain circumstances involving `Aligned` blocks with `LineBreakHint`s.
* `Gutenberg.Brackets`: tools for surrounding text with brackets, supporting a variety of indentation styles.
* `Gutenberg.Expression`: tools for displaying expressions with operators and precedence.

### Changed

* `IDocumentRenderer.WhiteSpace` will no longer be called for 0-length whitespace chunks.
* Lines will no longer be written with trailing whitespace.


v1.1.0
------

### Added

* `Box`, a class for tabular or tiling layouts
* `ConsoleDocumentRenderer`, a class which can interpret annotations to write coloured output to the console
* A family of virtual methods (`OnPushAnnotation` etc) on `StackMachineDocumentRenderer`

### Changed
* Some internal optimisations


v1.0.0
------

Initial release
