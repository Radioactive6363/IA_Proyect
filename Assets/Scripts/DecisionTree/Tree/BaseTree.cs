using UnityEngine;

public abstract class BaseTree : MonoBehaviour
{
    protected ITreeNode _rootNode;

    protected virtual void Start()
    {
        CreateTree();
    }

    protected abstract void CreateTree();

    protected virtual void Update()
    {
        _rootNode?.Execute();
    }
}
