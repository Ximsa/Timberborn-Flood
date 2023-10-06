using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using TimberApi.UiBuilderSystem;
using UnityEngine.UIElements;


namespace Timberborn_FloodSeason
{
    public class WeatherPanel : ILoadableSingleton
    {
        private readonly UIBuilder _builder;
        private readonly UILayout _layout;

        public  VisualElement _root;
        

        public Label DroughtLabel;
        public Label BadTideLabel;
        public Label FloodLabel;

        public WeatherPanel(UILayout layout, UIBuilder builder)
        {
            _layout = layout;
            _builder = builder;
        }
        public void Load()
        {
            _root = _builder
                .CreateComponentBuilder()
                .CreateVisualElement()
                .AddClass("top-right-item")
                .AddClass("square-large--green")
                .SetFlexDirection(FlexDirection.Row)
                .SetFlexWrap(Wrap.Wrap)
                .SetJustifyContent(Justify.Center)
                .AddComponent(_builder.CreateComponentBuilder()
                    .CreateLabel()
                    .AddClass("text--centered")
                    .AddClass("text--yellow")
                    .AddClass("date-panel__text")
                    .AddClass("game-text-normal")
                    .SetName("DroughtLabel")
                    .Build())
                .AddComponent(_builder.CreateComponentBuilder()
                    .CreateLabel()
                    .AddClass("text--centered")
                    .AddClass("text--yellow")
                    .AddClass("date-panel__text")
                    .AddClass("game-text-normal")
                    .SetName("BadTideLabel")
                    .Build())
                .AddComponent(_builder.CreateComponentBuilder()
                    .CreateLabel()
                    .AddClass("text--centered")
                    .AddClass("text--yellow")
                    .AddClass("date-panel__text")
                    .AddClass("game-text-normal")
                    .SetName("FloodLabel")
                    .Build())
                .BuildAndInitialize();
            DroughtLabel = _root.Q<Label>("DroughtLabel");
            BadTideLabel = _root.Q<Label>("BadTideLabel");
            FloodLabel = _root.Q<Label>("FloodLabel");
            _layout.AddTopRight(_root, 6);
        }
    }
}
