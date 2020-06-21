using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Common
{
    // public abstract class Singleton<T> where T : class
    // {
    //     public static T Instance => sInstance.Value;
    //     private static readonly Lazy<T> sInstance = new Lazy<T>(() => CreateInstanceOfT());
    //     private static T CreateInstanceOfT() => Activator.CreateInstance(typeof(T), true) as T;
    // }

    public interface IUpdateable
    {
        void Update();
    }

    public interface IDestroyable
    {
        void OnDestroy();
    }

    public interface INamed
    {
        string Name { get; }
    }

    public interface ICloneable<T>
    {
        T Clone();
    }

    public interface IWithPosition
    {
        Vector3 Position { get; }
    }

    public interface IWithPosition2d
    {
        Vector2 Position2d { get; }
    }

    public interface IIdentifiable
    {
        int Guid { get; }
    }
}
