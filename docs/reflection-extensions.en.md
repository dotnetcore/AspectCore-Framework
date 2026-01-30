## Getting AspectCore.Extension.Reflection
Get AspectCore.Extension.Reflection through nuget:
```
    Install-Package AspectCore.Extensions.Reflection -pre
```

## Constructor Reflection Extensions
Provides ConstructorReflector as the entry point for constructor reflection extensions. Usage is similar to System.Reflection.ConstructorInfo:
```
var constructorInfo = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
var reflector = constructorInfo.GetReflector();
var instance = reflector.Invoke(args);
```
Performance test (Reflection is .NET Core provided reflection call, Reflector is AspectCore.Extension.Reflection call, Native is hardcoded call, same below):
```
 |     Method |       Mean |     Error |    StdDev |    StdErr |          Op/s |  Gen 0 | Allocated |
 |----------- |-----------:|----------:|----------:|----------:|--------------:|-------:|----------:|
 | Reflection | 119.505 ns | 0.5146 ns | 0.4814 ns | 0.1243 ns |   8,367,831.8 | 0.0074 |      24 B |
 |  Reflector |   8.990 ns | 0.0403 ns | 0.0377 ns | 0.0097 ns | 111,236,649.9 | 0.0076 |      24 B |
 |     Native |   3.825 ns | 0.0620 ns | 0.0580 ns | 0.0150 ns | 261,404,148.5 | 0.0076 |      24 B |
```

## Method Invocation Reflection Extensions
Provides MethodReflector as the entry point for method reflection extensions. Usage is similar to System.Reflection.MethodInfo:
```
var typeInfo = typeof(MethodFakes).GetTypeInfo();
var method = typeInfo.GetMethod("Call");
var refector = method.GetReflector();
refector.Invoke(instance,args);
```
Performance test:
```
 |             Method |        Mean |     Error |    StdDev |    StdErr |            Op/s |
 |------------------- |------------:|----------:|----------:|----------:|----------------:|
 |        Native_Call |   1.0473 ns | 0.0064 ns | 0.0050 ns | 0.0015 ns |   954,874,046.8 |
 |    Reflection_Call |  91.9543 ns | 0.3540 ns | 0.3311 ns | 0.0855 ns |    10,874,961.4 |
 |     Reflector_Call |   7.1544 ns | 0.0628 ns | 0.0587 ns | 0.0152 ns |   139,774,408.3 |
 ```

## Property Invocation Reflection Extensions
Provides PropertyReflector as the entry point for property reflection extensions. Usage is similar to System.Reflection.PropertyInfo:
```
var property  = typeof(PropertyFakes).GetTypeInfo().GetProperty("Property");
var reflector = property.GetReflector();
var value = reflector.GetValue(instance);
```
Performance test:
```
 |                    Method |       Mean |     Error |    StdDev |    StdErr |          Op/s |  Gen 0 | Allocated |
 |-------------------------- |-----------:|----------:|----------:|----------:|--------------:|-------:|----------:|
 |       Native_Get_Property |   1.178 ns | 0.0244 ns | 0.0229 ns | 0.0059 ns | 848,858,716.1 |      - |       0 B |
 |   Reflection_Get_Property | 103.028 ns | 0.2217 ns | 0.2074 ns | 0.0535 ns |   9,706,088.1 |           |       0 B |
 |    Reflector_Get_Property |   4.172 ns | 0.0194 ns | 0.0172 ns | 0.0046 ns | 239,694,827.7 |      - |       0 B |
 |       Native_Set_Property |   2.002 ns | 0.0122 ns | 0.0114 ns | 0.0030 ns | 499,447,543.5 |      - |       0 B |
 |   Reflection_Set_Property | 188.313 ns | 0.5347 ns | 0.5002 ns | 0.1292 ns |   5,310,298.0 | 0.0203 |      64 B |
 |    Reflector_Set_Property |   5.878 ns |   0.0234 ns | 0.0219 ns | 0.0056 ns | 170,138,324.7 |      - |       0 B |
 ```

## Attribute Retrieval Extensions
Taking retrieving attributes marked on a method as an example.

Method definition:
```
[Attribute1]
[Attribute2("benchmark", Id = 10000)]
[Attribute3]
[Attribute3]
[Attribute3]
public void Method()
{
}
```
Use MethodReflector to get attributes:
```
var method = type.GetMethod("Method");
var reflector = method.GetReflector();
var attribute1 = reflector.GetCustomAttribute(typeof(Attribute1));
var attributes = reflector.GetCustomAttributes();
```
Performance test:
```
 |                                      Method |        Mean |         Op/s |  Gen 0 | Allocated |
 |-------------------------------------------- |------------:|-------------:|-------:|----------:|
 |               Reflection_GetCustomAttribute | 4,642.13 ns |    215,418.5 | 0.2289 |     744 B |
 |                               Reflector_GetCustomAttribute |    35.52 ns | 28,154,302.3 | 0.0101 |      32 B |
 | Reflection_GetCustomAttributes_WithAttrType | 5,354.49 ns |    186,759.2 | 0.3281 |    1048 B |
 |  Reflector_GetCustomAttributes_WithAttrType |   168.61 ns |  5,930,816.1 | 0.0710 |     224 B |
 |          Reflection_GetCustomAttributes_All | 7,915.45 ns |    126,335.2 | 0.5035 |    1632 B |
 |           Reflector_GetCustomAttributes_All |    98.36 ns | 10,166,253.6 | 0.0737 |     232 B |
 |                        Reflection_IsDefined | 1,723.30 ns |    580,283.6 | 0.0801 |     256 B |
 |                         Reflector_IsDefined |    35.55 ns | 28,126,759.1 |      - |       0 B |
```
You can see that AspectCore.Extension.Reflection has 2 orders of magnitude optimization in performance compared to reflection, achieving the same order of magnitude as hardcoded calls. The optimization for attribute retrieval is particularly obvious.
