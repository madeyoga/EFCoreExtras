using BenchmarkDotNet.Running;
using EFCoreExtras.Benchmarks;

var summary = BenchmarkRunner.Run<BulkCreateBenchmark>();
// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

