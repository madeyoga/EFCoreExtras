## Bulk Create

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4291/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
```

| Method                                     | NumberOfItems | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Rank | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|------------------------------------------- |-------------- |---------:|---------:|---------:|---------:|------:|--------:|-----:|----------:|----------:|---------:|----------:|------------:|
| EFCoreExtras_BulkCreate                    | 2000          | 10.57 ms | 0.209 ms | 0.337 ms | 10.39 ms |  0.33 |    0.02 |    1 |  500.0000 |   93.7500 |        - |   3.05 MB |        0.22 |
| EFCoreExtras_BulkCreateRetrieve            | 2000          | 19.99 ms | 0.142 ms | 0.133 ms | 19.99 ms |  0.61 |    0.02 |    2 |  718.7500 |  218.7500 |        - |   4.45 MB |        0.31 |
| EFCoreExtras_BulkCreateRetrieve_BeginTrack | 2000          | 24.19 ms | 0.295 ms | 0.276 ms | 24.28 ms |  0.74 |    0.03 |    3 | 1093.7500 |  500.0000 | 156.2500 |   6.19 MB |        0.44 |
| EFCore_SaveChanges                         | 2000          | 32.04 ms | 0.640 ms | 1.605 ms | 32.93 ms |  1.00 |    0.00 |    4 | 2500.0000 | 1000.0000 | 250.0000 |  14.15 MB |        1.00 |


## Bulk Update

### 2 Columns

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4291/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  Job-QWSAUY : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  
```

| Method                  | NumberOfItems | Mean       | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------ |-------------- |-----------:|----------:|----------:|------:|--------:|-----:|----------:|----------:|----------:|------------:|
| EFCore_ExecuteUpdate    | 4000          |   6.062 ms | 0.1155 ms | 0.0964 ms |  1.00 |    0.00 |    1 |         - |         - |   1.96 MB |        1.00 |
| EFCoreExtras_BulkUpdate | 4000          |  32.042 ms | 0.6223 ms | 0.9688 ms |  5.37 |    0.16 |    2 | 2000.0000 | 1000.0000 |   12.1 MB |        6.16 |
| EFCore_SaveChanges      | 4000          | 102.479 ms | 1.3585 ms | 1.1344 ms | 16.91 |    0.36 |    3 | 4000.0000 | 1000.0000 |  27.64 MB |       14.08 |


### 9 Columns

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4291/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  Job-UDHWWW : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  
```

| Method                               | NumberOfItems | Mean     | Error   | StdDev  | Ratio | RatioSD | Rank | Gen0      | Gen1      | Allocated | Alloc Ratio |
|------------------------------------- |-------------- |---------:|--------:|--------:|------:|--------:|-----:|----------:|----------:|----------:|------------:|
| EFCoreExtras_BulkUpdate_BatchSize10  | 4000          | 107.0 ms | 1.95 ms | 1.73 ms |  0.88 |    0.01 |    1 | 6000.0000 | 1000.0000 |  40.41 MB |        1.19 |
| EFCore_SaveChanges                   | 4000          | 121.2 ms | 1.26 ms | 0.98 ms |  1.00 |    0.00 |    2 | 5000.0000 | 1000.0000 |  33.99 MB |        1.00 |
| EFCoreExtras_BulkUpdate_BatchSize100 | 4000          | 242.3 ms | 2.47 ms | 2.19 ms |  2.00 |    0.03 |    3 | 5000.0000 | 1000.0000 |  34.96 MB |        1.03 |

