
using BenchmarkDotNet.Running;
using EFCoreExtras.Benchmarks;

var summary = BenchmarkRunner.Run<BulkCreateBenchmark>();
