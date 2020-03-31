## AspectCore Framework
[![Build status](https://ci.appveyor.com/api/projects/status/1awhaosnfcjbad77?svg=true)](https://ci.appveyor.com/project/liuhaoyang/aspectcore-framework)
[![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/dotnetcore/AspectCore/blob/dev/LICENSE)  
AspectCore is an Aspect-Oriented Programming based cross platform framework for .NET Core and .NET Framework.  
  
Core support for aspect-interceptor, dependency injection integration, web applications, data validation, and more.

## Nuget Packages

### Core library
| Package Name |  NuGet | MyGet | Downloads  |
|--------------|  ------- |  ------- |  ----  |
| AspectCore.Abstractions  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Abstractions) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Abstractions.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Abstractions) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Abstractions.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.AspNetCore?groupby=Version) |
| AspectCore.Core  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Core.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Core) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Core.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Core) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Core.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.AspNetCore?groupby=Version)  |
| AspectCore.Extensions.Reflection  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.Reflection.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.Reflection) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.Reflection.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.Reflection) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.Reflection.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.Reflection?groupby=Version)   |

### Integration library
| Package Name |  NuGet | MyGet | Downloads |
|--------------|  ------- |  ------- |  ---- |
| AspectCore.Extensions.DependencyInjection  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.DependencyInjection) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.DependencyInjection.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.DependencyInjection) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.DependencyInjection?groupby=Version) |
| AspectCore.Extensions.Autofac  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.Autofac.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.Autofac) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.Autofac.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.Autofac) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.Autofac.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.Autofac?groupby=Version) |
| AspectCore.Extensions.Windsor  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.Windsor.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.Windsor) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.Windsor.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.Windsor) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.Windsor.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.Windsor?groupby=Version) |
| AspectCore.Extensions.LightInject  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.LightInject.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.LightInject) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.LightInject.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.LightInject) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.LightInject.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.LightInject?groupby=Version) |
| AspectCore.Extensions.Hosting  | [![nuget](https://img.shields.io/nuget/v/AspectCore.Extensions.Hosting.svg?style=flat-square)](https://www.nuget.org/packages/AspectCore.Extensions.Hosting) | [![myget](https://img.shields.io/myget/aspectcore/vpre/AspectCore.Extensions.Hosting.svg?style=flat-square)](https://www.myget.org/feed/aspectcore/package/nuget/AspectCore.Extensions.Hosting) | [![stats](https://img.shields.io/nuget/dt/AspectCore.Extensions.Hosting.svg?style=flat-square)](https://www.nuget.org/stats/packages/AspectCore.Extensions.Hosting?groupby=Version) |

## Docs
* [IoC container and dependency injection in AspectCore](https://github.com/dotnetcore/AspectCore-Framework/blob/master/docs/injector.md)  
* [Reflection extension in AspectCore](https://github.com/dotnetcore/AspectCore-Framework/blob/master/docs/reflection-extensions.md)

## Components  
* [Autofac Adapter](https://github.com/dotnetcore/AspectCore-Framework/tree/master/src/AspectCore.Extensions.Autofac)
* [DataValidation](https://github.com/dotnetcore/AspectCore-Framework/tree/master/src/AspectCore.Extensions.DataValidation)
* [IoC & DynamicProxy](https://github.com/dotnetcore/AspectCore-Framework/tree/master/src/AspectCore.Core)
* [Microsoft.Extensions.DependencyInjection Adapter](https://github.com/dotnetcore/AspectCore-Framework/tree/master/src/AspectCore.Extensions.DependencyInjection)
* [Reflection](https://github.com/dotnetcore/AspectCore-Framework/tree/master/src/AspectCore.Extensions.Reflection)   

## Who is using
* [ButterflyAPM Client](https://github.com/ButterflyAPM/butterfly-csharp)
* [Bing(jianxuanbing)](https://github.com/jianxuanbing/Bing)
* [DotnetSpider](https://github.com/dotnetcore/DotnetSpider)
* [EasyCaching](https://github.com/catcherwong/EasyCaching)
* [shriek-fx](https://github.com/ElderJames/shriek-fx)   
* [Util](https://github.com/dotnetcore/Util)
* [Zxw.Framework.NetCore](https://github.com/VictorTzeng/Zxw.Framework.NetCore)
* [Tars.Net](https://github.com/TarsNET)

## Contributors
* [Savorboard](https://github.com/yuleyule66)  
* [AlexLEWIS](https://github.com/alexinea)
* [Konrad Banaszek](https://github.com/thecorrado)

## Contribute
One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

## License
[MIT](https://github.com/dotnetcore/AspectCore-Framework/blob/master/LICENSE)
