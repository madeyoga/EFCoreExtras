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


```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4291/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  Job-TLNOED : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  
```

| Method                        | NumberOfItems | Mean      | Error    | StdDev   | Ratio | RatioSD | Rank | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------------ |-------------- |----------:|---------:|---------:|------:|--------:|-----:|----------:|----------:|----------:|------------:|
| EFCoreExtras_BulkUpdate_Typed | 5000          |  49.68 ms | 0.779 ms | 0.728 ms |  0.99 |    0.01 |    1 | 3000.0000 | 1000.0000 |  21.64 MB |        1.01 |
| EFCoreExtras_BulkUpdate       | 5000          |  49.98 ms | 0.788 ms | 0.698 ms |  1.00 |    0.00 |    1 | 3000.0000 | 1000.0000 |  21.54 MB |        1.00 |
| EFCore_SaveChanges            | 5000          | 119.74 ms | 1.214 ms | 1.077 ms |  2.40 |    0.04 |    2 | 5000.0000 | 1000.0000 |  34.51 MB |        1.60 |
