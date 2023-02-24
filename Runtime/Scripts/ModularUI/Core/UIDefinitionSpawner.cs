using System;
using System.Collections.Generic;
using UnityEngine;

namespace GentlyUI.ModularUI {
    public static class UIDefinitionSpawner
    {
        private static Dictionary<RectTransform, UIDefinition> spawnedDefinitions = new Dictionary<RectTransform, UIDefinition>();

        /// <summary>
        /// Spawn UI from a definition class.
        /// </summary>
        /// <typeparam name="T">The UI Definition type.</typeparam>
        /// <param name="container">The container to spawn the ui definition into.</param>
        /// <returns>Returns a reference to the ui definition instance.</returns>
        public static T SpawnDefinition<T>(RectTransform container, object data = null) where T : UIDefinition {
            return SpawnDefinition(container, typeof(T), data) as T;
        }

        public static UIDefinition SpawnDefinition(RectTransform container, Type definitionType, object data = null) {
            //Despawn old definition in container if there was one spawned
            DespawnDefinition(container);

            //Add UIDefinitionContainer to container
            UIDefinitionContainer uiDefContainer = container.gameObject.GetOrAddComponent<UIDefinitionContainer>();

            UIDefinition def = (UIDefinition)Activator.CreateInstance(definitionType, new object[] { uiDefContainer, data });
            spawnedDefinitions[container] = def;

            return def;
        }

        public static void DespawnDefinition(RectTransform container) {
            if (spawnedDefinitions.ContainsKey(container)) {
                spawnedDefinitions[container].DisposeUI();
                spawnedDefinitions.Remove(container);
            }
        }
    }
}
