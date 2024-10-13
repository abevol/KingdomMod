using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif

namespace KingdomMod
{
    [Serializable]
    public class GameObjectDetails
    {
        #region[Declarations]

        public string name = "";
        public string tag = "";
        public string parent = "";
        public bool enabled = false;
        public int layer = -1;
        public int prefabID = -1;
        public string position = "";
        public string localPosition = "";
        public System.Collections.Generic.List<string> components = new();
        public System.Collections.Generic.List<GameObjectDetails> children = new();

        #endregion

        public GameObjectDetails() { }

        public GameObjectDetails(GameObject rootObject)
        {
            this.name = rootObject.name;
            this.tag = rootObject.tag == "Untagged" ? "" : rootObject.tag;
            
            if (rootObject.transform.parent != null)
            {
                this.parent = rootObject.transform.parent.gameObject.name;
            }
            else
            {
                this.parent = "null";
            }

            this.enabled = rootObject.activeSelf;
            this.layer = rootObject.layer;
            this.position = rootObject.transform.position.ToString();
            this.localPosition = rootObject.transform.localPosition.ToString();

            var prefabIdComp = rootObject.GetComponent<PrefabID>();
            if (prefabIdComp != null)
                this.prefabID = prefabIdComp.prefabID;

            var tmpComps = rootObject.GetComponents<Component>();
            foreach(var comp in tmpComps)
            {
                if (!comp) continue;
                components.Add(comp.
#if IL2CPP
                    GetIl2CppType()
#else
                    GetType()
#endif
                    .FullName);
            }

            var childCount = rootObject.transform.childCount;
            for (int idx = 0; idx < childCount; idx++)
            {
                this.children.Add(new GameObjectDetails(rootObject.transform.GetChild(idx).gameObject));
            }
        }

        // internal delegate void getRootSceneObjects(int handle, IntPtr list);
        //
        // // Resolve the GetRootGameObjects ICall (internal Unity MethodImp functions)
        // internal static getRootSceneObjects getRootSceneObjects_iCall =
        //     IL2CPP.ResolveICall<getRootSceneObjects>("UnityEngine.SceneManagement.Scene::GetRootGameObjectsInternal");
        //
        // private static void GetRootGameObjects_Internal(Scene scene, IntPtr list)
        // {
        //     getRootSceneObjects_iCall(scene.handle, list);
        // }

        public static System.Collections.Generic.List<GameObject> GetAllScenesGameObjects()
        {
            Scene[] array = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                array[i] = SceneManager.GetSceneAt(i);
            }

            var allObjectsList = new System.Collections.Generic.List<GameObject>();
            foreach (var scene in array)
            {
#if IL2CPP
                var list = new List<GameObject>(scene.rootCount);
                Scene.GetRootGameObjectsInternal(scene.handle, list);
#else
                var list = new System.Collections.Generic.List<GameObject>(scene.rootCount);
                typeof(Scene).GetMethodDelegate<Action<int, object>>("GetRootGameObjectsInternal")(scene.handle, list);
#endif
                foreach (var obj in list)
                {
                    allObjectsList.Add(obj);
                }
            }

            #region[DevNote]

            /*
            The reason these differ are that GetAllScenesObjects doen't get DontDestroyOnLoad objects and it maintains heirarchy so it looks like alot less
            For example: GetAllScenesObjects() doesn't find this Trainer and the games StageLoadManager object.
            */
            //log.LogMessage("AllScenesObject's Count: " + allObjectsList.Count.ToString());
            //log.LogMessage("FindAll<GameObject>() Count: " + GameObject.FindObjectsOfType<GameObject>().Count.ToString());

            #endregion

            return allObjectsList;
        }

        public static string XMLSerialize(System.Collections.Generic.List<GameObjectDetails> objectTree)
        {
            string xml = "<?xml version=\"1.0\"?>\r\n";

            xml += "<GameObjects>\r\n";
            xml += "<Count value=\"" + objectTree.Count.ToString() + "\" />\r\n";
            foreach(var obj in objectTree)
            {
                xml += "<GameObject name=\"" + obj.name + "\">\r\n";
                xml += CreateXMLGameObject(obj);
                xml += "</GameObject>\r\n";
            }

            xml += "</GameObjects>";

            return xml;
        }

        private static string CreateXMLGameObject(GameObjectDetails obj)
        {
            string xml = "";

            xml += "<parent name=\"" + obj.parent + "\" />\r\n";
            xml += "<enabled value=\"" + obj.enabled.ToString() + "\" />\r\n";
            xml += "<layer value=\"" + obj.layer.ToString() + "\" />\r\n";
            xml += "<prefabID value=\"" + obj.prefabID.ToString() + "\" />\r\n";
            xml += "<position value=\"" + obj.position + "\" />\r\n";
            xml += "<localPosition value=\"" + obj.localPosition + "\" />\r\n";

            xml += "<components>\r\n";
            foreach (var comp in obj.components)
            {
                xml += "<component name=\"" + comp + "\" />\r\n";
            }
            xml += "</components>\r\n";

            if (obj.children.Count > 0)
            {
                xml += "<children>\r\n";
                foreach (GameObjectDetails child in obj.children)
                {
                    xml += "<child name=\"" + child.name + "\">\r\n";
                    xml += CreateXMLGameObject(child);
                    xml += "</child>\r\n";
                }
                xml += "</children>\r\n";
            }

            return xml;
        }

        public static string JsonSerialize(System.Collections.Generic.List<GameObjectDetails> objectTree)
        {
            string json = "{";

            json += "\"Count\":" + objectTree.Count.ToString() + ",";
            json += "\"GameObjects\":[";
            foreach (var obj in objectTree)
            {
                json += CreateJsonGameObject(obj);
            }

            json = json.TrimEnd(',');
            json += "]}";
            return json;
        }

        private static string CreateJsonGameObject(GameObjectDetails obj)
        {
            string json = "{";

            json += "\"name\":\"" + obj.name + "\",";
            if (obj.tag != "")
                json += "\"tag\":\"" + obj.tag + "\",";

            json += "\"parent\":\"" + obj.parent + "\",";
            if (obj.enabled == false)
                json += "\"enabled\":" + obj.enabled.ToString().ToLower() + ",";
            json += "\"layer\":" + obj.layer.ToString() + ",";
            if (obj.prefabID != -1)
                json += "\"prefabID\":" + obj.prefabID.ToString() + ",";

            json += "\"position\":\"" + obj.position + "\",";
            json += "\"localPosition\":\"" + obj.localPosition + "\",";

            json += "\"components\":[";
            foreach (var comp in obj.components)
            {
                json += "\"" + comp + "\",";
            }
            json = json.TrimEnd(',');
            json += "],";

            if (obj.children.Count > 0)
            {
                json += "\"children\":[";
                foreach (GameObjectDetails child in obj.children)
                {
                    json += CreateJsonGameObject(child);
                }
                json = json.TrimEnd(',');
                json += "],";
            }

            json = json.TrimEnd(',');
            json += "},";
            return json;
        }
    }
}
