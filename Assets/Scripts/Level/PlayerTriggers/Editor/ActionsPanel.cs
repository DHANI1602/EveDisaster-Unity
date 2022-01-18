using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine.UIElements;

namespace Game.Level.Triggers
{
    internal sealed class PlayerTriggerActionsList : VisualElement
    {
        private static readonly List<Type> types = typeof(PlayerTriggerVolume).Assembly.GetTypes().Where(e => !e.IsAbstract && typeof(PlayerTriggerAction).IsAssignableFrom(e)).ToList();

        public string text {
            get => this.Q<ListPanel>("List").text;
            set => this.Q<ListPanel>("List").text = value;
        }

        public PlayerTriggerActionsList()
        {
            ListPanel list = new ListPanel();
            {
                list.name = "List";
                list.style.flexGrow = 1;

                ListView addList = new ListView(types, 20, () => new Label(), (e, i) => ((Label)e).text = types[i].ToString());
                {
                    addList.selectionType = SelectionType.Single;
                    addList.style.flexGrow = 1;
                    addList.onItemsChosen += e => list.Add(Activator.CreateInstance((Type)e.First()));
                    addList.style.height = 160;
                }
                list.addPanel = addList;
            }
            hierarchy.Add(list);
        }

        public void BindProperty(SerializedProperty obj)
            => this.Q<ListPanel>("List").BindProperty(obj);
    }
}