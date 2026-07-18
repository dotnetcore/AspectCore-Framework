# Data Validation

`AspectCore.Extensions.DataAnnotations` uses an interceptor to validate parameters before method execution, reusing the standard .NET `System.ComponentModel.DataAnnotations` attributes (`[Required]`, `[EmailAddress]`, `[StringLength]`, etc.) directly as validation rules. The validation result is exposed to the service through `IDataState`, and the business code decides how to handle it, rather than throwing an exception directly.

## The Relationship Between the Two Packages

Data validation involves two packages in an "infrastructure + implementation" relationship:

- `AspectCore.Extensions.DataValidation`: the validation infrastructure, providing abstractions and the interceptor such as `IDataValidator`, `IPropertyValidator`, `IDataState`, and `DataValidationInterceptorAttribute`. It **does not provide** a one-line registration entry point for `IServiceContext`.
- `AspectCore.Extensions.DataAnnotations`: a concrete implementation based on DataValidation, which validates using `System.ComponentModel.DataAnnotations` and provides the user entry point `AddDataAnnotations(...)`.

Users generally only need to install and call `AspectCore.Extensions.DataAnnotations`; although the shared interceptor `DataValidationInterceptorAttribute` physically resides in the DataValidation package, you enable it through `AddDataAnnotations`.

## Installation and Enabling

```bash
dotnet add package AspectCore.Extensions.DataAnnotations
```

Call `AddDataAnnotations(...)` on the built-in container:

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.DataAnnotations;

var services = new ServiceContext();
services.AddType<IAccountService, AccountService>();
services.AddDataAnnotations();

var resolver = services.Build();
var accountService = resolver.Resolve<IAccountService>();
```

The signature of `AddDataAnnotations` is `IServiceContext AddDataAnnotations(this IServiceContext services, params AspectPredicate[] predicates)`. It registers the validator implementation and adds `DataValidationInterceptorAttribute` to the global interceptors; passing `predicates` constrains the scope of validation (usage is the same as in [Conditional Interception](./conditional-interception.md)).

## Defining Rules with DataAnnotations Attributes

Validation rules are written on the parameter model using the standard `System.ComponentModel.DataAnnotations` attributes:

```csharp
using System.ComponentModel.DataAnnotations;

public class RegisterInput
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(18, MinimumLength = 6)]
    public string Password { get; set; }
}
```

## Reading the Validation Result: IDataState

The interceptor completes validation before method execution and places the result into the service's `IDataState DataState` property. The business method uses it to decide whether validation passed:

```csharp
using AspectCore.Extensions.DataValidation;

public class AccountService : IAccountService
{
    public IDataState DataState { get; set; }

    public void Register(RegisterInput input)
    {
        if (DataState.IsValid)
        {
            Console.WriteLine($"register.. name:{input.Name}, email:{input.Email}");
            return;
        }

        foreach (var error in DataState.Errors)
        {
            Console.WriteLine($"error.. key:{error.Key}, message:{error.ErrorMessage}");
        }
    }
}
```

`IDataState` provides two members:

- `bool IsValid` — whether all parameters passed validation.
- `DataValidationErrorCollection Errors` — the collection of failed validation items, each carrying a `Key` and an `ErrorMessage`.

This pattern of "the business reads `DataState` after validation" leaves the decision of "whether to proceed" to the business code, rather than having the framework abort the call directly.

## Next Steps

- [Interceptor Configuration](./interceptor-configuration.md) — how the validation interceptor is registered and scoped.
- [Dependency Injection Integration](./dependency-injection.md) — usage of the built-in container `IServiceContext`.
- [Common Scenarios](./common-scenarios.md) — an example of hand-writing a parameter validation interceptor.
