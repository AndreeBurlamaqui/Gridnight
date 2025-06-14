using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    private readonly Dictionary<Type, EntityModule> attachedModules = new();

    private void Awake()
    {
        // Get every module attached to this entity
        var attachedModules = GetComponentsInChildren<EntityModule>();
        for(int a = 0; a < attachedModules.Length; a++)
        {
            AttachModule(attachedModules[a]);
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        Initiate();
    }

    protected virtual void Initiate()
    {
        // Fix the position of this entity on initiate
        WorldGrid.Instance.Spawn(this, transform.position);
    }

    protected virtual void Terminate()
    {

    }

    public bool HasModule<T>() where T : EntityModule
    {
        return attachedModules.ContainsKey(typeof(T));
    }

    public bool TryGetModule<T>(out T module) where T : EntityModule
    {
        if (attachedModules.TryGetValue(typeof(T), out EntityModule rawModule))
        {
            module = rawModule as T;
            return module != null;
        }

        module = null;
        return false;
    }

    public void AttachModule(EntityModule module)
    {
        var type = module.GetType();
        if (attachedModules.ContainsKey(type))
        {
            Debug.Log($"Entity {gameObject.name} already has module of type {type.FullName}");
            return;
        }

        Debug.Log($"Attaching module of type {type.FullName} in entity {gameObject.name}");
        attachedModules[type] = module;
        module.Initiate(this);
    }

    public void DetachModule(EntityModule targetModule)
    {
        var targetType = targetModule.GetType();
        if (attachedModules.TryGetValue(targetType, out var detached)){
            detached.Terminate();
        }
        attachedModules.Remove(targetType);
    }

    public virtual void Hit(BaseEntity otherEntity)
    {
        Debug.Log($"Entity {gameObject.name} touched {otherEntity.name}");
        if(otherEntity.TryGetModule(out HealthModule health))
        {
            health.DamageBy(1);
        }

        if (TryGetModule(out MovementModule movement))
        {
            movement.HitMove(otherEntity.transform.position);
        }
    }
}
