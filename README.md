# NullGuard

The null reference has been called "my billion dollar mistake" by its inventor, Tony Hoare. Guarding against null 
arguments and return values ends up resulting in a lot of boilerplate code.

This library attempts to remove the repetitive work of all that argument null checking by rewriting IL to check 
all nullable arguments. For more details, check out the [blog post announcing this library](http://haacked.com/archive/2013/01/04/mitigate-the-billion-dollar-mistake-with-aspects.aspx).

# Install this library.

```
Install-Package NullGuard.PostSharp
```

You can apply the `EnsureNonNullAspect` attribute to individual classes, methods, or an entire assembly. You'll also
need to install the [PostSharp Visual Studio Extension](http://www.sharpcrafters.com/postsharp/download). This library 
should work fine with the free community edition.

# Usage Examples

```
using NullGuard;

[assembly: EnsureNonNullAspect]

public class Sample 
{
    public void SomeMethod(string arg) {
        // throws ArgumentNullException if arg is null.
    }

    public void AnotherMethod([AllowNull]string arg) {
        // arg may be null here
    }

    public string MethodWithReturn() {
        // Throws InvalidOperationException if return value is null.
    }
   
    // Null checking works for automatic properties too.
    public string SomeProperty { get; set; }

    [AllowNull] // can be applied to a whole property
    public string NullProperty { get; set; }

    public string NullProperty { 
        get; 
        [param: AllowNull] // Or just the setter.
        set; 
}
```