using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class BTree : MonoBehaviour
    {

        private Node root = null;

        private Dictionary<string, object> dataContext = new Dictionary<string, object>();

        protected void Start()
        {
            root = SetupTree();
        }

        private void Update()
        {
            if (root != null)
                root.Evaluate();
        }

        protected abstract Node SetupTree();

        public void SetData(string key, object value)
        {
            dataContext[key] = value;
        }

        public object GetData(string key)
        {
            object val;
            dataContext.TryGetValue(key, out val);
            return val;
        }

        public bool ClearData(string key)
        {
            bool cleared = false;
            if (dataContext.ContainsKey(key))
            {
                dataContext.Remove(key);
                return true;
            }
            return cleared;
        }

    }

}