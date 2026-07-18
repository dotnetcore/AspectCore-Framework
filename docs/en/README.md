# AspectCore Documentation

> AspectCore is a cross-platform AOP (Aspect-Oriented Programming) framework for .NET Core and .NET Framework, providing dynamic-proxy interception, dependency-injection integration, web application support, data validation, and more.

This is the complete AspectCore documentation. English lives here; the Chinese version is under [`../`](../README.md)（中文文档见 [`../`](../README.md)）.

## Navigation

### 🚀 Getting Started
Start using AspectCore from scratch.

- [Installation](./getting-started/installation.md) — NuGet packages and target frameworks
- [Quick Start](./getting-started/quick-start.md) — your first interceptor in five minutes
- [Core Concepts](./getting-started/concepts.md) — interceptor, proxy, aspect context and other terms

### 📖 Guide
Feature documentation for day-to-day development.

- [Interceptor Basics](./guide/interceptor.md) — defining interceptors, attribute interceptors, global interceptors
- [Interceptor Configuration](./guide/interceptor-configuration.md) — the three registration styles and scope predicates
- [Async Interception](./guide/async-interception.md) — Task / ValueTask / IAsyncEnumerable
- [Conditional Interception](./guide/conditional-interception.md) — matching by namespace / service / method
- [Dependency Injection](./guide/dependency-injection.md) — the built-in container and Microsoft.Extensions.DependencyInjection
- [Third-Party Containers](./guide/third-party-containers.md) — Autofac / Windsor / LightInject
- [Configuration Injection](./guide/configuration-injection.md) — binding values from IConfiguration
- [Data Validation](./guide/data-validation.md) — DataAnnotations validation interception
- [Reflection Extensions](./guide/reflection-extensions.md) — AspectCore.Extensions.Reflection high-performance reflection
- [Common Scenarios](./guide/common-scenarios.md) — logging, caching, retry, performance monitoring, and more

### 🏛 Architecture
Design documents for contributors and advanced users.

- [Overview](./architecture/overview.md) — layering and runtime flows
- [Module & Package Design](./architecture/module-design.md) — responsibilities and dependency direction across the 14 packages
- [DynamicProxy Runtime Engine](./architecture/dynamic-proxy.md) — runtime proxying based on Reflection.Emit
- [Source Generator Compile-Time Engine](./architecture/source-generator.md) — compile-time proxying based on Roslyn
- [Engine Comparison & Selection](./architecture/engine-comparison.md) — DynamicProxy vs SourceGenerator vs Auto
- [C# Language Feature Adaptation](./architecture/language-features.md) — how C# 6 ~ C# 13 features are adapted in AOP emit
- [Record Type Support](./architecture/record-support.md) — the two-engine differences for C# 9 record proxying

### 🔧 Development
For contributors to this repository.

- [Building](./development/building.md) — restore, compile, target frameworks
- [Project Structure](./development/project-structure.md) — source, tests, samples, benchmarks layout
- [Contributing](./development/contributing.md) — branching, commits, PR flow

### ✅ Testing
- [Testing Strategy](./testing/testing-strategy.md) — unit, dual-engine parity, E2E, coverage gates
- [Running Tests](./testing/running-tests.md) — how to run and filter tests

## About This Documentation

This documentation is generated from a source-code scan and aims to stay consistent with the code. It clearly separates "implemented capabilities" from "design / proposals"; unimplemented content is explicitly marked. The product version is defined in `build/version.props`.
