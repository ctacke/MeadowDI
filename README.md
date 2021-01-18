# Meadow Dependency Injection

A test run on a DI container for Wilderness Labs Meadow

## The `ServiceCollection`

The `MeadowApp` class contains a static `ServiceCollection` instance, which is a very simple, but still powerful Dependency Injection (DI) container.

The `ServiceCollection` supports:
- Singleton instance registration by type (class or interface)
- Resolving a contained instance by type (class or interface)
- Registering an existing type instance
- Registering a Type and the cotainer will auto-create it
- Simple constructor injection on creation
- Simple property injection on creation

## Adding an instace that already exists

You can directly add an instance.  It will be registered by direct type name:

```
var myObject = new Thing();
MeadowApp.Services.Add(myObject)
```

You can optionally register it as anything it derives from:

```
var myObject = new Thing(); // <-- implements IThing
MeadowApp.Services.Add<IThing>(myObject);
```

## Automatic Instance Creation

You can have the DI container create an object for you (with injection) as follows:

```
var myObject = MeadowApp.Services.Create<Thing>()
```

You can optionally register it as anything it derives from:

```
var myObject = MeadowApp.Services.Create<Thing, IThing>(); <-- Thing implements IThing
```

## Retrieving an Object

You simply resolve and retrieve by type:

```
var thing = MeadowApp.Services.Get<Thing>()
```

or

```
var thing = MeadowApp.Services.Get<IThing>()
```


## Constructor Injection

If a constructor exists that has parameters that can be fulfilled by existing registered objects, that constructor will be called.  The container will *not* cascade create objects, so if your contructor needs something that is not yet registered, it will fail.

## Property Injection

Property injection happed after construction, but works similarly.  If you have a public, settable property of a Type that is registered, the property will be set immediately after construction.

## Types in the base implementation

The default DI Container starts with the global `F7Micro` Device object already registered, so if you create a service that needs that object, you can use injection to retrieve it.

> Example

```
var myService = MeadowApp.Services.Create<ILedService>();

...

public LedService(F7Micro device)
{
    // F7Micro will get injected here
}
```