using System.Collections.Generic;

namespace CloudExplorer.Models
{
    public class Data : Dictionary<string, object> { }

    public class DataNode<T>
    {
        public T Data { get; set; }
        public bool IsLeaf { get; set; }
        public List<DataNode<T>> Children { get; set; } = new List<DataNode<T>>();
        public DataNode(T data)
        {
            Data = data;
            IsLeaf = false;
            Children = new List<DataNode<T>>();
        }
    }
}
