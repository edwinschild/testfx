# Performance rules

Rules that support high-performance testing.

Identifier | Name | Description
-----------|------|------------
[MSTEST0001](MSTEST0001.md) | UseParallelizeAttributeAnalyzer | By default, MSTest runs tests sequentially which can lead to severe performance limitations. It is recommended to enable assembly attribute `[Parallelize]` or if the assembly is known to not be parallelizable, to use explicitly the assembly level attribute `[DoNotParallelize]`.