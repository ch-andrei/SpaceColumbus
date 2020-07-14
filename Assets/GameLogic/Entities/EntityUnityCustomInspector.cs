using UnityEngine;
using UnityEditor;

using EntitySelection;
using UnityEditor.UIElements;

namespace Entities
{
    // [CustomEditor(typeof(Entity))]
    // [CanEditMultipleObjects]
    // public class EntityUnityCustomInspector : Editor
    // {
    //     public GameObject selectableObject;
    //
    //     public override void OnInspectorGUI()
    //     {
    //         Debug.Log("ENTITY INSPECTOR");
    //
    //         DrawDefaultInspector();
    //
    //         serializedObject.Update();
    //
    //         var entity = target as Entity;
    //
    //         var selectable = EntityManager.GetComponent<Selectable>(entity);
    //         if (selectable != null)
    //         {
    //             GameObject go;
    //         }
    //
    //         serializedObject.ApplyModifiedProperties();
    //     }
    // }
}
