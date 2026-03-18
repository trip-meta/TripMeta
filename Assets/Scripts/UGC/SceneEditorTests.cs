using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TripMeta.UGC;

namespace TripMeta.Tests.UGC
{
    /// <summary>
    /// UGC场景编辑器单元测试
    /// </summary>
    public class SceneEditorTests
    {
        private GameObject testObject;
        private SceneEditorManager editor;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestSceneEditor");
            editor = testObject.AddComponent<SceneEditorManager>();
            editor.scenesSavePath = "TestScenes/";
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator SceneEditor_CreateNewScene_CreatesValidScene()
        {
            yield return null;

            var scene = editor.CreateNewScene("TestScene", new Vector3(100, 50, 100));

            Assert.IsNotNull(scene);
            Assert.AreEqual("TestScene", scene.name);
            Assert.IsFalse(string.IsNullOrEmpty(scene.sceneId));
            Assert.IsNotNull(scene.objects);
            Assert.AreEqual(new Vector3(100, 50, 100), scene.size);
        }

        [UnityTest]
        public IEnumerator SceneEditor_AddObject_AddsToScene()
        {
            yield return null;

            editor.CreateNewScene("TestScene", Vector3.one * 100);
            var obj = editor.AddObject("tree_01", Vector3.zero, Quaternion.identity, Vector3.one);

            Assert.IsNotNull(obj);
            Assert.IsFalse(string.IsNullOrEmpty(obj.objectId));
            Assert.AreEqual("tree_01", obj.prefabId);
            Assert.AreEqual(1, editor.CurrentScene.objects.Count);
        }

        [UnityTest]
        public IEnumerator SceneEditor_SelectObject_SelectsCorrectly()
        {
            yield return null;

            editor.CreateNewScene("TestScene", Vector3.one * 100);
            var obj = editor.AddObject("rock_01", Vector3.zero, Quaternion.identity, Vector3.one);

            bool selectionChanged = false;
            editor.OnSelectionChanged += (objs) => selectionChanged = true;

            editor.SelectObject(obj);

            Assert.AreEqual(1, editor.SelectedObjects.Count);
            Assert.IsTrue(selectionChanged);
            Assert.IsTrue(obj.IsSelected);
        }

        [UnityTest]
        public IEnumerator SceneEditor_SelectObject_AdditiveSelect()
        {
            yield return null;

            editor.CreateNewScene("TestScene", Vector3.one * 100);
            var obj1 = editor.AddObject("tree_01", Vector3.zero, Quaternion.identity, Vector3.one);
            var obj2 = editor.AddObject("tree_02", Vector3.one, Quaternion.identity, Vector3.one);

            editor.SelectObject(obj1);
            editor.SelectObject(obj2, additive: true);

            Assert.AreEqual(2, editor.SelectedObjects.Count);
        }

        [UnityTest]
        public IEnumerator SceneEditor_DeleteSelectedObjects_RemovesFromScene()
        {
            yield return null;

            editor.CreateNewScene("TestScene", Vector3.one * 100);
            var obj = editor.AddObject("bush_01", Vector3.zero, Quaternion.identity, Vector3.one);

            editor.SelectObject(obj);
            editor.DeleteSelectedObjects();

            Assert.AreEqual(0, editor.SelectedObjects.Count);
            Assert.AreEqual(0, editor.CurrentScene.objects.Count);
        }

        [UnityTest]
        public IEnumerator SceneEditor_SnapPosition_SnapsCorrectly()
        {
            yield return null;

            editor.snapMode = SnapMode.Grid;
            editor.snapGridSize = 1f;

            Vector3 rawPos = new Vector3(1.7f, 2.3f, 3.1f);
            Vector3 snappedPos = editor.GetSnappedPosition(rawPos);

            Assert.AreEqual(2f, snappedPos.x);
            Assert.AreEqual(2f, snappedPos.y);
            Assert.AreEqual(3f, snappedPos.z);
        }

        [UnityTest]
        public IEnumerator SceneEditor_SnapRotation_SnapsCorrectly()
        {
            yield return null;

            editor.snapMode = SnapMode.Grid;
            editor.snapAngle = 15f;

            Quaternion rawRot = Quaternion.Euler(23f, 47f, 89f);
            Quaternion snappedRot = editor.GetSnappedRotation(rawRot);

            Assert.AreEqual(15f, snappedRot.eulerAngles.x, 0.1f);
            Assert.AreEqual(45f, snappedRot.eulerAngles.y, 0.1f);
            Assert.AreEqual(90f, snappedRot.eulerAngles.z, 0.1f);
        }

        [Test]
        public void SceneEditor_CloneScene_CreatesIndependentCopy()
        {
            var original = new EditableScene
            {
                name = "Original",
                objects = new System.Collections.Generic.List<SceneObject>
                {
                    new SceneObject { prefabId = "tree", position = Vector3.zero }
                }
            };

            var clone = original.Clone();

            Assert.AreEqual(original.name, clone.name);
            Assert.AreEqual(original.objects.Count, clone.objects.Count);

            // 修改克隆不影响原始
            clone.name = "Clone";
            clone.objects[0].position = Vector3.one;

            Assert.AreEqual("Original", original.name);
            Assert.AreEqual(Vector3.zero, original.objects[0].position);
        }

        [Test]
        public void SceneObject_Clone_CreatesIndependentCopy()
        {
            var original = new SceneObject
            {
                prefabId = "rock",
                position = Vector3.zero,
                customProperties = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "color", "red" }
                }
            };

            var clone = original.Clone();

            Assert.AreEqual(original.prefabId, clone.prefabId);
            Assert.AreEqual(original.position, clone.position);

            // 修改克隆不影响原始
            clone.customProperties["color"] = "blue";

            Assert.AreEqual("red", original.customProperties["color"]);
        }

        [UnityTest]
        public IEnumerator SceneEditor_SetEditMode_ChangesMode()
        {
            yield return null;

            bool modeChanged = false;
            editor.OnEditModeChanged += (mode) => modeChanged = true;

            editor.SetEditMode(EditMode.Terrain);

            Assert.AreEqual(EditMode.Terrain, editor.currentEditMode);
            Assert.IsTrue(modeChanged);
        }

        [Test]
        public void TerrainData_Clone_CreatesDeepCopy()
        {
            var original = new TerrainData
            {
                size = Vector3.one * 100,
                heightmap = new float[,] { { 0.1f, 0.2f }, { 0.3f, 0.4f } }
            };

            var clone = original.Clone();

            Assert.AreEqual(original.size, clone.size);
            Assert.AreEqual(original.heightmap[0, 0], clone.heightmap[0, 0]);

            // 修改克隆不影响原始
            clone.heightmap[0, 0] = 0.9f;

            Assert.AreEqual(0.1f, original.heightmap[0, 0]);
        }

        [UnityTest]
        public IEnumerator SceneEditor_HasUnsavedChanges_SetCorrectly()
        {
            yield return null;

            Assert.IsFalse(editor.HasUnsavedChanges);

            editor.CreateNewScene("TestScene", Vector3.one * 100);

            Assert.IsFalse(editor.HasUnsavedChanges); // 新场景不算未保存

            editor.AddObject("tree_01", Vector3.zero, Quaternion.identity, Vector3.one);

            Assert.IsTrue(editor.HasUnsavedChanges);
        }

        [UnityTest]
        public IEnumerator SceneEditor_LoadScene_LoadsCorrectly()
        {
            yield return null;

            var scene = new EditableScene
            {
                sceneId = "test-id",
                name = "LoadedScene",
                objects = new System.Collections.Generic.List<SceneObject>()
            };

            bool loaded = false;
            editor.OnSceneLoaded += (s) => loaded = true;

            editor.LoadScene(scene);

            Assert.AreEqual(scene, editor.CurrentScene);
            Assert.IsTrue(loaded);
        }
    }
}
