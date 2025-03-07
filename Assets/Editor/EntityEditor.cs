/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityPrefab))]
public class EntityPrefabEditor : Editor
{
    private EntityPrefab entityPrefab;

    private void OnEnable()
    {
        entityPrefab = (EntityPrefab)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);

        // Отображаем список ITag
        for (int i = 0; i < entityPrefab.entity.tags.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            var tag = entityPrefab.entity.tags[i];
            if (tag != null)
            {
                EditorGUILayout.LabelField(tag.GetType().Name);
                DrawTagFields(tag);
            }

            // Кнопка для удаления ITag
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                entityPrefab.entity.tags.RemoveAt(i);
                MarkPrefabDirty(entityPrefab);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        // Кнопка для добавления нового ITag
        if (GUILayout.Button("Add Tag"))
        {
            var types = TypeCache.GetTypesDerivedFrom<ITag>();
            var menu = new GenericMenu();

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    var tag = System.Activator.CreateInstance(type) as ITag;
                    entityPrefab.entity.tags.Add(tag);
                    MarkPrefabDirty(entityPrefab);
                });
            }

            menu.ShowAsContext();
        }
    }

    private void DrawTagFields(object tag)
    {
        if (tag == null) return;

        var fields = tag.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(tag);
            if (value is int intValue)
            {
                field.SetValue(tag, EditorGUILayout.IntField(field.Name, intValue));
            }
            else if (value is float floatValue)
            {
                field.SetValue(tag, EditorGUILayout.FloatField(field.Name, floatValue));
            }
            else if (value is string stringValue)
            {
                field.SetValue(tag, EditorGUILayout.TextField(field.Name, stringValue));
            }
            // Добавьте другие типы, если нужно
        }
    }

    private void MarkPrefabDirty(EntityPrefab entityPrefab)
    {
        // Помечаем объект как "грязный"
        EditorUtility.SetDirty(entityPrefab);

        // Уведомляем Unity о изменениях в префабе
        PrefabUtility.RecordPrefabInstancePropertyModifications(entityPrefab);
        Debug.Log("MakeDirty!");
    }
}*/