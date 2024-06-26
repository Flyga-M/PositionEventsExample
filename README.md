# PositionEventsExample
Provides an example of how to make use of
 the [Position Events Module](https://github.com/Flyga-M/PositionEventsModule).

## Setup
1. Add the PositionEvents Package as a reference to your module. It is available as a [NuGet](https://www.nuget.org/packages/PositionEvents) package.
2. Add the Position Events Module [.dll](https://github.com/Flyga-M/PositionEventsModule/releases/) as a reference to your module.
3. Add the Position Events Module as a dependency to your module manifest.
```
dependencies":{
    "bh.blishhud": "^1.0.0",
    "Flyga.PositionEvents": "^0.3.0"
}
```
> [!TIP]
> Make sure that the references of the PositionEvents Package and the Position Events Module have **Copy Local** set
> to **False** in the properties of the reference. You don't need to ship them with your module, since they will
> already be present, because of your modules dependence on the Position Events Module.
## Retrieving a reference to the Position Events Context
To be able to use the functionality of the Position Events Module, you need to retrieve
 it's `Context`. With this `PositionEventsContext` you can call it's methods to
  add and remove your areas.

> [!IMPORTANT]
> To make sure the context can be loaded, you have to wait for the Position Events
> Module to fully load, before retrieving it's context.
>
> In this is example this is solved by listening to the `ModuleManager.ModuleEnabled`
> and `Module.ModuleLoaded` events.
### LoadAsync
If the Position Events Module was enabled before your module is loaded, you can directly
 assign and use the Position Events Context (via OnPositionEventsEnabled).

If the Position Events Module loads after your module, you can't directly assign and use it,
 because it would throw an Exception, since the required assemblies are not loaded yet.
 This is solved by listening to the ModuleManager.ModuleEnabled event for the Position
 Events Module.
```
protected override Task LoadAsync()
{
    // [...] irrelevant parts for this explanation. For combined source look at Module.cs
    
    // Retrieve a reference to the Position Events Context
    foreach (ModuleManager item in GameService.Module.Modules)
    {
        if (item.Manifest.Namespace == POSITION_EVENTS_MODULE_NAMESPACE)
        {
            // if the assembly is already loaded, call OnPositionEventsEnabled manually
            if (item.AssemblyLoaded)
            {
                OnPositionEventsEnabled(item);
            }

            // make sure to retrieve the context only after the
            // Position Events Module was enabled (and therefor the
            // assembly was loaded)
            item.ModuleEnabled += OnOtherModuleEnabled;

            break;
        }
    }

    return Task.CompletedTask;
}
```
### OnOtherModuleEnabled
If the Position Events Module and it's assembly was loaded, you can assign and use the
 Position Events Context (via OnPositionEventsEnabled).

Make sure to unbox the ModuleManager and check if it's the manager of the Position Events
 Module.
```
private void OnOtherModuleEnabled(object sender, EventArgs e)
{
    if (!(sender is ModuleManager moduleManager))
    {
        throw new ArgumentException("OnOtherModuleEnabled must be called " +
            "by a ModuleManager.", nameof(sender));
    }

    if (moduleManager.Manifest.Namespace != POSITION_EVENTS_MODULE_NAMESPACE)
    {
        throw new ArgumentException("OnOtherModuleEnabled must be called " +
            $"by the ModuleManager of the {POSITION_EVENTS_MODULE_NAMESPACE} " +
            "module.", nameof(sender));
    }

    if (!moduleManager.AssemblyLoaded)
    {
        Logger.Error($"Unable to load module, because dependency " +
            $"{POSITION_EVENTS_MODULE_NAMESPACE} module" +
            "could not be loaded.");
        // [...] irrelevant parts for this explanation. For combined source look at Module.cs
        return;
    }

    OnPositionEventsEnabled(moduleManager);
}
```
### OnPositionEventsEnabled
After the Position Events Module has been enabled, you can finally use it's context to
 register your areas.

Here you can also save a reference to the module manager for later use.
```
private void OnPositionEventsEnabled(ModuleManager moduleManager)
{
    if (!(moduleManager.ModuleInstance is PositionEventsModule positionEventsModule))
    {
        Logger.Error($"Unable to detect required Position Events Module: {moduleManager.ModuleInstance?.GetType()}");
        // [...] irrelevant parts for this explanation. For combined source look at Module.cs
        return;
    }

    // save a reference to the ModuleManager for later use
    _positionEventsModuleManager = moduleManager;

    if (!positionEventsModule.Loaded)
    {
        // if the Position Events Module is not loaded yet, come back when it is
        positionEventsModule.ModuleLoaded += OnPositionEventsLoaded;
        return;
    }
    else
    {
        positionEventsModule.ModuleLoaded -= OnPositionEventsLoaded;
    }

    // [...] irrelevant parts for this explanation. For combined source look at Module.cs

     // Retrieve the context, once you're sure the Position Events Module has been loaded
    RetrieveContext();

    // Add your areas, once you're sure the Position Events Module has been loaded
    AddTestAreas();
}
```
### OnPositionEventsLoaded
This just loops back to `OnPositionEventsEnabled`,
 when the Position Events Module was loaded.
```
private void OnPositionEventsLoaded(object _, EventArgs _1)
{
    OnPositionEventsEnabled(_positionEventsModuleManager);
}
```
### RetrieveContext
Finally you can retrieve the Position Events Context to register your areas
 afterwards.
```
private void RetrieveContext()
{
    _positionEventsContext = GameService.Contexts.GetContext<PositionEventsContext>();
}
```
## Building and registering your areas
Now that you have a reference to the Position Events Context and you made sure
 the Position Events Module is loaded, you can register your areas.
```
private void AddTestAreas(PositionEventsModule positionEventsModule)
{
    if (_positionEventsContext == null)
    {
        Logger.Error("Unable to add test areas, since the context was not retrieved.");
        return;
    }
    
    // create the areas
    IBoundingObject area = new BoundingObjectBox(new Vector3(50, 50, 10), new Vector3(60, 70, 40));
    IBoundingObject prism = GetTestPrism();
    IBoundingObject testLake = GetTestLake();
    IBoundingObject testDifference = GetTestDifference();

    // register the areas with the Position Events Context
    // debug flags are true for this example. Never ship your module with
    // those set to true!
    positionEventsModule.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, debug: true);
    positionEventsModule.RegisterArea(this, 15, prism, OnAreaJoinedOrLeft, debug: true);
    positionEventsModule.RegisterArea(this, 15, testLake, OnAreaJoinedOrLeft, debug: true);
    positionEventsModule.RegisterArea(this, 15, testDifference, OnAreaJoinedOrLeft, debug: true);
}
```
> [!TIP]
> For more information on area types and how to use them, please take a look at the
> [Types of areas](https://github.com/Flyga-M/PositionEventsModule?tab=readme-ov-file#types-of-areas-bounding-objects)
> section of the Position Events Module Readme.
>
> For the implementation of `GetTestPrism()`, `GetTestLake()` and `GetTestDifference()`
> take a look at the [source code](https://github.com/Flyga-M/PositionEventsExample/blob/master/Module.cs).
## Handling unloading of the Position Events Module
If the Position Events Module is disabled, you can no longer register or remove areas and you
 won't get any more updates about the character position inside or outside your areas.

> [!IMPORTANT]
> The preferred way of handling this dependency issue, is to disable your module, when the
> Position Events Module is disabled.
### LoadAsync
To be able to disable your module more easily, you can retrieve a reference to your modules manager
 while your module is loaded.
```
protected override Task LoadAsync()
{
    // set reference for this modules manager
    _thisModuleManager = GameService.Module.Modules
        .Where(moduleManager => moduleManager.Manifest.Namespace == Namespace)
        .FirstOrDefault();

    // [...] irrelevant parts for this explanation. For combined source look at Module.cs
}
```
### OnPositionEventsEnabled
If the module instance can't be unboxed as PositionEventsModule, we can't use it. This
 should generally never happen. Just in case it does, you can log the error and disable
 your module.

To be notified, when the Position Events Module is disabled, you can subscribe to the
 ModuleManager.ModuleDisabled event.
```
private void OnPositionEventsEnabled(ModuleManager moduleManager)
{
    if (!(moduleManager.ModuleInstance is PositionEventsModule positionEventsModule))
    {
        Logger.Error($"Unable to detect required Position Events Module: {moduleManager.ModuleInstance?.GetType()}");
        // disable this module
        _thisModuleManager?.Disable();
        return;
    }
    _positionEventsModuleManager = moduleManager;

    // [...] irrelevant parts for this explanation. For combined source look at Module.cs

    _positionEventsModuleManager.ModuleDisabled += OnOtherModuleDisabled;

    // [...] irrelevant parts for this explanation. For combined source look at Module.cs
}
```
### OnOtherModuleDisabled
Similar to OnOtherModuleEnabled, you should unbox the module manager and make sure, that
it is the manager of the Position Events Module.
```
private void OnOtherModuleDisabled(object sender, EventArgs e)
{
    if (!(sender is ModuleManager moduleManager))
    {
        throw new ArgumentException("OnOtherModuleEnabled must be called " +
            "by a ModuleManager.", nameof(sender));
    }

    if (moduleManager.Manifest.Namespace != POSITION_EVENTS_MODULE_NAMESPACE)
    {
        throw new ArgumentException("OnOtherModuleEnabled must be called " +
            $"by the ModuleManager of the {POSITION_EVENTS_MODULE_NAMESPACE} " +
            "module.", nameof(sender));
    }

    OnPositionEventsDisabled(moduleManager);
}
```
### OnPositionEventsDisabled
When you're sure, the Position Events Module was disabled, your module should also be
 disabled.
```
private void OnPositionEventsDisabled(ModuleManager moduleManager)
{
    // disable this module since it's dependent on the Position Events Module
    _thisModuleManager?.Disable();
}
```
## Clean up after yourself
When your module is unloaded, you should make sure to unset all static members and
 unsubscribe from the events you subscribed to.

You don't need to remove your registered areas, because the Position Events Module
 takes care of that.
 ```
 protected override void Unload()
{
    // Unload here

    // All static members must be manually unset

    if (_positionEventsModuleManager != null)
    {
        _positionEventsModuleManager.ModuleEnabled -= OnOtherModuleEnabled;
        _positionEventsModuleManager.ModuleDisabled -= OnOtherModuleDisabled;
    }

    // no need to remove the areas from the Position Events Module, since it 
    // takes care of that on it's own.

    _positionEventsModuleManager = null;
    _thisModuleManager = null;
    _positionEventsContext = null;
}
 ```