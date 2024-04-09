using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Flyga.PositionEventsModule;
using Microsoft.Xna.Framework;
using PositionEvents;
using PositionEvents.Area;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace PositionEventsExample
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        private static ModuleManager _positionEventsModuleManager;
        private static ModuleManager _thisModuleManager;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {
            
        }

        private void OnAreaJoinedOrLeft(PositionData positionData, bool isInside)
        {
            if (isInside)
            {
                // do something, if the player joined an area.
                Logger.Info("Area joined.");
                return;
            }

            // do something else, if the player left an area.
            Logger.Info("Area left.");
        }

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

            _positionEventsModuleManager.ModuleDisabled += OnOtherModuleDisabled;

            // Add your areas, once you're sure the Position Events Module has been loaded
            AddTestAreas(positionEventsModule);
        }

        private void OnPositionEventsDisabled(ModuleManager moduleManager)
        {
            // disable this module since it's dependent on the Position Events Module
            _thisModuleManager?.Disable();

            // no need to remove the areas from the Position Events Module, since it 
            // takes care of that on it's own.
        }

        private void OnOtherModuleEnabled(object sender, EventArgs e)
        {
            if (!(sender is ModuleManager moduleManager))
            {
                return;
            }

            if (moduleManager.Manifest.Namespace != "Flyga.PositionEvents")
            {
                return;
            }

            if (!moduleManager.AssemblyLoaded)
            {
                Logger.Error("Unable to load module, because dependency Position Events Module " +
                    "could not be loaded.");
                _thisModuleManager?.Disable();
                return;
            }

            OnPositionEventsEnabled(moduleManager);
        }

        private void OnOtherModuleDisabled(object sender, EventArgs e)
        {
            if (!(sender is ModuleManager moduleManager))
            {
                return;
            }

            if (moduleManager.Manifest.Namespace != "Flyga.PositionEvents")
            {
                return;
            }

            OnPositionEventsDisabled(moduleManager);
        }

        protected override Task LoadAsync()
        {
            // set reference for this modules manager
            _thisModuleManager = GameService.Module.Modules
                .Where(moduleManager => moduleManager.Manifest.Namespace == Namespace)
                .FirstOrDefault();
            
            // Retrieve a reference to the Position Events Module instance
            foreach (ModuleManager item in GameService.Module.Modules)
            {
                if (item.Manifest.Namespace == "Flyga.PositionEvents")
                {
                    // if the assembly is already loaded, call OnPositionEventsEnabled manually
                    if (item.AssemblyLoaded)
                    {
                        OnPositionEventsEnabled(item);
                    }

                    item.ModuleEnabled += OnOtherModuleEnabled;

                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void AddTestAreas(PositionEventsModule positionEventsModule)
        {
            // create the areas
            IBoundingObject area = new BoundingObjectBox(new Vector3(50, 50, 10), new Vector3(60, 70, 40));
            IBoundingObject prism = GetTestPrism();
            IBoundingObject testLake = GetTestLake();
            IBoundingObject testDifference = GetTestDifference();

            // register the areas with the Position Events Module
            // debug flags are true for this example. Never ship your module with
            // those set to true!
            positionEventsModule.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, debug: true);
            positionEventsModule.RegisterArea(this, 15, prism, OnAreaJoinedOrLeft, debug: true);
            positionEventsModule.RegisterArea(this, 15, testLake, OnAreaJoinedOrLeft, debug: true);
            positionEventsModule.RegisterArea(this, 15, testDifference, OnAreaJoinedOrLeft, debug: true);
        }

        private IBoundingObject GetTestPrism()
        {
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(0),
                new Vector2(15),
                new Vector2(15, -3)
            };

            return new BoundingObjectPrism(10, 0, vertices);
        }

        private IBoundingObject GetTestLake()
        {
            Vector2[] vertices = new Vector2[]
            {
                new Vector2(-83.0f, 584.0f),
                new Vector2(-81.0f, 626.0f),
                new Vector2(-24.0f, 634.0f),
                new Vector2(52.0f, 594.0f),
                new Vector2(85.0f, 485.0f),
                new Vector2(195.0f, 467.0f),
                new Vector2(208.0f, 423.0f),
                new Vector2(157.0f, 386.0f),
                new Vector2(152.0f, 317.0f),
                new Vector2(56.0f, 310.0f),
                new Vector2(18.0f, 376.0f),
                new Vector2(22.0f, 416.0f),
                new Vector2(-20.0f, 438.0f),
                new Vector2(-29.0f, 538.0f)
            };

            return new BoundingObjectPrism(50, 0, vertices);
        }

        private IBoundingObject GetTestDifference()
        {
            BoundingObjectSphere outerSphere = new BoundingObjectSphere(new Vector3(-20, -20, 20), 20);
            BoundingObjectSphere innerSphere = new BoundingObjectSphere(new Vector3(-23, -26, 23), 8);

            BoundingObjectBuilder builder = new BoundingObjectBuilder()
                .Add(outerSphere)
                .Subtract(innerSphere);

            return builder.Build();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            /** NOOP **/
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset

            if (_positionEventsModuleManager != null)
            {
                _positionEventsModuleManager.ModuleEnabled -= OnOtherModuleEnabled;
                _positionEventsModuleManager.ModuleDisabled -= OnOtherModuleDisabled;
            }

            _positionEventsModuleManager = null;
            _thisModuleManager = null;
        }

    }

}
