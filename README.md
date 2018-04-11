AdvancedDLSupport.Benchmark
===========================

This repo contains a small test application for benchmarking different ways of native interop. Currently, it tests 
traditional `DllImport` statements, delegate-based techniques, and the `calli` opcode. Each permutation is tested 
against a by-reference and a by-value parameter.

The native code accepts a `Matrix2f`, and performs a simple matrix inversion on it.

Each permutation is timed over 10k iterations, with a 100 iteration warmup, and is presented as a per-iteration time 
(in milliseconds).
