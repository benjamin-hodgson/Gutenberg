v1.3.2
------

### Fixed
* Fixed some bugs in the handling of multiple `LineBreakHint`s on a single line


v1.3.1
------

### Added

* Support for SourceLink
* Support for trimming and AOT deployments

### Changed

* Some optimisations to the layout engine

v1.3.0
------

### Added

* Methods to write a document into a `TextWriter`.


### Fixed

* A bug when calling `ToString` on a document annotated with a value type


v1.2.1
------

### Added

* An option to disable the default "strip trailing whitespace" behaviour


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
