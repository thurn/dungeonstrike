using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class OakBehavior : MonoBehaviour
{
    [Obsolete("Use Name instead.", true)]
    public new string name;

    [Obsolete("Use HideFlags instead.", true)]
    public new HideFlags hideFlags;

    [Obsolete("Use GetOakComponent<SomeComponent> instead.", true)]
    public new GameObject gameObject;

    [Obsolete("Use Tag instead.", true)]
    public new string tag;

    [Obsolete("Use GetOakComponent<ITransform> instead.", true)]
    public new Transform transform;

    [Obsolete("Use Enabled instead.", true)]
    public new bool enabled;

    [Obsolete("Use IsActiveAndEnabled instead.", true)]
    public new bool isActiveAndEnabled;

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T GetComponent<T>()
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component GetComponent(Type type)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component GetComponent(string type)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component GetComponentInChildren(Type t)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component GetComponentInParent(Type t)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T GetComponentInParent<T>()
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component[] GetComponents(Type type)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component[] GetComponentsInChildren(Type t, bool includeInactive = false)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T[] GetComponentsInChildren<T>(bool includeInactive)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T[] GetComponentsInChildren<T>()
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new Component[] GetComponentsInParent(Type t, bool includeInactive = false)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T[] GetComponentsInParent<T>(bool includeInactive)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use GetOakComponent() instead.", true)]
    public new T[] GetComponentsInParent<T>()
    {
        throw new NotImplementedException();
    }
}

public interface ITransform
{
    int ChildCount { get; }
    Vector3 EulerAngles { get; set; }
    Vector3 Forward { get; }
    bool HasChanged { get; set; }
    int HierarchyCapacity { get; }
    int HierarchyCount { get; }
    Vector3 LocalEulerAngles { get; set; }
    Vector3 LocalPosition { get; set; }
    Quaternion LocalRotation { get; set; }
    Vector3 LocalScale { get; set; }
    Matrix4x4 LocalToWorldMatrix { get; }
    Vector3 LossyScale { get; }
    ITransform Parent { get; }
    Vector3 Right { get; }
    ITransform Root { get; }
    int SiblingIndex { get; set; }
    Vector3 Up { get; }
    Vector3 WorldPosition { get; set; }
    Quaternion WorldRotation { get; set; }

    void DetachChildren();
    ITransform Find(string name);
    ITransform GetChild(int index);
    Vector3 InverseTransformDirection(Vector3 direction);
    Vector3 InverseTransformPoint(Vector3 position);
    Vector3 InverseTransformVector(Vector3 vector);
    bool IsChildOf(ITransform parent);
    void LookAt(Transform target);
    void LookAt(Transform target, Vector3 worldUp);
    void Rotate(Vector3 eulerAngles, Space relativeTo = Space.Self);
    void RotateAround(Vector3 point, Vector3 axis, float angle);
    void SetAsFirstSibling();
    void SetAsLastSibling();
    void SetParent(Transform parent, bool worldPositionStays = true);
    Vector3 TransformDirection(Vector3 direction);
    Vector3 TransformPoint(Vector3 position);
    Vector3 TransformVector(Vector3 vector);
    void Translate(Vector3 translation, Space relativeTo = Space.Self);
}



public class Test : OakBehavior {

	// Use this for initialization
	void Start ()
	{
	}

	// Update is called once per frame
	void Update () {
		
	}
}
