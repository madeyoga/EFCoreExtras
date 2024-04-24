```
BenchmarkDotNet v0.13.12, macOS Sonoma 14.4.1 (23E224) [Darwin 23.4.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.1 (8.0.123.58001), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.1 (8.0.123.58001), Arm64 RyuJIT AdvSIMD
```

| Method                               | Mean      | Error     | StdDev    | Min       | Max       | Ratio | RatioSD | Rank | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|------------------------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-----:|----------:|----------:|---------:|----------:|------------:|
| EFCoreExtras_BulkCreate_100BatchSize |  8.020 ms | 0.0392 ms | 0.0367 ms |  7.962 ms |  8.084 ms |  1.00 |    0.00 |    1 |  484.3750 |   93.7500 |        - |   2.96 MB |        1.00 |
| EFCore_SaveChanges                   | 19.149 ms | 0.1066 ms | 0.0997 ms | 18.960 ms | 19.335 ms |  2.39 |    0.02 |    2 | 2656.2500 | 1187.5000 | 781.2500 |  13.87 MB |        4.69 |
| EFCoreExtras_BulkCreate              | 78.853 ms | 0.4679 ms | 0.4147 ms | 78.206 ms | 79.494 ms |  9.84 |    0.07 |    3 |  428.5714 |  285.7143 | 142.8571 |   2.77 MB |        0.94 |
