using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    public class EntityModelBindingT<IdType, EntityType, ModelType> where ModelType : MonoBehaviour
    {
        private class Entry
        {
            public EntityType Entity { get; set; }
            public IdType Id { get; set; }
            public ModelType Model { get; set; }
        }

        private readonly Dictionary<EntityType, Entry> _entityEntryTable = new();
        private readonly Dictionary<IdType, Entry> _idEntryTable = new();
        private readonly Dictionary<ModelType, Entry> _modelEntryTable = new();

        public IEnumerable<EntityType> Entities => _entityEntryTable.Keys;
        public IEnumerable<ModelType> Models => _modelEntryTable.Keys;

        public void Bind(IdType id, EntityType entity, ModelType model)
        {
            if (model == default)
            {
                Debug.LogWarning($"{nameof(EntityModelBindingT<IdType, EntityType, ModelType>)}> Bind 실패: 모델이 비어 있다");
                return;
            }

            WarnIfBindingExists(id, entity, model);
            UnbindExistingByEntity(entity);
            UnbindExistingById(id);
            UnbindExistingByModel(model);

            var entry = new Entry()
            {
                Entity = entity,
                Id = id,
                Model = model,
            };

            _entityEntryTable[entity] = entry;
            _idEntryTable[id] = entry;
            _modelEntryTable[model] = entry;
        }

        public void Clear()
        {
            foreach (var model in Models)
                model.DestroyObject();

            _entityEntryTable.Clear();
            _idEntryTable.Clear();
            _modelEntryTable.Clear();
        }

        public bool HasEntity(EntityType entity)
        {
            return _entityEntryTable.ContainsKey(entity);
        }

        public void Remove(ModelType model, float delay = 0, bool destroyModel = true)
        {
            if (model == default)
                return;

            if (_modelEntryTable.TryGetValue(model, out var entry) == false)
                return;

            RemoveEntry(entry);

            if (destroyModel)
            {
                if (delay > 0)
                    model.DestroyObject(delay);
                else
                    model.DestroyObject();
            }
        }

        private void RemoveEntry(Entry entry)
        {
            _entityEntryTable.Remove(entry.Entity);
            _idEntryTable.Remove(entry.Id);
            _modelEntryTable.Remove(entry.Model);
        }

        public bool TryGetEntity(ModelType model, out EntityType entity)
        {
            if (_modelEntryTable.TryGetValue(model, out var entry))
            {
                entity = entry.Entity;
                return true;
            }
            else
            {
                entity = default;
                return false;
            }
        }

        public bool TryGetModel(IdType id, out ModelType model)
        {
            if (_idEntryTable.TryGetValue(id, out var entry))
            {
                model = entry.Model;
                return true;
            }
            else
            {
                model = default;
                return false;
            }
        }

        private void UnbindExistingByEntity(EntityType entity)
        {
            if (_entityEntryTable.TryGetValue(entity, out var entry))
                RemoveEntry(entry);
        }

        private void UnbindExistingById(IdType id)
        {
            if (_idEntryTable.TryGetValue(id, out var entry))
                RemoveEntry(entry);
        }

        private void UnbindExistingByModel(ModelType model)
        {
            if (_modelEntryTable.TryGetValue(model, out var entry))
                RemoveEntry(entry);
        }

        private void WarnIfBindingExists(IdType id, EntityType entity, ModelType model)
        {
            if (_idEntryTable.ContainsKey(id))
                Debug.LogWarning($"{nameof(EntityModelBindingT<IdType, EntityType, ModelType>)}> Bind 경고: 같은 ID가 이미 있다");

            if (_entityEntryTable.ContainsKey(entity))
                Debug.LogWarning($"{nameof(EntityModelBindingT<IdType, EntityType, ModelType>)}> Bind 경고: 같은 Entity가 이미 있다");

            if (_modelEntryTable.ContainsKey(model))
                Debug.LogWarning($"{nameof(EntityModelBindingT<IdType, EntityType, ModelType>)}> Bind 경고: 같은 Model이 이미 있다");
        }
    }
}
