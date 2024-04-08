using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using PositionEvents;
using PositionEvents.Area;
using Flyga.PositionEventsModule;

namespace PositionEventsExample
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        private static PositionEventsModule _positionEventsModule;

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

        protected override Task LoadAsync()
        {
            // Retrieve a reference to the Position Events Module instance
            foreach (ModuleManager item in GameService.Module.Modules)
            {
                if (item.Manifest.Namespace == "Flyga.PositionEvents")
                {
                    if (item.ModuleInstance is PositionEventsModule positionEventsModule)
                    {
                        _positionEventsModule = positionEventsModule;
                    }
                    else
                    {
                        Logger.Error("Unable to detect required Position Events Module.");
                    }
                    
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void AddTestAreas()
        {
            // create the areas
            IBoundingObject area = new BoundingObjectBox(new BoundingBox(new Vector3(0), new Vector3(10, 20, 30)));
            IBoundingObject prism = GetTestPrism();
            IBoundingObject testLake = GetTestLake();
            IBoundingObject testDifference = GetTestDifference();

            // register the areas with the Position Events Module
            _positionEventsModule?.RegisterArea(this, 15, area, OnAreaJoinedOrLeft, true);
            _positionEventsModule?.RegisterArea(this, 15, prism, OnAreaJoinedOrLeft, true);
            _positionEventsModule?.RegisterArea(this, 15, testLake, OnAreaJoinedOrLeft, true);
            _positionEventsModule?.RegisterArea(this, 15, testDifference, OnAreaJoinedOrLeft, true);
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
            BoundingObjectSphere outerSphere = new BoundingObjectSphere(new BoundingSphere(new Vector3(20), 20));
            BoundingObjectSphere innerSphere = new BoundingObjectSphere(new BoundingSphere(new Vector3(23), 8));

            return new BoundingObjectGroupDifference(outerSphere, new IBoundingObject[] { innerSphere }, false);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Add test areas, once the module is loaded.
            AddTestAreas();

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

            _positionEventsModule = null;
        }

    }

}
