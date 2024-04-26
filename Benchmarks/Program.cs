using BenchmarkDotNet.Running;
using EFCoreExtras.Benchmarks;

//var summary = BenchmarkRunner.Run<BulkCreateBenchmark>();
var summary = BenchmarkRunner.Run<BulkUpdateBenchmark>();
// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

