using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BrilliantSkies.Core.Collections;
using BrilliantSkies.Core.ImageFinder;

namespace AdvShields
{
    public static class TypeStorage
    {
        private static Dictionary<string, HashSet<AdvShieldProjector>> StorageContainer { get; set; }
        /*
        private static Dictionary<string, HashSet<ShieldHardpoint>> ShieldStorageContainer { get; set; }
        private static Dictionary<string, HashSet<ShieldNode>> ShieldNodeContainer { get; set; }
        private static Dictionary<string, HashSet<ShieldNodeSet>> ShieldNodeSetContainer { get; set; }
        //private static DictionaryOfTypesWithConstructor <HashSet<ShieldNodeSet>> ShieldNodeSetContainer { get; set; }
        //This doesn't work for the node
        */
        static TypeStorage()
        {
            StorageContainer = new Dictionary<string, HashSet<AdvShieldProjector>>();
            /*
            ShieldStorageContainer = new Dictionary<string, HashSet<ShieldHardpoint>>();
            ShieldNodeContainer = new Dictionary<string, HashSet<ShieldNode>>();
            ShieldNodeSetContainer = new Dictionary<string, HashSet<ShieldNodeSet>>();
            */
            //ShieldNodeSetContainer = new DictionaryOfTypesWithConstructor<HashSet<ShieldNodeSet>>();
        }

        public static HashSet<AdvShieldProjector> GetObjects()
        {
            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var storage))
                return storage;
            else
                return new HashSet<AdvShieldProjector>();
        }
        /*
        public static HashSet<ShieldHardpoint> GetShieldObjects()
        {
            if (ShieldStorageContainer.TryGetValue(typeof(ShieldHardpoint).FullName, out var storage))
                return storage;
            else
                return new HashSet<ShieldHardpoint>();
        }

        public static HashSet<ShieldNode> GetShieldNode()
        {
            if (ShieldNodeContainer.TryGetValue(typeof(ShieldNode).FullName, out var storage))
                return storage;
            else
                return new HashSet<ShieldNode>();
        }
        /*
        public static HashSet<ShieldNodeSet> GetShieldNodeSet()
        {
                return new HashSet<ShieldNodeSet>();
        }
        */
        /*
        public static HashSet<ShieldNodeSet> GetShieldNodeSet()
        {
            if (ShieldNodeSetContainer.TryGetValue(typeof(ShieldNodeSet).FullName, out var storage))
                return storage;
            else
                return new HashSet<ShieldNodeSet>();
        }
        */
        public static void AddProjector(AdvShieldProjector newValue)
        {            
            HashSet<AdvShieldProjector> storage;

            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var value))
            {
                storage = value;
            }
            else
            {
                storage = new HashSet<AdvShieldProjector>();
                StorageContainer.Add(typeof(AdvShieldProjector).FullName, storage);
            }

            storage.Add(newValue);
        }


        public static void RemoveProjector(AdvShieldProjector oldValue)
        {
            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var value))
            {
                HashSet<AdvShieldProjector> storage = value;

                if (storage.Contains(oldValue))
                    storage.Remove(oldValue);
            }
        }
        /*
        public static void AddHardpoint(ShieldHardpoint newValue)
        {
            HashSet<ShieldHardpoint> storage;

            if (ShieldStorageContainer.TryGetValue(typeof(ShieldHardpoint).FullName, out var value))
            {
                storage = value;
            }
            else
            {
                storage = new HashSet<ShieldHardpoint>();
                ShieldStorageContainer.Add(typeof(ShieldHardpoint).FullName, storage);
            }

            storage.Add(newValue);
        }
        public static void RemoveHardpoint(ShieldHardpoint oldValue)
        {
            if (ShieldStorageContainer.TryGetValue(typeof(ShieldHardpoint).FullName, out var value))
            {
                HashSet<ShieldHardpoint> storage = value;

                if (storage.Contains(oldValue))
                    storage.Remove(oldValue);
            }
        }
        public static void AddShieldNode(ShieldNode newValue)
        {
            HashSet<ShieldNode> storage;
            if (ShieldNodeContainer.TryGetValue(typeof(ShieldNode).FullName, out var value))
            {
                storage = value;
            }
            else
            {
                storage = new HashSet<ShieldNode>();
                ShieldNodeContainer.Add(typeof(ShieldNode).FullName, storage);
            }

            storage.Add(newValue);
        }
        
        public static void AddShieldNodeSet(ShieldNodeSet newValue)
        {
            HashSet<ShieldNodeSet> storage;
            if (ShieldNodeSetContainer.TryGetValue(typeof(ShieldNodeSet).FullName, out var value))
            {
                storage = value;
            }
            else
            {
                storage = new HashSet<ShieldNodeSet>();
                ShieldNodeSetContainer.Add(typeof(ShieldNodeSet).FullName, storage);
            }

            storage.Add(newValue);
        }
        public static void RemoveShieldNodeSet(ShieldNodeSet oldValue)
        {
            if (ShieldNodeSetContainer.TryGetValue(typeof(ShieldNodeSet).FullName, out var value))
            {
                HashSet<ShieldNodeSet> storage = value;

                if (storage.Contains(oldValue))
                    storage.Remove(oldValue);
            }
        }
        */
    }
}
