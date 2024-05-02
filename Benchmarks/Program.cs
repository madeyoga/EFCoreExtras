using BenchmarkDotNet.Running;
using EFCoreExtras.Benchmarks;

//var summary = BenchmarkRunner.Run<BulkCreateBenchmark>();
//var summary = BenchmarkRunner.Run<BulkUpdateBenchmark>();
//var summary = BenchmarkRunner.Run<BulkUpdate9ColumnsBenchmark>();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);


//var summary = BenchmarkRunner.Run<SettingPropertyBenchmark>();
